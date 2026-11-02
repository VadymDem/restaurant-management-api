import { useState } from 'react'
import { Link, NavLink, useNavigate } from 'react-router-dom'
import { isAdministrator, isAuthenticated } from '../../utils/auth'
import './Header.css'

function Header() {
  const navigate = useNavigate()
  const [menuOpen, setMenuOpen] = useState(false)
  const authenticated = isAuthenticated()
  const administrator = isAdministrator()

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `navigation__link${isActive ? ' navigation__link--active' : ''}`

  const closeMenu = () => setMenuOpen(false)

  function handleLogout() {
    localStorage.removeItem('authToken')
    closeMenu()
    navigate('/', { replace: true })
  }

  return (
    <header className="app-header">
      <div className="container header-content">
        <Link
          to="/"
          className="brand"
          aria-label="Restaurant Reservation home"
          onClick={closeMenu}
        >
          <span className="brand__mark" aria-hidden="true">R</span>
          <span className="brand__text">
            <strong>Restaurant</strong>
            <small>Reservation</small>
          </span>
        </Link>

        <button
          className={`navigation-toggle${menuOpen ? ' navigation-toggle--open' : ''}`}
          type="button"
          aria-label={menuOpen ? 'Close navigation' : 'Open navigation'}
          aria-controls="main-navigation"
          aria-expanded={menuOpen}
          onClick={() => setMenuOpen((open) => !open)}
        >
          <span />
        </button>

        <nav
          id="main-navigation"
          className={`navigation${menuOpen ? ' navigation--open' : ''}`}
          aria-label="Main navigation"
        >
          <NavLink to="/" end className={navLinkClass} onClick={closeMenu}>
            Home
          </NavLink>
          <NavLink to="/menu" className={navLinkClass} onClick={closeMenu}>
            Our menu
          </NavLink>
          {authenticated ? (
            <>
              <NavLink to="/reservations" className={navLinkClass} onClick={closeMenu}>
                Reserve
              </NavLink>
              {administrator && (
                <NavLink to="/admin/menu" className={navLinkClass} onClick={closeMenu}>
                  Manage menu
                </NavLink>
              )}
              <button className="navigation__logout" type="button" onClick={handleLogout}>
                Sign out
              </button>
            </>
          ) : (
            <>
              <NavLink to="/login" className={navLinkClass} onClick={closeMenu}>
                Log in
              </NavLink>
              <Link to="/register" className="navigation__cta" onClick={closeMenu}>
                Create account
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  )
}

export default Header
