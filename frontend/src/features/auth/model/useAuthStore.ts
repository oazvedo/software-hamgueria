import { create } from 'zustand'
import { clearToken, getToken, setToken } from '@/shared/auth/authToken'
import { login as loginRequest } from '@/features/auth/api/authApi'

interface AuthState {
  token: string | null
  isAuthenticated: boolean
  login: (username: string, password: string) => Promise<void>
  logout: () => void
}

export const useAuthStore = create<AuthState>((set) => ({
  token: getToken(),
  isAuthenticated: Boolean(getToken()),
  login: async (username, password) => {
    const { token } = await loginRequest(username, password)
    setToken(token)
    set({ token, isAuthenticated: true })
  },
  logout: () => {
    clearToken()
    set({ token: null, isAuthenticated: false })
  },
}))
