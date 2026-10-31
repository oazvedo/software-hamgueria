import { setupServer } from 'msw/node'
import { handlers } from '@/shared/mocks/handlers'

// The app's own MSW handlers, served in Node for tests (ADR 0016).
export const server = setupServer(...handlers)
