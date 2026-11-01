import { useState } from 'react'
import { Pencil, X } from 'lucide-react'
import { MAX_ITEM_QUANTITY, isTerminal, ownerTokenOf } from '@/entities/order'
import type { OrderResponse } from '@/entities/order'
import { useChangeOwnOrder } from '@/features/change-own-order/model/useChangeOwnOrder'
import { QuantityStepper } from '@/shared/ui/molecules'
import { Button, Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from '@/shared/ui/shadcn'

/**
 * Change or cancel an order (spec 015). Only on the device that created it (it holds the owner token, ADR 0030) and
 * while it is not served or cancelled; the API checks both again.
 */
export function OwnOrderActions({ order }: { order: OrderResponse }) {
  const { cancel, update } = useChangeOwnOrder(order.id)
  const [confirming, setConfirming] = useState(false)
  const [editing, setEditing] = useState(false)
  const [quantities, setQuantities] = useState<Record<string, number>>({})

  if (!ownerTokenOf(order.id) || isTerminal(order.status)) return null

  const openEditor = () => {
    setQuantities(Object.fromEntries(order.items.map((i) => [i.dishId, i.quantity])))
    setEditing(true)
  }
  const kept = order.items.filter((i) => (quantities[i.dishId] ?? 0) > 0)

  return (
    <>
      <div className="flex flex-wrap justify-center gap-3">
        <Button variant="outline" onClick={openEditor}>
          <Pencil />
          Alterar itens
        </Button>
        <Button variant="ghost" className="text-destructive hover:text-destructive" onClick={() => setConfirming(true)}>
          <X />
          Cancelar pedido
        </Button>
      </div>

      <Dialog open={confirming} onOpenChange={setConfirming}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Cancelar este pedido?</DialogTitle>
            <DialogDescription>A cozinha deixa de prepará-lo. Não dá para desfazer.</DialogDescription>
          </DialogHeader>
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setConfirming(false)}>
              Manter pedido
            </Button>
            <Button
              variant="destructive"
              disabled={cancel.isPending}
              onClick={() => cancel.mutate(undefined, { onSettled: () => setConfirming(false) })}
            >
              Cancelar pedido
            </Button>
          </div>
        </DialogContent>
      </Dialog>

      <Dialog open={editing} onOpenChange={setEditing}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Alterar itens</DialogTitle>
            <DialogDescription>Ajuste as quantidades. Zerar um item tira ele do pedido.</DialogDescription>
          </DialogHeader>
          <ul className="flex flex-col gap-3" aria-label="Itens do pedido">
            {order.items.map((item) => (
              <li key={item.dishId} className="flex items-center justify-between gap-3 text-sm">
                <span className="text-foreground">{item.dishName}</span>
                <QuantityStepper
                  size="md"
                  value={quantities[item.dishId] ?? 0}
                  max={MAX_ITEM_QUANTITY}
                  onChange={(n) => setQuantities((q) => ({ ...q, [item.dishId]: Math.max(0, n) }))}
                  decreaseLabel={`Tirar um ${item.dishName}`}
                  increaseLabel={`Mais um ${item.dishName}`}
                />
              </li>
            ))}
          </ul>
          {kept.length === 0 && (
            <p role="alert" className="text-sm text-destructive">
              O pedido precisa de pelo menos um item. Para desistir de tudo, cancele o pedido.
            </p>
          )}
          <div className="flex justify-end gap-2">
            <Button variant="outline" onClick={() => setEditing(false)}>
              Voltar
            </Button>
            <Button
              disabled={kept.length === 0 || update.isPending}
              onClick={() =>
                update.mutate(
                  kept.map((i) => ({ dishId: i.dishId, quantity: quantities[i.dishId], notes: i.notes || undefined })),
                  { onSuccess: () => setEditing(false) },
                )
              }
            >
              Salvar alteração
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </>
  )
}
