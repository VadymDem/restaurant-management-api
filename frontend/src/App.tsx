
import { Navigate, Route, Routes } from 'react-router-dom'
import Layout from './components/layout/Layout'
import AdminRoute from './components/AdminRoute'
import AuthenticatedRoute from './components/AuthenticatedRoute'
import AdminMenuPage from './pages/AdminMenuPage'
import AdminReservationsPage from './pages/AdminReservationsPage'
import HomePage from './pages/HomePage'
import LoginPage from './pages/LoginPage'
import MenuPage from './pages/MenuPage'
import MyReservationsPage from './pages/MyReservationsPage'
import RegisterPage from './pages/RegisterPage'
import ReservationPage from './pages/ReservationPage'

function App() {
  return (
    <Routes>
      <Route
        path="/"
        element={
          <Layout fullWidth>
            <HomePage />
          </Layout>
        }
      />

      <Route
        path="/register"
        element={
          <Layout fullWidth>
            <RegisterPage />
          </Layout>
        }
      />

      <Route
        path="/login"
        element={
          <Layout fullWidth>
            <LoginPage />
          </Layout>
        }
      />

      <Route
        path="/menu"
        element={
          <Layout fullWidth>
            <MenuPage />
          </Layout>
        }
      />

      <Route
        path="/reservations"
        element={
          <Layout fullWidth>
            <AuthenticatedRoute>
              <ReservationPage />
            </AuthenticatedRoute>
          </Layout>
        }
      />

      <Route
        path="/my-reservations"
        element={
          <Layout fullWidth>
            <AuthenticatedRoute>
              <MyReservationsPage />
            </AuthenticatedRoute>
          </Layout>
        }
      />

      <Route
        path="/admin/menu"
        element={
          <Layout>
            <AdminRoute>
              <AdminMenuPage />
            </AdminRoute>
          </Layout>
        }
      />

      <Route
        path="/admin/reservations"
        element={
          <Layout fullWidth>
            <AdminRoute>
              <AdminReservationsPage />
            </AdminRoute>
          </Layout>
        }
      />

      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}

export default App
