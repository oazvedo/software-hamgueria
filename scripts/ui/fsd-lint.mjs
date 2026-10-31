#!/usr/bin/env node
// Feature-Sliced Design import rules (ADR 0011, spec 022), checked on every import of frontend/src:
//   1. a layer imports only from layers below it: app > pages > widgets > features > entities > shared;
//   2. two slices of the same layer do not import each other (shared has no slices);
//   3. another slice is imported through its public API (its index), never a deep path.
// Usage: node scripts/ui/fsd-lint.mjs [dir]   (default frontend/src; any folder inside src works too).
// Sees `@/` aliases, relative paths and side-effect imports. Exit 1 with the list when something breaks.
import { readFileSync, readdirSync, statSync } from 'node:fs'
import { dirname, join, relative, resolve, sep } from 'node:path'

const LAYERS = ['shared', 'entities', 'features', 'widgets', 'pages', 'app']
const target = resolve(process.argv[2] ?? 'frontend/src')

function* files(dir) {
  if (statSync(dir).isFile()) return yield dir
  for (const name of readdirSync(dir)) {
    const path = join(dir, name)
    if (statSync(path).isDirectory()) yield* files(path)
    else if (/\.(ts|tsx)$/.test(name) && !/\.test\.tsx?$/.test(name)) yield path
  }
}

/** The FSD position of an absolute path: the segment after `src`, else the first layer-named segment. */
function locate(absolute) {
  const parts = absolute.split(sep)
  const src = parts.lastIndexOf('src')
  let i = src >= 0 && LAYERS.includes(parts[src + 1]) ? src + 1 : parts.findIndex((p) => LAYERS.includes(p))
  if (i < 0) return undefined
  const layer = parts[i]
  return { root: parts.slice(0, i).join(sep), layer, slice: layer === 'shared' || layer === 'app' ? undefined : parts[i + 1], rest: parts.slice(i) }
}

const IMPORT = /(?:import|export)\s+(?:type\s+)?(?:[^'"`;]*?\s+from\s+)?['"]([^'"]+)['"]|import\(\s*['"]([^'"]+)['"]\s*\)/g
const problems = []

for (const file of files(target)) {
  const from = locate(file)
  if (!from) continue
  const source = readFileSync(file, 'utf8')
  for (const match of source.matchAll(IMPORT)) {
    const spec = match[1] ?? match[2]
    let to
    if (spec.startsWith('@/')) to = locate(join(from.root, ...spec.slice(2).split('/')))
    else if (spec.startsWith('.')) to = locate(resolve(dirname(file), spec))
    if (!to || !LAYERS.includes(to.layer)) continue
    const line = source.slice(0, match.index).split('\n').length
    const at = `${relative(process.cwd(), file)}:${line}`
    const fromRank = LAYERS.indexOf(from.layer)
    const toRank = LAYERS.indexOf(to.layer)
    const shown = `${to.rest.join('/')}`
    if (toRank > fromRank) problems.push(`${at}  camada: ${from.layer} não importa de ${to.layer} (${shown})`)
    else if (toRank === fromRank && to.slice && to.slice !== from.slice)
      problems.push(`${at}  slice: ${from.layer}/${from.slice} não importa de ${to.layer}/${to.slice}`)
    else if (to.slice && to.slice !== from.slice && to.rest.length > 2)
      problems.push(`${at}  API pública: use @/${to.layer}/${to.slice}, não ${shown}`)
  }
}

if (problems.length) {
  console.log(`fsd-lint: ${problems.length} import(s) fora das regras do FSD (ADR 0011)`)
  for (const p of problems) console.log('  ' + p)
  process.exit(1)
}
console.log('fsd-lint: clean')
