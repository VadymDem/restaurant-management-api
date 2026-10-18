
interface JwtPayload {
  role?: string | string[]
  roles?: string | string[]
  exp?: number
  [key: string]: unknown
}

const ROLE_CLAIM =
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'

function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const payload = token.split('.')[1]

    if (!payload) {
      return null
    }

    const base64 = payload
      .replace(/-/g, '+')
      .replace(/_/g, '/')

    const decoded = atob(base64)

    return JSON.parse(decoded) as JwtPayload
  } catch {
    return null
  }
}

export function isAdministrator(): boolean {
  const token = localStorage.getItem('authToken')

  if (!token) {
    return false
  }

  const payload = decodeJwtPayload(token)

  if (!payload) {
    return false
  }

  if (typeof payload.exp !== 'number') {
    return false
  }

  if (payload.exp * 1000 <= Date.now()) {
    return false
  }

  const role =
    payload.role ??
    payload.roles ??
    payload[ROLE_CLAIM]

  return Array.isArray(role)
    ? role.includes('Administrator')
    : role === 'Administrator'
}
