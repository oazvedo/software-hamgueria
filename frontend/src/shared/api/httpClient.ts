import axios from 'axios'
import { env } from '@/shared/config/env'
import { getToken } from '@/shared/auth/authToken'

export const httpClient = axios.create({
  baseURL: env.apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
})

// Attach the kitchen JWT (when present) to every request.
httpClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  return config
})
