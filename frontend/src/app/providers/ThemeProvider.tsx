import type { ReactNode } from 'react'
import { Toaster } from '@/shared/ui/shadcn'

/**
 * Theming is token-driven (CSS variables in app/styles/index.css, dark by
 * default). This provider only mounts the global toast portal.
 */
export function ThemeProvider({ children }: { children: ReactNode }) {
  return (
    <>
      {children}
      <Toaster position="bottom-right" richColors />
    </>
  )
}
