import { useEffect, useRef, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Plus, Send } from 'lucide-react'
import { useOrderStore, useTableComandas } from '@/entities/order'
import { useTables } from '@/entities/table'
import { useSubmitOrder } from '@/features/submit-order/model/useSubmitOrder'
import {
  Button,
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  Input,
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
  Spinner,
} from '@/shared/ui/shadcn'
import { formatCurrency } from '@/shared/lib/formatCurrency'

const NEW = '__new__'

export function CheckoutDialog({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const tableId = useOrderStore((s) => s.tableId)
  const items = useOrderStore((s) => s.items)
  const persistedName = useOrderStore((s) => s.customerName)
  const setCustomerName = useOrderStore((s) => s.setCustomerName)

  const { data: tables } = useTables()
  const { data: comandas, isLoading } = useTableComandas(tableId ?? undefined, open)
  const submit = useSubmitOrder()
  const navigate = useNavigate()

  const [choice, setChoice] = useState<string>(NEW)
  const [newName, setNewName] = useState('')
  const inited = useRef(false)

  // Initialize once per open, after the existing comandas have loaded.
  useEffect(() => {
    if (!open) {
      inited.current = false
      return
    }
    if (inited.current || isLoading) return
    inited.current = true
    const existing = comandas ?? []
    if (persistedName && existing.includes(persistedName)) {
      setChoice(persistedName)
      setNewName('')
    } else {
      setChoice(NEW)
      setNewName(persistedName)
    }
  }, [open, isLoading, comandas, persistedName])

  const existing = comandas ?? []
  const creatingNew = choice === NEW || existing.length === 0
  const finalName = creatingNew ? newName.trim() : choice
  const tableName = tables?.find((t) => t.id === tableId)?.tableName ?? tableId

  const total = items.reduce((sum, i) => sum + i.price * i.quantity, 0)
  const preview = {
    mesa: tableName,
    comanda: finalName || '???',
    itens: items.length,
    total: formatCurrency(total),
  }

  const handleConfirm = () => {
    if (!finalName || !tableId) return
    setCustomerName(finalName)
    submit.mutate(undefined, {
      onSuccess: (order) => {
        onOpenChange(false)
        navigate(`/comanda/${order.tableId}/${encodeURIComponent(order.customerName)}`)
      },
    })
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="gap-0 overflow-hidden p-0" showCloseButton={false}>
        {/* terminal title bar */}
        <div className="flex items-center gap-2 border-b border-border px-3 py-2">
          <span className="h-3 w-3 rounded-full bg-[#ff5f56]" />
          <span className="h-3 w-3 rounded-full bg-[#ffbd2e]" />
          <span className="h-3 w-3 rounded-full bg-[#27c93f]" />
          <span className="ml-2 font-mono text-xs text-muted-foreground">checkout.json</span>
        </div>

        <div className="flex flex-col gap-4 p-5">
          <DialogHeader>
            <DialogTitle>Identifique sua comanda</DialogTitle>
          </DialogHeader>

          {/* json preview */}
          <pre className="overflow-auto rounded-lg border border-border bg-background/60 p-3 font-mono text-xs text-muted-foreground">
            {JSON.stringify(preview, null, 2)}
          </pre>

          {/* comanda picker */}
          {isLoading ? (
            <div className="flex justify-center py-4">
              <Spinner className="size-5" />
            </div>
          ) : (
            <div className="flex flex-col gap-2">
              {existing.length > 0 && (
                <label className="flex flex-col gap-1">
                  <span className="font-mono text-xs text-muted-foreground">comanda</span>
                  <Select value={choice} onValueChange={setChoice}>
                    <SelectTrigger className="w-full">
                      <SelectValue placeholder="selecione a comanda…" />
                    </SelectTrigger>
                    <SelectContent>
                      {existing.map((name) => (
                        <SelectItem key={name} value={name}>
                          {name}
                        </SelectItem>
                      ))}
                      <SelectItem value={NEW}>➕ nova comanda</SelectItem>
                    </SelectContent>
                  </Select>
                </label>
              )}

              {creatingNew && (
                <label className="flex flex-col gap-1">
                  <span className="font-mono text-xs text-muted-foreground">
                    {existing.length > 0 ? 'nome da nova comanda' : 'seu nome (comanda)'}
                  </span>
                  <Input
                    value={newName}
                    onChange={(e) => setNewName(e.target.value)}
                    placeholder="ex.: joão"
                    maxLength={60}
                    autoFocus
                    onKeyDown={(e) => {
                      if (e.key === 'Enter' && finalName) handleConfirm()
                    }}
                  />
                </label>
              )}
            </div>
          )}

          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => onOpenChange(false)}>
              Cancelar
            </Button>
            <Button onClick={handleConfirm} disabled={!finalName || submit.isPending}>
              {creatingNew && existing.length > 0 && <Plus />}
              <Send />
              {submit.isPending ? 'Enviando…' : 'Enviar pedido'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  )
}
