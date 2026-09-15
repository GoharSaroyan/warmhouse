#!/usr/bin/env bash
# End-to-end smoke test, run against the stack after `docker-compose up`.
# Everything goes through the API Gateway (port 5000) - the platform's
# single public entry point (see docs/c4/container-to-be.puml).
#
# Covers:
#   1) Identity: create a user + a house (gives every later step a real houseId).
#   2) Task 5 - "Create Sensor": create a Telemetry-type device via the
#      Device Management catalog.
#   3) Task 5 - "Get All Sensors": GET /api/v1/devices?deviceType=Telemetry
#      returns a *different* generated reading on every call.
#   4) Async pub/sub scoped by houseId: PUT a desired Heating state ->
#      HeatingCommandRequested -> RabbitMQ -> Device Gateway "delivers" it
#      and publishes DeviceStateChanged(deviceId, houseId, ...) -> RabbitMQ
#      -> Monitoring records it as that device's live state. The test polls
#      Monitoring (both by device id and by houseId) until the event has
#      landed, and confirms a different house's houseId does NOT see it.
#
# Every step asserts on the status code and/or payload; the script exits
# non-zero if any assertion fails, so it doubles as a CI gate. Responses are
# still printed as they arrive - the assertions decide the outcome, the
# output shows the real data behind it.
#
# Usage:      bash deploy/smoke-test.sh
# Variables:  GATEWAY_URL, READY_TIMEOUT, EVENT_TIMEOUT
#
# set -e is deliberately absent: a failing assertion has to be recorded and
# the run continued, otherwise the first failure hides every later one.
set -uo pipefail

GW="${GATEWAY_URL:-http://localhost:5000}"
READY_TIMEOUT="${READY_TIMEOUT:-120}"
EVENT_TIMEOUT="${EVENT_TIMEOUT:-45}"

RUN_ID="$(date +%s)-$$"
EMAIL="smoke-$RUN_ID@warmhouse.example"
TELEMETRY_MODULE_ID="22222222-2222-2222-2222-222222222222" # seeded in db/init.sql
# Non-empty but guaranteed not to be the house created below - Guid.Empty
# is rejected outright by Monitoring as "houseId is required", so it can't
# stand in for "a real house that isn't this one".
FOREIGN_HOUSE_ID="99999999-9999-9999-9999-999999999999"

TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT

BOLD='\033[1m'; RED='\033[1;31m'; GREEN='\033[1;32m'; DIM='\033[2m'; OFF='\033[0m'

PASSED=0
FAILED=0
FAILED_NAMES=()

STATUS=""
BODY=""

section() { printf '\n%b== %s%b\n' "$BOLD" "$1" "$OFF"; }

pass() {
    PASSED=$((PASSED + 1))
    printf '  %bok%b   %s\n' "$GREEN" "$OFF" "$1"
}

fail() {
    FAILED=$((FAILED + 1))
    FAILED_NAMES+=("$1")
    printf '  %bFAIL%b %s\n' "$RED" "$OFF" "$1"
    if [ -n "${2:-}" ]; then
        printf '       %b%s%b\n' "$DIM" "$2" "$OFF"
    fi
    return 0
}

# Prints what the service actually returned. Assertions decide pass/fail;
# this is what makes the run also readable as a walkthrough.
show() {
    if [ -n "$BODY" ]; then
        printf '       %b%s%b\n' "$DIM" "$BODY" "$OFF"
    fi
}

request() {
    local method="$1" url="$2" file="${3:-}"
    local out="$TMP/response"
    local args=(-sS --max-time 30 -o "$out" -w '%{http_code}' -X "$method" "$url")

    if [ -n "$file" ]; then
        args+=(-H 'Content-Type: application/json' --data-binary "@$file")
    fi

    STATUS="$(curl "${args[@]}" 2>/dev/null)" || STATUS="000"
    BODY="$(cat "$out" 2>/dev/null)"
    rm -f "$out"
}

assert_status() {
    local expected="$1" name="$2"
    if [ "$STATUS" = "$expected" ]; then
        pass "$name"
    else
        fail "$name" "expected HTTP $expected, got $STATUS: $(printf '%s' "$BODY" | head -c 300)"
    fi
}

assert_body_contains() {
    local needle="$1" name="$2"
    if printf '%s' "$BODY" | grep -qF -- "$needle"; then
        pass "$name"
    else
        fail "$name" "response did not contain '$needle': $(printf '%s' "$BODY" | head -c 300)"
    fi
}

assert_body_not_contains() {
    local needle="$1" name="$2"
    if printf '%s' "$BODY" | grep -qF -- "$needle"; then
        fail "$name" "response unexpectedly contained '$needle': $(printf '%s' "$BODY" | head -c 300)"
    else
        pass "$name"
    fi
}

# First top-level "id" in the response.
id_of() { printf '%s' "$BODY" | grep -o '"id":"[^"]*"' | head -1 | cut -d'"' -f4; }

# The numeric "value" field belonging to the flat JSON object whose "id"
# matches $1, inside an array response (DeviceResponse has no nested
# objects, so "up to the next closing brace" is exactly that object).
value_for_id() {
    printf '%s' "$BODY" \
        | grep -o "\"id\":\"$1\"[^}]*" \
        | grep -o '"value":[0-9.]*' \
        | head -1 \
        | cut -d':' -f2
}

abort() {
    printf '\n%b%s%b\n' "$RED" "$1" "$OFF" >&2
    exit 1
}

# Polls a predicate until it holds or the deadline passes. The pub/sub steps
# depend on RabbitMQ delivery + consumer processing, so a fixed sleep would
# be either flaky or slow.
retry_until() {
    local seconds="$1" name="$2"; shift 2
    local deadline=$(( $(date +%s) + seconds ))

    while :; do
        if "$@"; then
            pass "$name"
            show
            return 0
        fi
        if [ "$(date +%s)" -ge "$deadline" ]; then
            fail "$name" "did not happen within ${seconds}s; last response HTTP $STATUS: $(printf '%s' "$BODY" | head -c 300)"
            return 1
        fi
        sleep 2
    done
}

# --------------------------------------------------------------------------
section "0. Gateway is up"

gateway_ready() {
    request GET "$GW/health"
    [ "$STATUS" = "200" ]
}
retry_until "$READY_TIMEOUT" "gateway responds on /health" gateway_ready \
    || abort "Gateway unreachable at $GW - is the stack up (docker-compose up)?"

# --------------------------------------------------------------------------
section "1. Identity: user and house"

cat > "$TMP/user.json" <<JSON
{"name":"Smoke Test Resident","email":"$EMAIL","password":"smoke-test-password"}
JSON
request POST "$GW/api/v1/identity/users" "$TMP/user.json"
assert_status 201 "POST /identity/users -> 201"
show
USER_ID="$(id_of)"
[ -n "$USER_ID" ] || abort "User was not created; nothing downstream can proceed."

cat > "$TMP/house.json" <<JSON
{"userId":"$USER_ID","address":"1 Smoke Test Ave, Run $RUN_ID"}
JSON
request POST "$GW/api/v1/identity/houses" "$TMP/house.json"
assert_status 201 "POST /identity/houses -> 201"
show
HOUSE_ID="$(id_of)"
[ -n "$HOUSE_ID" ] || abort "House was not created; nothing downstream can proceed."

# --------------------------------------------------------------------------
section "2. Task 5 - Create Sensor"

cat > "$TMP/sensor.json" <<JSON
{"moduleId":"$TELEMETRY_MODULE_ID","houseId":"$HOUSE_ID","serialNumber":"SMOKE-TEMP-$RUN_ID"}
JSON
request POST "$GW/api/v1/devices" "$TMP/sensor.json"
assert_status 201 "POST /devices (Create Sensor) -> 201"
show
assert_body_contains "SMOKE-TEMP-$RUN_ID" "response echoes the serial number"
SENSOR_ID="$(id_of)"
[ -n "$SENSOR_ID" ] || abort "Sensor device was not created; Get All Sensors can't be checked."

# --------------------------------------------------------------------------
section "3. Task 5 - Get All Sensors (different value on every call)"

request GET "$GW/api/v1/devices?deviceType=Telemetry"
assert_status 200 "GET /devices?deviceType=Telemetry -> 200"
assert_body_contains "$SENSOR_ID" "list includes the sensor just created"
FIRST_VALUE="$(value_for_id "$SENSOR_ID")"
if [ -n "$FIRST_VALUE" ]; then
    pass "sensor carries a generated value ($FIRST_VALUE)"
else
    fail "sensor carries a generated value" "no numeric value field found for $SENSOR_ID"
fi

request GET "$GW/api/v1/devices?deviceType=Telemetry"
assert_status 200 "second GET /devices?deviceType=Telemetry -> 200"
SECOND_VALUE="$(value_for_id "$SENSOR_ID")"

if [ -n "$FIRST_VALUE" ] && [ -n "$SECOND_VALUE" ] && [ "$FIRST_VALUE" != "$SECOND_VALUE" ]; then
    pass "value differs between calls ($FIRST_VALUE -> $SECOND_VALUE)"
else
    fail "value differs between calls" "got '$FIRST_VALUE' then '$SECOND_VALUE'"
fi

# --------------------------------------------------------------------------
section "4. Async pub/sub scoped by houseId (Heating -> Device Gateway -> Monitoring)"

cat > "$TMP/heating-type.json" <<JSON
{"name":"Heating-Smoke-$RUN_ID","unit":"","description":"Smoke test heating device type"}
JSON
request POST "$GW/api/v1/devices/types" "$TMP/heating-type.json"
assert_status 201 "POST /devices/types -> 201"
HEATING_TYPE_ID="$(id_of)"
[ -n "$HEATING_TYPE_ID" ] || abort "Heating device type was not created."

cat > "$TMP/heating-module.json" <<JSON
{"deviceTypeId":"$HEATING_TYPE_ID","name":"SmokeTherm","manufacturer":"WarmHouse","protocol":"mqtt","price":49.99}
JSON
request POST "$GW/api/v1/devices/modules" "$TMP/heating-module.json"
assert_status 201 "POST /devices/modules -> 201"
HEATING_MODULE_ID="$(id_of)"
[ -n "$HEATING_MODULE_ID" ] || abort "Heating module was not created."

cat > "$TMP/heating-device.json" <<JSON
{"moduleId":"$HEATING_MODULE_ID","houseId":"$HOUSE_ID","serialNumber":"SMOKE-HEAT-$RUN_ID"}
JSON
request POST "$GW/api/v1/devices" "$TMP/heating-device.json"
assert_status 201 "POST /devices (heating device) -> 201"
HEATING_DEVICE_ID="$(id_of)"
[ -n "$HEATING_DEVICE_ID" ] || abort "Heating device was not created; the pub/sub chain can't be exercised."

cat > "$TMP/heating-desired.json" <<JSON
{"houseId":"$HOUSE_ID","desiredValue":"22"}
JSON
request PUT "$GW/api/v1/heating/$HEATING_DEVICE_ID" "$TMP/heating-desired.json"
assert_status 200 "PUT /heating/{deviceId} -> 200 (publishes HeatingCommandRequested)"
show
assert_body_contains '"desiredValue":"22"' "desired value recorded immediately (synchronous part)"

# The asynchronous part: Device Gateway consumes HeatingCommandRequested,
# "delivers" it, and publishes DeviceStateChanged(deviceId, houseId, ...),
# which Monitoring consumes and records as this device's live state.
live_state_landed() {
    request GET "$GW/api/v1/monitoring/$HEATING_DEVICE_ID"
    [ "$STATUS" = "200" ] && printf '%s' "$BODY" | grep -qF "\"houseId\":\"$HOUSE_ID\""
}
retry_until "$EVENT_TIMEOUT" "Monitoring recorded the live state via async pub/sub" live_state_landed
assert_body_contains '"value":"22"' "live state carries the actual value published by Device Gateway"

request GET "$GW/api/v1/monitoring?houseId=$HOUSE_ID"
assert_status 200 "GET /monitoring?houseId=<this house> -> 200"
assert_body_contains "$HEATING_DEVICE_ID" "house-scoped live-state list includes the device"

request GET "$GW/api/v1/monitoring?houseId=$FOREIGN_HOUSE_ID"
assert_status 200 "GET /monitoring?houseId=<a different house> -> 200"
assert_body_not_contains "$HEATING_DEVICE_ID" "a different house's houseId does not see this device's live state"

# --------------------------------------------------------------------------
TOTAL=$((PASSED + FAILED))
printf '\n%b--------------------------------------------------%b\n' "$BOLD" "$OFF"

if [ "$FAILED" -eq 0 ]; then
    printf '%bAll checks passed: %d of %d.%b\n' "$GREEN" "$PASSED" "$TOTAL" "$OFF"
    exit 0
fi

printf '%b%d of %d failed:%b\n' "$RED" "$FAILED" "$TOTAL" "$OFF"
for name in "${FAILED_NAMES[@]}"; do
    printf '  - %s\n' "$name"
done
exit 1
