#!/usr/bin/env node
// Feature-Sliced Design import rules (ADR 0011, spec 022), checked on every import of frontend/src:
//   1. a layer imports only from layers below it: app > pages > widgets > features > entities > shared;
//   2. two slices of the same layer do not import each other (shared has no slices);
//   3. another slice is imported through its public API (its index), never a deep path.
// Usage: node scripts/ui/fsd-lint.mjs [src dir]   (default frontend/src). Exit 1 with the list when something breaks.
import { readFileSync, readdirSync, statSync } from 'node:fs'
import { join, relative, sep } from 'node:path'

const LAYERS = ['shared', 'entities', 'features', 'widgets', 'pages', 'app']
const root = process.argv[2] ?? 'frontend/src'

function* files(dir) {
  for (const name of readdirSync(dir)) {
    const path = join(dir, name)
    if (statSync(path).isDirectory()) yield* files(path)
    else if (/\.(ts|tsx)$/.test(name) && !/\.test\.tsx?$/.test(name)) yield path
  }
}

const where = (parts) => ({ layer: parts[0], slice: LAYERS.includes(parts[0]) && parts[0] !== 'shared' && parts[0] !== 'app' ? parts[1] : undefined })
const problems = []

for (const file of files(root)) {
  const rel = relative(root, file).split(sep)
  const from = where(rel)
  if (!LAYERS.includes(from.layer)) continue
  const source = readFileSync(file, 'utf8')
  for (const match of source.matchAll(/(?:import|export)[^'"`]*?from\s+['"]@\/([^'"]+)['"]|import\(\s*['"]@\/([^'"]+)['"]\s*\)/g)) {
    const target = (match[1] ?? match[2]).split('/')
    const to = where(target)
    if (!LAYERS.includes(to.layer)) continue
    const line = source.slice(0, match.index).split('\n').length
    const at = `${relative(process.cwd(), file)}:${line}`
    const fromRank = LAYERS.indexOf(from.layer)
    const toRank = LAYERS.indexOf(to.layer)
    if (toRank > fromRank) problems.push(`${at}  camada: ${from.layer} não importa de ${to.layer} (@/${target.join('/')})`)
    else if (toRank === fromRank && to.slice && to.slice !== from.slice)
      problems.push(`${at}  slice: ${from.layer}/${from.slice} não importa de ${to.layer}/${to.slice}`)
    else if (to.slice && to.slice !== from.slice && target.length > 2)
      problems.push(`${at}  API pública: use @/${to.layer}/${to.slice}, não @/${target.join('/')}`)
  }
}

if (problems.length) {
  console.log(`fsd-lint: ${problems.length} import(s) fora das regras do FSD (ADR 0011)`)
  for (const p of problems) console.log('  ' + p)
  process.exit(1)
}
console.log('fsd-lint: clean')
