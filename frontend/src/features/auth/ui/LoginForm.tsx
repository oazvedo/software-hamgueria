import { useState } from 'react'
import { LogIn, Lock } from 'lucide-react'
import { useAuthStore } from '@/features/auth/model/useAuthStore'
import { Button, Input } from '@/shared/ui/shadcn'

export function LoginForm() {
  const authLogin = useAuthStore((s) => s.login)
  const [username, setUsername] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [pending, setPending] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)
    setPending(true)
    try {
      await authLogin(username, password)
    } catch {
      setError('Credenciais inválidas.')
    } finally {
      setPending(false)
    }
  }

  return (
    <div className="mx-auto flex min-h-[60vh] max-w-sm flex-col justify-center px-6">
      <div className="rounded-xl border border-border bg-card p-6">
        <div className="mb-5 flex flex-col items-center gap-2 text-center">
          <span className="flex size-10 items-center justify-center rounded-full border border-border text-brand">
            <Lock className="size-5" />
          </span>
          <h1 className="text-lg font-semibold text-foreground">Acesso à produção</h1>
          {/* EXCEÇÃO ao ruido-scan/code-crumb: rótulo de seção em forma de comentário, identidade dev do produto (spec 006, RN-02) */}
          <p className="font-mono text-xs text-muted-foreground">// área da cozinha</p>
        </div>

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <label className="flex flex-col gap-1">
            <span className="font-mono text-xs text-muted-foreground">usuário</span>
            <Input
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              autoComplete="username"
              autoFocus
            />
          </label>
          <label className="flex flex-col gap-1">
            <span className="font-mono text-xs text-muted-foreground">senha</span>
            <Input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              autoComplete="current-password"
            />
          </label>

          {error && <p className="text-sm text-destructive">{error}</p>}

          <Button type="submit" className="mt-1 w-full" disabled={pending || !username || !password}>
            <LogIn />
            {pending ? 'Entrando…' : 'Entrar'}
          </Button>
        </form>
      </div>
    </div>
  )
}
