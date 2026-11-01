import { CornerDownRight } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

/**
 * An order item's note, under the dish name. The marker is an icon hidden from screen readers, so only the note
 * itself is read (spec 006, RN-05). `inline` keeps it on the dish line (kitchen queue).
 */
export function ItemNote({ notes, inline, className }: { notes: string; inline?: boolean; className?: string }) {
  return (
    <span className={cn(inline ? 'ml-1 inline-flex' : 'mt-0.5 flex', 'items-center gap-1 text-xs', className)}>
      <CornerDownRight aria-hidden="true" className="size-3 shrink-0" />
      {notes}
    </span>
  )
}
