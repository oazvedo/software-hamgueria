// Author and screen sent with every API call, so the database audit trail knows who changed what and from where
// (ADR 0027). The name is the comanda name the app layer keeps in sync; the kitchen is identified by its token instead.
// Header values must be ASCII, so the name travels URI-encoded and the API decodes it.

let author = ''

export function setAuditAuthor(name: string): void {
  author = name.trim()
}

export function auditHeaders(): Record<string, string> {
  const headers: Record<string, string> = {}
  if (author) headers['X-Author-Name'] = encodeURIComponent(author)
  if (typeof window !== 'undefined') headers['X-Frontend-Url'] = window.location.href
  return headers
}
