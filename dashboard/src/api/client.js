// Configuration constants defining the base API endpoint path and the key used for session persistence
const BASE = '/api'
const KEY = 'fanjan-session'

// Serializes the user session object into JSON format and saves it to localStorage
export function saveSession(s) {
  localStorage.setItem(KEY, JSON.stringify(s))
}

// Retrieves the stored session string from localStorage and parses it back into a JavaScript object (returns null if missing)
export function loadSession() {
  const raw = localStorage.getItem(KEY)
  return raw ? JSON.parse(raw) : null
}

// Removes the session token and user state data completely from local browser storage
export function clearSession() {
  localStorage.removeItem(KEY)
}

// Core helper function that executes HTTP fetch requests, handles authorization headers, catches network failures, and parses JSON responses/errors
async function request(path, method = 'GET', body = null) {
  const session = loadSession()

  const headers = {}
  if (body) headers['Content-Type'] = 'application/json'
  if (session) headers['Authorization'] = 'Bearer ' + session.token

  let res
  try {
    res = await fetch(BASE + path, {
      method,
      headers,
      body: body ? JSON.stringify(body) : undefined,
    })
  } catch (e) {
    console.error('فشل الاتصال:', e)
    const err = new Error('تعذر الوصول الى الخادم')
    err.code = 'NETWORK_ERROR'
    throw err
  }

  if (res.status === 204) return null

  let data = null
  try { data = await res.json() } catch { data = null }

  if (!res.ok) {
    const err = new Error(data?.message || 'صار خطأ غير متوقع')
    err.code = data?.code || 'UNKNOWN'
    throw err
  }

  return data
}

// Exported wrapper object exposing concise utility methods for standard HTTP requests (GET, POST, PUT, PATCH, DELETE)
export const api = {
  get:   (path)       => request(path),
  post:  (path, body) => request(path, 'POST',  body),
  put:   (path, body) => request(path, 'PUT',   body),
  patch: (path, body) => request(path, 'PATCH', body),
  del:   (path)       => request(path, 'DELETE'),
}