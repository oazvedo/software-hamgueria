#!/bin/sh
# Fails if any NuGet package (direct or transitive) has a known vulnerability, and also when the check itself could not
# run (advisory feed down, restore error): passing requires every project to say "has no vulnerable packages" (spec 018).
# Usage (repo root): sh scripts/ci/nuget-vulnerabilities.sh [solution]
set -u
solution=${1:-backend/SoftwareHamgueria.slnx}
out=$(dotnet list "$solution" package --vulnerable --include-transitive 2>&1)
status=$?
echo "$out"
if [ $status -ne 0 ]; then echo "FALHA: dotnet list saiu com $status"; exit 1; fi
if echo "$out" | grep -Eq "has the following vulnerable packages"; then echo "FALHA: pacote vulnerável"; exit 1; fi
if echo "$out" | grep -Eqi "NU1900|NU1905|error"; then echo "FALHA: a checagem não conseguiu consultar as vulnerabilidades"; exit 1; fi
projects=$(echo "$out" | grep -c "The given project")
clean=$(echo "$out" | grep -c "has no vulnerable packages")
if [ "$projects" -eq 0 ] || [ "$projects" -ne "$clean" ]; then echo "FALHA: $clean de $projects projetos confirmados sem vulnerabilidade"; exit 1; fi
echo "OK: $clean projetos sem pacote vulnerável"
