import { cn } from '@/shared/lib/cn'

/** The three terminal window dots: decoration, so hidden from screen readers (spec 006, RN-07). */
export function WindowDots({ className }: { className?: string }) {
  return (
    <span aria-hidden="true" data-testid="window-dots" className={cn('flex gap-1.5', className)}>
      <span className="h-3 w-3 rounded-full bg-window-close" />
      <span className="h-3 w-3 rounded-full bg-window-minimize" />
      <span className="h-3 w-3 rounded-full bg-window-zoom" />
    </span>
  )
}
