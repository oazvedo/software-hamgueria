# 014. Tema claro: Plan

- **ADRs aplicados:** 0011, 0012 (estado do cliente em Zustand), 0015, 0016, 0026. Sem ADR novo.

## Abordagem
`index.html` sem `class="dark"` e com um script inline que aplica a escolha guardada (`hamgueria-theme`, texto puro) ou
a do sistema antes da primeira pintura. `shared/theme/themeStore` (Zustand): tema, se houve escolha, `toggle` (aplica
classe e `color-scheme`, guarda), `followSystemTheme` (escuta `prefers-color-scheme` enquanto não há escolha).
`ThemeToggle` (molécula, Sol/Lua do `lucide-react`) no `Header` e na barra da cozinha. O realce do JSON troca entre
`vsDark` e `vsLight`. Contraste: o `--brand` claro foi de L 0.62 (3,4:1 no branco) para 0.52 (5,0:1).

## Verificação
| CA | Como |
|---|---|
| CA-01 a CA-03 | Interface: `themeStore.test.tsx` |
| CA-04 | E2E: `cliente.spec.ts` (escolhe, recarrega, a classe já vem certa) |
| CA-05 | Cálculo de contraste dos tokens (registrado acima) e prints das telas no claro |
