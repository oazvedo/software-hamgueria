import { Plus } from 'lucide-react'
import { MAX_ITEM_QUANTITY, useOrderStore } from '@/entities/order'
import type { Dish } from '@/entities/dish'
import { QuantityStepper } from '@/shared/ui/molecules'
import { Button } from '@/shared/ui/shadcn'

/** "Adicionar" while the dish is not in the cart; then a stepper right on the card (spec 008, RN-04). */
export function AddDishButton({ dish }: { dish: Dish }) {
  const addDish = useOrderStore((s) => s.addDish)
  const setQuantity = useOrderStore((s) => s.setQuantity)
  const quantity = useOrderStore((s) => s.items.find((i) => i.dishId === dish.id)?.quantity ?? 0)

  if (quantity === 0) {
    return (
      <Button size="sm" variant="outline" onClick={() => addDish(dish)}>
        <Plus />
        Adicionar
      </Button>
    )
  }
  return (
    <QuantityStepper
      size="md"
      value={quantity}
      max={MAX_ITEM_QUANTITY}
      onChange={(n) => setQuantity(dish.id, n)}
      decreaseLabel={`Tirar um ${dish.name}`}
      increaseLabel={`Mais um ${dish.name}`}
    />
  )
}
