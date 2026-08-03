import { Check, Plus } from 'lucide-react'
import { useOrderStore } from '@/entities/order'
import type { Dish } from '@/entities/dish'
import { Button } from '@/shared/ui/shadcn'

export function AddDishButton({ dish, className }: { dish: Dish; className?: string }) {
  const addDish = useOrderStore((s) => s.addDish)
  const quantity = useOrderStore((s) => s.items.find((i) => i.dishId === dish.id)?.quantity ?? 0)
  const inOrder = quantity > 0

  return (
    <Button
      size="sm"
      variant={inOrder ? 'secondary' : 'outline'}
      onClick={() => addDish(dish)}
      className={className}
    >
      {inOrder ? <Check /> : <Plus />}
      {inOrder ? `No pedido (${quantity})` : 'Adicionar'}
    </Button>
  )
}
