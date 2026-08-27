const BASE = '/api'
const KEY = 'fanjan-session'

export function saveSession(s) {
  localStorage.setItem(KEY, JSON.stringify(s))
}

export function loadSession() {
  const raw = localStorage.getItem(KEY)
  return raw ? JSON.parse(raw) : null
}

export function clearSession() {
  localStorage.removeItem(KEY)
}

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

export const api = {
  get:   (path)       => request(path),
  post:  (path, body) => request(path, 'POST',  body),
  put:   (path, body) => request(path, 'PUT',   body),
  patch: (path, body) => request(path, 'PATCH', body),
  del:   (path)       => request(path, 'DELETE'),
}