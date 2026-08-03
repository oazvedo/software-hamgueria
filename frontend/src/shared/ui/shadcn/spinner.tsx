import { Loader2Icon } from 'lucide-react'
import { cn } from '@/shared/lib/cn'

/** Neutral loading spinner (replaces MUI CircularProgress). */
export function Spinner({ className }: { className?: string }) {
  return (
    <Loader2Icon
      role="status"
      aria-label="Carregando"
      className={cn('size-5 animate-spin text-muted-foreground', className)}
    />
  )
}
