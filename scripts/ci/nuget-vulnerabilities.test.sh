#!/bin/sh
# Success and failure cases of nuget-vulnerabilities.sh with canned dotnet output (spec 018, CA-03).
# Usage: sh scripts/ci/nuget-vulnerabilities.test.sh
set -u
SCRIPT="$(cd "$(dirname "$0")" && pwd)/nuget-vulnerabilities.sh"
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
erros=0

# confere <ok|falha> <descrição> <status do dotnet> <saída>
confere() {
  printf '%s\n' "$4" >"$TMP/out"
  if NUGET_VULN_OUTPUT="$TMP/out" NUGET_VULN_STATUS="$3" sh "$SCRIPT" >"$TMP/log" 2>&1; then got=ok; else got=falha; fi
  if [ "$got" = "$1" ]; then echo "ok    $2"; else echo "ERRO  $2 (esperado $1, obtido $got)"; sed 's/^/      /' "$TMP/log"; erros=$((erros + 1)); fi
}

CLEAN="The given project \`A\` has no vulnerable packages given the current sources.
The given project \`Hamgueria.ErrorHandling\` has no vulnerable packages given the current sources."

confere ok "todos os projetos limpos (inclusive um com Error no nome)" 0 "$CLEAN"
confere falha "pacote vulnerável" 0 "Project \`A\` has the following vulnerable packages
   > Newtonsoft.Json      11.0.1     High"
confere falha "feed de vulnerabilidades fora do ar (NU1900)" 0 "warning NU1900: Error occurred while getting package vulnerability data
$CLEAN"
confere falha "dotnet list saiu com erro" 1 "$CLEAN"
confere falha "nenhum projeto confirmado" 0 "Restore failed."

[ $erros -eq 0 ] && echo "nuget-vulnerabilities.test: tudo certo" || { echo "nuget-vulnerabilities.test: $erros erro(s)"; exit 1; }
