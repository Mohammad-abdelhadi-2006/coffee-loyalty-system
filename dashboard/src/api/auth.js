/*
 * Purpose:
 * Connects the UI layer, backend server, and local storage to manage user authentication states.
 * 
 * Connections & Flow:
 * 1. login(username, password):
 *    - Receives credentials from the UI/Form.
 *    - Sends an async POST request via `api` to the backend endpoint '/auth/login'.
 *    - Merges the returned server session data (e.g., tokens) with the `username`.
 *    - Passes the combined data to `saveSession()` to persist it in browser storage (localStorage/cookies).
 *    - Returns the combined session object back to the UI to trigger UI state updates (e.g., redirecting to dashboard).
 * 
 * 2. logout():
 *    - Triggered by UI user actions (e.g., clicking a Logout button).
 *    - Calls `clearSession()` to purge saved authentication data/tokens from browser storage.
 */
import { api, saveSession, clearSession } from './client.js'

export async function login(username, password) {
  const session = await api.post('/auth/login', { username, password })
  saveSession({ ...session, username })
  return { ...session, username }
}

export function logout() {
  clearSession()
}