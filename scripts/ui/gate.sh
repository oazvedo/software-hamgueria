#!/bin/sh
# Gate de interface (ADR 0015, spec 006): cor, ruído e AI tell (killer-scan), copy (copy-lint) e motion (motion-lint).
# Sai com erro se qualquer detector achar algo. Uso (da raiz): sh scripts/ui/gate.sh [alvo]   (padrão: frontend/src)
# Os códigos de saída dos detectores não servem de veredito: o killer-scan sai 1 mesmo limpo ("GATE INCOMPLETE",
# porque o copy roda à parte) e o motion-lint sai 0 com achados MEDIUM. Por isso o gate lê a saída de cada um.
set -u

ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
SKILLS="$ROOT/.claude/skills"
ALVO=$(cd "$(dirname "${1:-$ROOT/frontend/src}")" && pwd)/$(basename "${1:-$ROOT/frontend/src}")
# Sem isso, um clone sem node_modules faria o npx baixar a última versão do impeccable (CLAUDE.md, ajuste 1).
export npm_config_yes=false
# O impeccable é devDependency do frontend: o npx só o acha rodando de dentro do frontend/.
cd "$ROOT/frontend" || exit 2

falhas=0
saida=$(mktemp)
trap 'rm -f "$saida"' EXIT

node "$SKILLS/visant-killer/scripts/killer-scan.mjs" "$ALVO" >"$saida" 2>&1
if grep -q '^\[FAIL\]' "$saida"; then
  echo "FALHA killer-scan (cor, ruído ou AI tell):"; grep -A40 '^\[FAIL\]' "$saida" | grep -v '^\[OK\]\|^\[SKIPPED\]' | sed -n '1,60p'
  falhas=$((falhas + 1))
elif ! grep -q '^\[OK\].*impeccable' "$saida"; then
  echo "FALHA killer-scan: o impeccable não rodou (falta npm install no frontend?)"; cat "$saida"
  falhas=$((falhas + 1))
else
  echo "OK    killer-scan (cor, ruído, AI tell)"
fi

if node "$SKILLS/visant-copy/scripts/copy-lint.mjs" "$ALVO" >"$saida" 2>&1; then
  echo "OK    copy-lint"
else
  echo "FALHA copy-lint:"; cat "$saida"; falhas=$((falhas + 1))
fi

node "$SKILLS/visant-motion/scripts/motion-lint.mjs" "$ALVO" >"$saida" 2>&1
if grep -q 'motion-lint: clean' "$saida"; then
  echo "OK    motion-lint"
else
  echo "FALHA motion-lint:"; cat "$saida"; falhas=$((falhas + 1))
fi

if [ $falhas -eq 0 ]; then echo "Gate de interface aberto."; else echo "$falhas detector(es) com achados."; exit 1; fi
