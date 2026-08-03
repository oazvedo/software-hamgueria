import { httpClient } from '@/shared/api/httpClient'

export interface TokenResponse {
  token: string
  expiresAt: string
}

export async function login(username: string, password: string): Promise<TokenResponse> {
  const { data } = await httpClient.post<TokenResponse>('/api/auth/login', { username, password })
  return data
}
