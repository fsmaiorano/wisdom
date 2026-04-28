#!/bin/sh
set -e

# Start Keycloak in the background
/opt/keycloak/bin/kc.sh start-dev --import-realm &
KC_PID=$!

# Wait until kcadm can authenticate (Keycloak is fully up)
echo "[entrypoint] Waiting for Keycloak to be ready..."
until /opt/keycloak/bin/kcadm.sh config credentials \
  --server http://localhost:8080 \
  --realm master \
  --user "$KC_BOOTSTRAP_ADMIN_USERNAME" \
  --password "$KC_BOOTSTRAP_ADMIN_PASSWORD" 2>/dev/null; do
  sleep 5
done

# Disable HTTPS requirement on both realms so HTTP works locally
echo "[entrypoint] Disabling sslRequired on master and wisdom realms..."
/opt/keycloak/bin/kcadm.sh update realms/master -s sslRequired=none
/opt/keycloak/bin/kcadm.sh update realms/wisdom  -s sslRequired=none 2>/dev/null || true

echo "[entrypoint] Done. Keycloak is ready for local HTTP access."

wait $KC_PID
