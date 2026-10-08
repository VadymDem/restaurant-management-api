import type {
  LoginRequest,
  LoginResponse,
  RegisterRequest,
} from '../types/auth'

interface ApiProblemDetails {
  title?: string
  detail?: string
  errors?: Record<string, string[]>
}

const API_BASE_URL = (
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5214'
).replace(/\/$/, '')

async function getErrorMessage(response: Response): Promise<string> {
  const fallbackMessage = `Request failed with status ${response.status}.`

  try {
    const problem = (await response.json()) as ApiProblemDetails
    const validationMessage = problem.errors
      ? Object.values(problem.errors).flat()[0]
      : undefined

    return validationMessage ?? problem.detail ?? problem.title ?? fallbackMessage
  } catch {
    return fallbackMessage
  }
}

async function submitAuthRequest(
  endpoint: 'login' | 'register',
  request: LoginRequest | RegisterRequest,
): Promise<LoginResponse> {
  const response = await fetch(`${API_BASE_URL}/api/auth/${endpoint}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(request),
  })

  if (!response.ok) {
    throw new Error(await getErrorMessage(response))
  }

  const result = (await response.json()) as LoginResponse

  if (!result.token) {
    throw new Error('The server response did not include an authentication token.')
  }

  return result
}

export function registerUser(request: RegisterRequest): Promise<LoginResponse> {
  return submitAuthRequest('register', request)
}

export function loginUser(request: LoginRequest): Promise<LoginResponse> {
  return submitAuthRequest('login', request)
}
