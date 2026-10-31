import axios from 'axios'
import { env } from '@/shared/config/env'
import { getToken } from '@/shared/auth/authToken'
import { auditHeaders } from '@/shared/api/auditHeaders'
import { newTraceparent } from '@/shared/telemetry'

export const httpClient = axios.create({
  baseURL: env.apiBaseUrl,
  headers: { 'Content-Type': 'application/json' },
})

// Attach the kitchen JWT (when present) to every request.
httpClient.interceptors.request.use((config) => {
  const token = getToken()
  if (token) config.headers.Authorization = `Bearer ${token}`
  for (const [name, value] of Object.entries(auditHeaders())) config.headers[name] = value
  // Each call starts a trace the API joins (spec 023), so a slow screen can be followed into the server.
  config.headers.traceparent = newTraceparent()
  return config
})
