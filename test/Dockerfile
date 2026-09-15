# Runs smoke-test.sh against the rest of the stack over the docker network.
# Not a long-running service - a one-shot container that exits with the
# smoke test's own exit code (0 = all checks passed).
FROM alpine:3.20

RUN apk add --no-cache bash curl

WORKDIR /test
COPY smoke-test.sh .
RUN chmod +x smoke-test.sh

ENTRYPOINT ["bash", "smoke-test.sh"]
