import { Link } from 'react-router-dom'
import { ArrowRight } from 'lucide-react'
import { Header } from '@/widgets/header'
import { NeonPanel, TerminalPrompt } from '@/shared/ui/atoms'
import { Button } from '@/shared/ui/shadcn'

export function HomePage() {
  return (
    <div className="min-h-full">
      <Header />
      <main className="mx-auto flex max-w-3xl flex-col items-center gap-8 px-4 py-24 text-center">
        <div className="flex flex-col gap-3">
          <span className="inline-flex items-center gap-2 self-center rounded-full border border-border px-3 py-1 font-mono text-xs text-muted-foreground">
            <span className="text-brand">{'</>'}</span>
            software hamgueria
          </span>
          <h1 className="text-4xl font-semibold tracking-tight text-foreground sm:text-5xl">
            O restaurante onde você monta seu pedido como um JSON.
          </h1>
          <p className="text-lg text-muted-foreground">
            Cardápio à esquerda, seu pedido tomando forma em tempo real à direita.
          </p>
        </div>

        <NeonPanel className="w-full max-w-xl text-left">
          <TerminalPrompt command="curl -X POST softwarehamgueria.com/api/order" />
          <pre className="mt-3 overflow-auto font-mono text-sm text-muted-foreground">{`{
  "tableId": "03",
  "items": [
    { "dish": "Burger Monolito", "quantity": 1 },
    { "dish": "Black Coffee", "quantity": 2 }
  ]
}`}</pre>
        </NeonPanel>

        <Button asChild size="lg">
          <Link to="/menu">
            Abrir cardápio
            <ArrowRight />
          </Link>
        </Button>
      </main>
    </div>
  )
}
