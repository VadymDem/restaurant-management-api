import { useState } from 'react'
import { Link, NavLink } from 'react-router-dom'
import './Header.css'

function Header() {
  const [menuOpen, setMenuOpen] = useState(false)

  const navLinkClass = ({ isActive }: { isActive: boolean }) =>
    `navigation__link${isActive ? ' navigation__link--active' : ''}`

  const closeMenu = () => setMenuOpen(false)

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
          <NavLink to="/login" className={navLinkClass} onClick={closeMenu}>
            Log in
          </NavLink>
          <Link to="/register" className="navigation__cta" onClick={closeMenu}>
            Create account
          </Link>
        </nav>
      </div>
    </header>
  )
}

export default Header
