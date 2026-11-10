import type {
  Reservation,
  ReservationStatus,
  RestaurantTable,
} from '../types/reservation'

interface ApiProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5214'
).replace(/\/$/, '')

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

async function adminRequest<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const token = localStorage.getItem('authToken')

  if (!token) {
    throw new Error('Please log in as an administrator.')
  }

  const headers = new Headers(options.headers)
  headers.set('Accept', 'application/json')
  headers.set('Authorization', `Bearer ${token}`)

  if (options.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  const response = await fetch(`${API_BASE_URL}${endpoint}`, {
    ...options,
    headers,
  })

  if (!response.ok) {
    if (response.status === 401) {
      throw new Error('Your session has expired. Please log in again.')
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

export function getAdminReservations(): Promise<Reservation[]> {
  return adminRequest<Reservation[]>('/api/admin/reservations')
}

export function updateAdminReservationStatus(
  reservationId: number,
  status: ReservationStatus,
): Promise<Reservation> {
  return adminRequest<Reservation>(
    `/api/admin/reservations/${reservationId}/status`,
    {
      method: 'PUT',
      body: JSON.stringify({ status }),
    },
  )
}

export function getAdminTables(): Promise<RestaurantTable[]> {
  return adminRequest<RestaurantTable[]>('/api/Tables')
}
