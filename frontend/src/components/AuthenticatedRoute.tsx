import type { ReactNode } from 'react'
import { Navigate, useLocation } from 'react-router-dom'
import { isAuthenticated } from '../utils/auth'

interface AuthenticatedRouteProps {
  children: ReactNode
}

function AuthenticatedRoute({ children }: AuthenticatedRouteProps) {
  const location = useLocation()

  if (!isAuthenticated()) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  return children
}

export default AuthenticatedRoute
