#!/bin/sh
# Success and failure cases of fsd-lint.mjs (spec 022). Usage: sh scripts/ui/fsd-lint.test.sh
set -u
LINT="$(cd "$(dirname "$0")" && pwd)/fsd-lint.mjs"
TMP=$(mktemp -d)
trap 'rm -rf "$TMP"' EXIT
erros=0

# arquivo <caso> <caminho> <conteúdo>
arquivo() { mkdir -p "$(dirname "$TMP/$1/$2")"; printf '%s\n' "$3" >"$TMP/$1/$2"; }
# confere <ok|falha> <descrição> <caso>
confere() {
  if node "$LINT" "$TMP/$3" >"$TMP/log" 2>&1; then got=ok; else got=falha; fi
  if [ "$got" = "$1" ]; then echo "ok    $2"; else echo "ERRO  $2 (esperado $1, obtido $got)"; sed 's/^/      /' "$TMP/log"; erros=$((erros + 1)); fi
}

arquivo limpo features/f/ui/F.tsx "import { Dish } from '@/entities/dish'
import { cn } from '@/shared/lib/cn'"
arquivo limpo entities/dish/index.ts "export { DishCard } from '@/entities/dish/ui/DishCard'"
arquivo subindo entities/dish/ui/D.tsx "import { AddDish } from '@/features/add-dish'"
arquivo cruzado entities/order/model/o.ts "import type { Dish } from '@/entities/dish'"
arquivo profundo features/f/ui/F.tsx "import { useOrderStore } from '@/entities/order/model/orderStore'"
arquivo shared shared/mocks/m.ts "import type { Dish } from '@/entities/dish'"
arquivo dinamico app/router/r.tsx "const P = lazy(() => import('@/pages/menu'))"

confere ok "imports dentro das regras" limpo
confere falha "camada importando de cima (entities de features)" subindo
confere falha "slices da mesma camada (entities/order de entities/dish)" cruzado
confere falha "caminho profundo em vez da API pública" profundo
confere falha "shared importando de entities" shared
confere ok "import dinâmico do app para pages" dinamico

[ $erros -eq 0 ] && echo "fsd-lint.test: tudo certo" || { echo "fsd-lint.test: $erros erro(s)"; exit 1; }
