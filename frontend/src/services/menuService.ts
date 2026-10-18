
import type { MenuItem, MenuItemRequest } from '../types/menu'

const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5214'
).replace(/\/$/, '')

interface ApiProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

async function getErrorMessage(response: Response): Promise<string> {
  const fallback = `Request failed with status ${response.status}.`

  try {
    const problem = (await response.json()) as ApiProblemDetails
    const validationMessage = problem.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined

    return validationMessage ?? problem.detail ?? problem.title ?? fallback
  } catch {
    return fallback
  }
}

async function menuRequest<T>(
  endpoint: string,
  options: RequestInit = {},
  requiresAuth = false,
): Promise<T> {
  const headers = new Headers(options.headers)
  headers.set('Accept', 'application/json')

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  if (requiresAuth) {
    const token = localStorage.getItem('authToken')

    if (!token) {
      throw new Error('You must log in as an administrator.')
    }

    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Your session is missing, invalid, or expired.')
    }

    if (response.status === 403) {
      throw new Error('Administrator access is required.')
    }

    throw new Error(await getErrorMessage(response))
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

export function getMenuItems(): Promise<MenuItem[]> {
  return menuRequest<MenuItem[]>('/api/Menu')
}

export function createMenuItem(
  item: MenuItemRequest,
): Promise<MenuItem> {
  return menuRequest<MenuItem>(
    '/api/Menu',
    {
      method: 'POST',
      body: JSON.stringify(item),
    },
    true,
  )
}

export function updateMenuItem(
  id: number,
  item: MenuItemRequest,
): Promise<MenuItem> {
  return menuRequest<MenuItem>(
    `/api/Menu/${id}`,
    {
      method: 'PUT',
      body: JSON.stringify(item),
    },
    true,
  )
}

export function deleteMenuItem(id: number): Promise<void> {
  return menuRequest<void>(
    `/api/Menu/${id}`,
    { method: 'DELETE' },
    true,
  )
}
