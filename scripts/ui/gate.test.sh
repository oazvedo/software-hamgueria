#!/bin/sh
# Casos de sucesso e falha de gate.sh (spec 006, CA-03). Uso: sh scripts/ui/gate.test.sh
set -u

GATE="$(cd "$(dirname "$0")" && pwd)/gate.sh"
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
erros=0

# amostra <nome> <jsx>: um componente com o JSX dado, num diretório próprio
amostra() {
  mkdir -p "$TMP/$1"
  printf 'export function Amostra() {\n  return %s\n}\n' "$2" >"$TMP/$1/Amostra.tsx"
}

# confere <ok|falha> <descrição> <alvo>
confere() {
  if sh "$GATE" "$3" >"$TMP/saida" 2>&1; then obtido=ok; else obtido=falha; fi
  if [ "$obtido" = "$1" ]; then
    echo "ok    $2"
  else
    echo "ERRO  $2 (esperado $1, obtido $obtido)"; sed 's/^/      /' "$TMP/saida"; erros=$((erros + 1))
  fi
}

amostra limpo '<p className="text-muted-foreground transition-colors">Pedido enviado.</p>'
amostra cor '<p className="bg-[#ff5f56]">Pedido enviado.</p>'
amostra seta '<p className="text-muted-foreground">ver detalhe →</p>'
amostra travessao '<p className="text-muted-foreground">Pedido enviado — bom apetite.</p>'
amostra emoji '<p className="text-muted-foreground">Nenhum pedido na fila. 🎉</p>'
amostra motion '<p className="text-muted-foreground transition-all">Pedido enviado.</p>'

confere ok "amostra limpa passa" "$TMP/limpo"
confere falha "cor crua (hex) é reprovada" "$TMP/cor"
confere falha "seta como caractere é reprovada" "$TMP/seta"
confere falha "travessão é reprovado" "$TMP/travessao"
confere falha "emoji é reprovado" "$TMP/emoji"
confere falha "transition-all é reprovado" "$TMP/motion"

[ $erros -eq 0 ] && echo "gate.test: tudo certo" || { echo "gate.test: $erros erro(s)"; exit 1; }
