
import { Navigate } from 'react-router-dom'
import type { ReactNode } from 'react'
import { isAdministrator } from '../utils/auth'

interface AdminRouteProps {
  children: ReactNode
}

function AdminRoute({ children }: AdminRouteProps) {
  const token = localStorage.getItem('authToken')

  if (!token) {
    return <Navigate to="/login" replace />
  }

  if (!isAdministrator()) {
    return (
      <section className="auth-section">
        <div className="auth-card auth-card--compact">
          <h1>Access denied</h1>
          <p>Administrator access is required to manage the menu.</p>
        </div>
      </section>
    )
  }

  return <>{children}</>
}

export default AdminRoute
