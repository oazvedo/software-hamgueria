export const env = {
  apiBaseUrl: import.meta.env.VITE_API_BASE_URL ?? '',
  enableMocks: import.meta.env.VITE_ENABLE_MOCKS !== 'false',
  isDev: import.meta.env.DEV,
}
