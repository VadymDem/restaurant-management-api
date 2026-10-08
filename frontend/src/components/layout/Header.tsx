import { Link } from 'react-router-dom'

function Header() {
  return (
    <header className="app-header">
      <div className="container header-content">
        <Link to="/" className="logo">
          Restaurant Reservation
        </Link>

        <nav className="navigation" aria-label="Main navigation">
          <Link to="/">Home</Link>
          <Link to="/login">Login</Link>
          <Link to="/register">Register</Link>
        </nav>
      </div>
    </header>
  )
}

export default Header
