// Owner tokens of the orders made on this device (ADR 0030): the API returns one when an order is created, and only
// its holder (or the kitchen) may change or cancel that order later. Kept per device, in localStorage.

const KEY = 'hamgueria-owner-tokens'

function load(): Record<string, string> {
  try {
    return JSON.parse(localStorage.getItem(KEY) ?? '{}') as Record<string, string>
  } catch {
    return {}
  }
}

export function saveOwnerToken(orderId: string, token: string): void {
  try {
    localStorage.setItem(KEY, JSON.stringify({ ...load(), [orderId]: token }))
  } catch {
    // Private mode or full storage: the order still exists; only changing it later needs the kitchen.
  }
}

export function ownerTokenOf(orderId: string): string | undefined {
  return load()[orderId]
}
