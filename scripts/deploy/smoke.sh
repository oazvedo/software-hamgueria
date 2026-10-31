#!/bin/sh
# Fumaça da pilha de produção (spec 004: CA-03, CA-06 a CA-09; spec 005: CA-02 a CA-04, CA-07, CA-09), rodada localmente.
# Sobe docker-compose.prod.yml com um projeto e um .env próprios (porta HTTP 8088), confere e derruba tudo,
# volumes inclusos. Não toca na pilha de produção nem nos dados de dev.
# Uso (da raiz do repo): sh scripts/deploy/smoke.sh
set -eu

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
TMP=$(mktemp -d)
PORT=${SMOKE_HTTP_PORT:-8088}
BASE="http://localhost:$PORT"
ADMIN_PASSWORD="fumaca-senha-forte"
erros=0

cat >"$TMP/.env" <<EOF
SITE_ADDRESS=:80
HTTP_PORT=$PORT
HTTPS_PORT=${SMOKE_HTTPS_PORT:-8443}
POSTGRES_PASSWORD=$(openssl rand -hex 16)
RABBITMQ_USER=hamgueria
RABBITMQ_PASSWORD=$(openssl rand -hex 16)
AUTH_ADMIN_USERNAME=cozinha
AUTH_ADMIN_PASSWORD=$ADMIN_PASSWORD
AUTH_JWT_SECRET=$(openssl rand -hex 32)
EOF

dc() { docker compose -p hamgueria-smoke -f "$ROOT/docker-compose.prod.yml" --env-file "$TMP/.env" "$@"; }
limpa() { dc down -v --remove-orphans >/dev/null 2>&1 || true; rm -rf "$TMP"; }
trap limpa EXIT

ok() { echo "OK    $1"; }
falha() { echo "FALHA $1"; erros=$((erros + 1)); }
# confere <descrição> <comando...>
confere() { d=$1; shift; if "$@" >/dev/null 2>&1; then ok "$d"; else falha "$d"; fi; }
status() { curl -s -o /dev/null -w '%{http_code}' "$@"; }
# ate <segundos> <descrição> <comando...>: OK quando o comando passar dentro do prazo
ate() {
  prazo=$1 d=$2; shift 2; i=0
  until "$@" >/dev/null 2>&1; do
    i=$((i + 1)); if [ $i -gt "$prazo" ]; then falha "$d"; return 0; fi; sleep 1
  done
  ok "$d"
}
espera_saude() {
  i=0
  until [ "$(curl -s "$BASE/api/health")" = "Healthy" ]; do
    i=$((i + 1)); [ $i -gt 90 ] && return 1; sleep 2
  done
}

echo "== subindo a pilha (build pode levar alguns minutos)"
dc up -d --build --wait >/dev/null 2>&1
confere "CA-09: /api/health responde Healthy pelo web" espera_saude

# CA-03: tela e cardápio real pelo endereço público
confere "CA-03: a tela abre" sh -c "curl -sf '$BASE/' | grep -q 'id=\"root\"'"
confere "CA-03: rota da SPA cai no index.html" sh -c "curl -sf '$BASE/producao' | grep -q 'id=\"root\"'"
confere "CA-03: cardápio real (semeado) em /api/dish" sh -c "curl -sf '$BASE/api/dish' | grep -q '\"isActive\"'"
login() { curl -s -o /dev/null -w '%{http_code}' -H 'content-type: application/json' \
  -d "{\"username\":\"cozinha\",\"password\":\"$1\"}" "$BASE/api/auth/login"; }
confere "CA-03: a cozinha entra com a senha do .env" test "$(login "$ADMIN_PASSWORD")" = 200
confere "CA-03: a senha de dev não entra" test "$(login admin123)" = 401

# CA-06: superfície pública
confere "CA-06: /swagger não existe em produção" test "$(status "$BASE/swagger/index.html")" = 404
for svc in postgres:5432 rabbitmq:5672 rabbitmq:15672 api:8080; do
  confere "CA-06: ${svc%%:*} sem porta publicada (${svc#*:})" sh -c "! dc port ${svc%%:*} ${svc#*:} 2>/dev/null | grep -q ."
done

# CA-08: pedido real pelo web
dish=$(curl -s "$BASE/api/dish" | grep -o '"id":"[^"]*","name":"[^"]*","description":"[^"]*","price":[0-9.]*,"category":"[^"]*","isActive":true' | head -n1 | sed 's/"id":"\([^"]*\)".*/\1/')
table=$(curl -s "$BASE/api/table" | grep -o '"id":"[^"]*"' | head -n1 | sed 's/"id":"\(.*\)"/\1/')
order=$(curl -s -H 'content-type: application/json' \
  -d "{\"tableId\":\"$table\",\"customerName\":\"Fumaca\",\"items\":[{\"dishId\":\"$dish\",\"quantity\":1}]}" \
  "$BASE/api/order" | grep -o '"id":"[^"]*"' | head -n1 | sed 's/"id":"\(.*\)"/\1/')
confere "CA-08: pedido criado pela tela publicada vai para a API real" test -n "$order"

novo_pedido() {
  [ "$(curl -s -o /dev/null -w '%{http_code}' -H 'content-type: application/json' \
    -d "{\"tableId\":\"$table\",\"customerName\":\"Fumaca\",\"items\":[{\"dishId\":\"$dish\",\"quantity\":1}]}" \
    "$BASE/api/order")" = 201 ]
}

# 005/CA-03, CA-07, CA-04: a fila cai com tudo no ar, pedidos seguem, e os eventos saem quando ela volta
dc stop rabbitmq >/dev/null 2>&1
confere "005/CA-03: com a fila parada, o pedido é aceito" novo_pedido
confere "005/CA-07: com a fila parada, a saúde principal segue Healthy" test "$(curl -s "$BASE/api/health")" = Healthy
ate 30 "005/CA-07: a saúde detalhada mostra a fila fora e eventos esperando" \
  sh -c "curl -s '$BASE/api/health/details' | grep -q '\"queue\":\"Unhealthy\",\"pendingEvents\":[1-9]'"
dc start rabbitmq >/dev/null 2>&1
ate 60 "005/CA-04: a fila volta e os eventos esperando saem sozinhos" \
  sh -c "curl -s '$BASE/api/health/details' | grep -q '\"queue\":\"Healthy\",\"pendingEvents\":0'"

# 004/CA-07 e 005/CA-09: reiniciar tudo de uma vez (como um reboot da instância, sem ordem e sem `up --wait`)
dc restart >/dev/null 2>&1
ate 60 "005/CA-09: depois de reiniciar tudo de uma vez, pedidos voltam sozinhos em até 1 minuto" novo_pedido
confere "CA-07: o pedido continua lá depois do reinício" test "$(status "$BASE/api/order/$order")" = 200
dc up -d --build --force-recreate --wait >/dev/null 2>&1
confere "CA-07: depois de publicar de novo, a pilha volta saudável" espera_saude
confere "CA-07: o pedido continua lá depois da nova versão" test "$(status "$BASE/api/order/$order")" = 200

# CA-09 (falha): banco parado
dc stop postgres >/dev/null 2>&1
confere "CA-09: com o banco parado, /api/health responde 503 Unhealthy" \
  sh -c "[ \"\$(curl -s -w ' %{http_code}' '$BASE/api/health')\" = 'Unhealthy 503' ]"

echo "== derrubando a pilha"
if [ $erros -eq 0 ]; then echo "Fumaça OK"; else echo "$erros falha(s)"; exit 1; fi
