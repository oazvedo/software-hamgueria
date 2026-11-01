#!/bin/sh
# Fails if any NuGet package (direct or transitive) has a known vulnerability, and also when the check itself could not
# run (advisory feed down, restore error): passing requires every project to say "has no vulnerable packages" (spec 018).
# Usage (repo root): sh scripts/ci/nuget-vulnerabilities.sh [solution]
# Tests feed a canned output through NUGET_VULN_OUTPUT (a file) and NUGET_VULN_STATUS instead of running dotnet.
set -u
solution=${1:-backend/SoftwareHamgueria.slnx}
if [ -n "${NUGET_VULN_OUTPUT:-}" ]; then
  out=$(cat "$NUGET_VULN_OUTPUT")
  status=${NUGET_VULN_STATUS:-0}
else
  out=$(dotnet list "$solution" package --vulnerable --include-transitive 2>&1)
  status=$?
fi
echo "$out"
if [ "$status" -ne 0 ]; then echo "FALHA: dotnet list saiu com $status"; exit 1; fi
if echo "$out" | grep -Eq "has the following vulnerable packages"; then echo "FALHA: pacote vulnerável"; exit 1; fi
# Only NuGet's own codes: a package or path with "error" in its name must not fail the check.
if echo "$out" | grep -Eq "\b(error|warning) NU[0-9]{4}\b|\bNU190[05]\b"; then
  echo "FALHA: a checagem não conseguiu consultar as vulnerabilidades"; exit 1
fi
projects=$(echo "$out" | grep -c "The given project")
clean=$(echo "$out" | grep -c "has no vulnerable packages")
if [ "$projects" -eq 0 ] || [ "$projects" -ne "$clean" ]; then echo "FALHA: $clean de $projects projetos confirmados sem vulnerabilidade"; exit 1; fi
echo "OK: $clean projetos sem pacote vulnerável"
