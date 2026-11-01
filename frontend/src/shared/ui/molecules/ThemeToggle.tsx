import { Moon, Sun } from 'lucide-react'
import { useThemeStore } from '@/shared/theme'
import { cn } from '@/shared/lib/cn'

/** Switches between the light and dark themes (spec 014). The icon shows the theme you would switch to. */
export function ThemeToggle({ className }: { className?: string }) {
  const theme = useThemeStore((s) => s.theme)
  const toggle = useThemeStore((s) => s.toggle)
  const next = theme === 'dark' ? 'claro' : 'escuro'
  const Icon = theme === 'dark' ? Sun : Moon
  return (
    <button
      type="button"
      onClick={toggle}
      aria-label={`Mudar para o tema ${next}`}
      title={`Tema ${next}`}
      className={cn(
        'inline-flex size-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-secondary hover:text-foreground focus-visible:ring-2 focus-visible:ring-ring focus-visible:outline-none',
        className,
      )}
    >
      <Icon aria-hidden="true" className="size-4" />
    </button>
  )
}
