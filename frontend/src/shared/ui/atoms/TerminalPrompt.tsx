export function TerminalPrompt({ command }: { command: string }) {
  return (
    <p className="font-mono text-sm text-muted-foreground">
      <span className="text-brand">dev@hamgueria</span>
      <span className="text-muted-foreground/60">:</span>
      <span className="text-foreground/70">~</span>
      <span className="text-muted-foreground/60">$ </span>
      <span className="text-foreground">{command}</span>
    </p>
  )
}
