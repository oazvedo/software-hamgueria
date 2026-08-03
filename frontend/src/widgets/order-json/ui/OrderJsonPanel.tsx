import { useState } from 'react'
import { Highlight, themes } from 'prism-react-renderer'
import { Send, Trash2 } from 'lucide-react'
import { buildOrderJson, useOrderStore } from '@/entities/order'
import { CheckoutDialog } from '@/features/submit-order'
import { TableSelect } from '@/features/select-table'
import { QuantityStepper } from '@/shared/ui/molecules'
import { Button, Input } from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'

export function OrderJsonPanel() {
  const tableId = useOrderStore((s) => s.tableId)
  const customerName = useOrderStore((s) => s.customerName)
  const items = useOrderStore((s) => s.items)
  const setQuantity = useOrderStore((s) => s.setQuantity)
  const setNotes = useOrderStore((s) => s.setNotes)
  const clear = useOrderStore((s) => s.clear)
  const [checkoutOpen, setCheckoutOpen] = useState(false)

  const json = buildOrderJson({ tableId, customerName, items })
  const code = JSON.stringify(json, null, 2)
  const canSubmit = Boolean(tableId) && items.length > 0

  return (
    <div className="flex h-full flex-col rounded-lg border border-border bg-card shadow-sm">
      {/* terminal title bar — macOS traffic lights */}
      <div className="flex items-center gap-2 border-b border-border px-3 py-2">
        <span className="h-3 w-3 rounded-full bg-[#ff5f56]" />
        <span className="h-3 w-3 rounded-full bg-[#ffbd2e]" />
        <span className="h-3 w-3 rounded-full bg-[#27c93f]" />
        <span className="ml-2 font-mono text-xs text-muted-foreground">order.json</span>
      </div>

      <div className="flex items-center justify-between gap-2 border-b border-border px-3 py-2">
        <TableSelect />
        <Button size="sm" variant="ghost" onClick={clear} disabled={items.length === 0}>
          <Trash2 />
          Limpar
        </Button>
      </div>

      {/* live json */}
      <div className="flex-1 overflow-auto p-3 font-mono text-sm">
        <Highlight theme={themes.vsDark} code={code} language="json">
          {({ style, tokens, getLineProps, getTokenProps }) => (
            <pre style={{ ...style, background: 'transparent', margin: 0 }}>
              {tokens.map((line, i) => (
                <div key={i} {...getLineProps({ line })}>
                  <span className="mr-3 select-none text-muted-foreground/40">
                    {String(i + 1).padStart(2, '0')}
                  </span>
                  {line.map((token, key) => (
                    <span key={key} {...getTokenProps({ token })} />
                  ))}
                </div>
              ))}
            </pre>
          )}
        </Highlight>
      </div>

      {/* editable quantities + per-item notes */}
      {items.length > 0 && (
        <div className="max-h-64 overflow-auto border-t border-border p-3">
          {items.map((item) => (
            <div key={item.dishId} className="flex flex-col gap-1.5 border-b border-border/60 py-2 last:border-b-0">
              <div className="flex items-center justify-between gap-2 text-sm">
                <span className="truncate text-foreground">{item.name}</span>
                <div className="flex items-center gap-3">
                  <span className="font-mono text-xs text-muted-foreground">
                    {formatCurrency(item.price * item.quantity)}
                  </span>
                  <QuantityStepper value={item.quantity} onChange={(q) => setQuantity(item.dishId, q)} />
                </div>
              </div>
              <Input
                value={item.notes}
                onChange={(e) => setNotes(item.dishId, e.target.value)}
                placeholder="observações (ex.: sem cebola)"
                maxLength={200}
                className="h-7 text-xs"
              />
            </div>
          ))}
        </div>
      )}

      <div className="border-t border-border p-3">
        <div className="mb-2 flex items-center justify-between">
          <span className="text-xs text-muted-foreground">total</span>
          <span className="font-mono font-semibold text-foreground">{formatCurrency(json.total)}</span>
        </div>
        <Button className="w-full" disabled={!canSubmit} onClick={() => setCheckoutOpen(true)}>
          <Send />
          Enviar pedido
        </Button>
        {items.length > 0 && !tableId && (
          <p className="mt-2 font-mono text-xs text-muted-foreground">
            // selecione uma mesa para enviar o pedido
          </p>
        )}
      </div>

      <CheckoutDialog open={checkoutOpen} onOpenChange={setCheckoutOpen} />
    </div>
  )
}
