import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { loginUser } from '../services/authService'

interface LoginFormValues {
  email: string
  password: string
}

type LoginFormErrors = Partial<Record<keyof LoginFormValues, string>>

const initialFormValues: LoginFormValues = {
  email: '',
  password: '',
}

function validateForm(values: LoginFormValues): LoginFormErrors {
  const errors: LoginFormErrors = {}
  const email = values.email.trim()

  if (!email) {
    errors.email = 'Email is required.'
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
    errors.email = 'Enter a valid email address.'
  }

  if (!values.password) {
    errors.password = 'Password is required.'
  }

  return errors
}

function LoginPage() {
  const navigate = useNavigate()
  const [values, setValues] = useState(initialFormValues)
  const [errors, setErrors] = useState<LoginFormErrors>({})
  const [serverError, setServerError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function handleChange(event: ChangeEvent<HTMLInputElement>) {
    const field = event.target.name as keyof LoginFormValues
    const value = event.target.value

    setValues((currentValues) => ({
      ...currentValues,
      [field]: value,
    }))

    setErrors((currentErrors) => ({
      ...currentErrors,
      [field]: undefined,
    }))
    setServerError('')
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()

    const validationErrors = validateForm(values)
    setErrors(validationErrors)
    setServerError('')

    if (Object.keys(validationErrors).length > 0) {
      return
    }

    setIsSubmitting(true)

    try {
      const response = await loginUser({
        email: values.email.trim(),
        password: values.password,
      })

      localStorage.setItem('authToken', response.token)
      navigate('/')
    } catch (error) {
      setServerError(
        error instanceof Error
          ? error.message
          : 'Login failed. Please try again.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <section className="auth-section">
      <div className="auth-card auth-card--compact">
        <div className="auth-heading">
          <p className="auth-eyebrow">Welcome back</p>
          <h1>Log in to your account</h1>
          <p>Access your reservations and manage your account.</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit} noValidate>
          <div className="form-field">
            <label htmlFor="email">Email</label>
            <input
              id="email"
              name="email"
              type="email"
              value={values.email}
              onChange={handleChange}
              autoComplete="email"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'email-error' : undefined}
              disabled={isSubmitting}
            />
            {errors.email && (
              <p className="field-error" id="email-error">
                {errors.email}
              </p>
            )}
          </div>

          <div className="form-field">
            <label htmlFor="password">Password</label>
            <input
              id="password"
              name="password"
              type="password"
              value={values.password}
              onChange={handleChange}
              autoComplete="current-password"
              aria-invalid={Boolean(errors.password)}
              aria-describedby={errors.password ? 'password-error' : undefined}
              disabled={isSubmitting}
            />
            {errors.password && (
              <p className="field-error" id="password-error">
                {errors.password}
              </p>
            )}
          </div>

          {serverError && (
            <p className="form-error" role="alert">
              {serverError}
            </p>
          )}

          <button className="primary-button" type="submit" disabled={isSubmitting}>
            {isSubmitting ? 'Logging in...' : 'Log in'}
          </button>
        </form>

        <p className="auth-switch">
          Do not have an account? <Link to="/register">Create one</Link>
        </p>
      </div>
    </section>
  )
}

export default LoginPage
