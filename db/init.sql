-- Create the database if it doesn't exist
CREATE DATABASE smarthome;

-- Connect to the database
\c smarthome;

-- Create the sensors table
CREATE TABLE IF NOT EXISTS sensors (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    type VARCHAR(50) NOT NULL,
    location VARCHAR(100) NOT NULL,
    value FLOAT DEFAULT 0,
    unit VARCHAR(20),
    status VARCHAR(20) NOT NULL DEFAULT 'inactive',
    last_updated TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW(),
    created_at TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT NOW()
);

-- Create indexes for common queries
CREATE INDEX IF NOT EXISTS idx_sensors_type ON sensors(type);
CREATE INDEX IF NOT EXISTS idx_sensors_location ON sensors(location);
CREATE INDEX IF NOT EXISTS idx_sensors_status ON sensors(status);

-- =============================================================================
-- Database-per-service: each microservice below owns its own database in
-- this same Postgres instance (see docs/c4/container-to-be.puml). This
-- script only runs once, against a fresh volume, so CREATE DATABASE here
-- (without IF NOT EXISTS, which Postgres doesn't support for CREATE
-- DATABASE) is safe - same pattern as the smarthome database above.
-- =============================================================================

-- Device Management Service ---------------------------------------------
CREATE DATABASE device_management;
\c device_management;

CREATE TABLE IF NOT EXISTS device_types (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    unit VARCHAR(20) NOT NULL DEFAULT '',
    description VARCHAR(255) NOT NULL DEFAULT ''
);

CREATE TABLE IF NOT EXISTS modules (
    id UUID PRIMARY KEY,
    device_type_id UUID NOT NULL REFERENCES device_types(id),
    name VARCHAR(100) NOT NULL,
    manufacturer VARCHAR(100) NOT NULL DEFAULT '',
    protocol VARCHAR(50) NOT NULL DEFAULT '',
    price NUMERIC(10, 2) NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS devices (
    id UUID PRIMARY KEY,
    module_id UUID NOT NULL REFERENCES modules(id),
    house_id UUID NOT NULL,
    serial_number VARCHAR(100) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'pending',
    installed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_modules_device_type_id ON modules(device_type_id);
CREATE INDEX IF NOT EXISTS idx_devices_module_id ON devices(module_id);
CREATE INDEX IF NOT EXISTS idx_devices_house_id ON devices(house_id);

-- Heating Control Service -------------------------------------------------
CREATE DATABASE heating;
\c heating;

CREATE TABLE IF NOT EXISTS heating_states (
    device_id UUID PRIMARY KEY,
    desired_value VARCHAR(50) NOT NULL DEFAULT 'off',
    actual_value VARCHAR(50) NOT NULL DEFAULT 'off',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Lighting Control Service -------------------------------------------------
CREATE DATABASE lighting;
\c lighting;

CREATE TABLE IF NOT EXISTS lighting_states (
    device_id UUID PRIMARY KEY,
    desired_value VARCHAR(50) NOT NULL DEFAULT 'off',
    actual_value VARCHAR(50) NOT NULL DEFAULT 'off',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Access Control Service -------------------------------------------------
CREATE DATABASE access_control;
\c access_control;

CREATE TABLE IF NOT EXISTS access_states (
    device_id UUID PRIMARY KEY,
    desired_value VARCHAR(50) NOT NULL DEFAULT 'locked',
    actual_value VARCHAR(50) NOT NULL DEFAULT 'locked',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS access_audit_log (
    id UUID PRIMARY KEY,
    device_id UUID NOT NULL,
    action VARCHAR(50) NOT NULL,
    actor VARCHAR(100) NOT NULL,
    occurred_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_access_audit_device_id ON access_audit_log(device_id);

-- Monitoring Service -------------------------------------------------------
CREATE DATABASE monitoring;
\c monitoring;

CREATE TABLE IF NOT EXISTS live_states (
    device_id UUID PRIMARY KEY,
    house_id UUID NOT NULL,
    value VARCHAR(100) NOT NULL DEFAULT '',
    status VARCHAR(50) NOT NULL DEFAULT '',
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_live_states_house_id ON live_states(house_id);

-- Telemetry Service --------------------------------------------------------
CREATE DATABASE telemetry;
\c telemetry;

CREATE TABLE IF NOT EXISTS telemetry_points (
    id UUID PRIMARY KEY,
    device_id UUID NOT NULL,
    metric VARCHAR(50) NOT NULL,
    value DOUBLE PRECISION NOT NULL,
    unit VARCHAR(20) NOT NULL DEFAULT '',
    measured_at TIMESTAMPTZ NOT NULL,
    received_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- Serves "latest N for this device/metric" reads.
CREATE INDEX IF NOT EXISTS idx_telemetry_points_device_metric_measured_at
    ON telemetry_points (device_id, metric, measured_at DESC);

CREATE TABLE IF NOT EXISTS threshold_rules (
    id UUID PRIMARY KEY,
    house_id UUID NOT NULL,
    device_id UUID NULL,
    metric VARCHAR(50) NOT NULL,
    operator VARCHAR(10) NOT NULL,
    value DOUBLE PRECISION NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_threshold_rules_house_id ON threshold_rules(house_id);

-- User Identity Service ------------------------------------------------
CREATE DATABASE identity;
\c identity;

CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS houses (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    address VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE INDEX IF NOT EXISTS idx_houses_user_id ON houses(user_id);

-- Billing Service ------------------------------------------------------
CREATE DATABASE billing;
\c billing;

CREATE TABLE IF NOT EXISTS subscriptions (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    plan VARCHAR(50) NOT NULL,
    status VARCHAR(20) NOT NULL DEFAULT 'active',
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS idx_subscriptions_user_id ON subscriptions(user_id);
