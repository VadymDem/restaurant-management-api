import { NavLink } from 'react-router-dom'
import './AdminNavigation.css'

const links = [
  { to: '/admin/reservations', label: 'Reservations' },
  { to: '/admin/menu', label: 'Menu' },
]

function AdminNavigation() {
  return (
    <nav className="admin-navigation" aria-label="Administration sections">
      {links.map((link) => (
        <NavLink
          to={link.to}
          className={({ isActive }) =>
            `admin-navigation__link${isActive ? ' admin-navigation__link--active' : ''}`
          }
          key={link.to}
        >
          {link.label}
        </NavLink>
      ))}
    </nav>
  )
}

export default AdminNavigation
