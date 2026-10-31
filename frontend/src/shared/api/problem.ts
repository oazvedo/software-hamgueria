import { isAxiosError } from 'axios'

/** Error body every API route returns (RFC 9457, ADR 0022). */
export interface ApiProblem {
  type: string
  title: string
  status: number
  detail?: string
  traceId?: string
  /** Invalid input only: JSON path of the field ("customerName", "items[1].quantity") to its messages. */
  errors?: Record<string, string[]>
}

const PROBLEM_TYPE_PREFIX = 'https://hamgueria.dev/erros/'

export const problemTypes = {
  validation: `${PROBLEM_TYPE_PREFIX}validacao`,
  businessRule: `${PROBLEM_TYPE_PREFIX}regra`,
  notFound: `${PROBLEM_TYPE_PREFIX}nao-encontrado`,
  unauthorized: `${PROBLEM_TYPE_PREFIX}sem-login`,
  forbidden: `${PROBLEM_TYPE_PREFIX}sem-permissao`,
  failure: `${PROBLEM_TYPE_PREFIX}falha`,
} as const

function isProblem(body: unknown): body is ApiProblem {
  if (typeof body !== 'object' || body === null) return false
  const { type, title } = body as Record<string, unknown>
  return typeof type === 'string' && typeof title === 'string'
}

/** The ProblemDetails of a failed request, or null (network error, unexpected body). */
export function readProblem(error: unknown): ApiProblem | null {
  if (!isAxiosError(error)) return null
  const body: unknown = error.response?.data
  return isProblem(body) ? body : null
}

/** One line for a toast: the server's own reason when there is one, otherwise the fallback. */
export function problemMessage(error: unknown, fallback: string): string {
  const problem = readProblem(error)
  if (!problem) return fallback
  if (problem.detail) return problem.detail
  const first = problem.errors && Object.values(problem.errors)[0]?.[0]
  return first ?? problem.title
}

/** Field messages of an invalid-input problem, one message per field. */
export function fieldErrors(error: unknown): Record<string, string> {
  const errors = readProblem(error)?.errors ?? {}
  return Object.fromEntries(
    Object.entries(errors)
      .filter(([, messages]) => messages.length > 0)
      .map(([field, messages]) => [field, messages[0]]),
  )
}
