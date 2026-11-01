import { ChefHat, LogOut } from 'lucide-react'
import { ThemeToggle } from '@/shared/ui/molecules'
import { Button } from '@/shared/ui/shadcn'

export function KitchenTopBar({ onLogout }: { onLogout?: () => void }) {
  return (
    <header className="sticky top-0 z-10 border-b border-border bg-background/80 backdrop-blur">
      <div className="mx-auto flex w-full max-w-[1400px] items-center justify-between px-6 py-3">
        <div className="flex items-center gap-2 font-semibold text-foreground">
          <ChefHat className="size-5 text-brand" />
          Produção
          {/* EXCEÇÃO ao ruido-scan/code-crumb: rótulo de seção em forma de comentário, identidade dev do produto (spec 006, RN-02) */}
          <span className="font-mono text-xs font-normal text-muted-foreground">// cozinha</span>
        </div>
        <div className="flex items-center gap-1">
          <ThemeToggle />
          {onLogout && (
            <Button size="sm" variant="ghost" onClick={onLogout}>
              <LogOut />
              Sair
            </Button>
          )}
        </div>
      </div>
    </header>
  )
}
