import { api, saveSession, clearSession } from './client.js'

export async function login(username, password) {
  const session = await api.post('/auth/login', { username, password })
  saveSession({ ...session, username })
  return { ...session, username }
}

export function logout() {
  clearSession()
}