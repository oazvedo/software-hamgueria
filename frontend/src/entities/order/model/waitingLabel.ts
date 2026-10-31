/** How long an order has been waiting, for the kitchen (spec 021 moved it out of the page). */
export function waitingLabel(iso: string): string {
  const mins = Math.floor((Date.now() - new Date(iso).getTime()) / 60000)
  if (mins < 1) return 'agora há pouco'
  if (mins === 1) return 'há 1 min'
  return `há ${mins} min`
}

/** Finalizar / Cancelar buttons, reused across queue + table views. */
