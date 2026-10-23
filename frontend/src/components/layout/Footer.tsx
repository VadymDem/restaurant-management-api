import { Link } from 'react-router-dom'
import './Footer.css'

function Footer() {
  return (
    <footer className="app-footer">
      <div className="container footer-content">
        <div className="footer-brand">
          <strong>Restaurant Reservation</strong>
          <span>Good food. Good company. Your table.</span>
        </div>

        <nav className="footer-navigation" aria-label="Footer navigation">
          <Link to="/">Home</Link>
          <Link to="/menu">Menu</Link>
          <Link to="/login">Log in</Link>
        </nav>

        <p className="footer-copy">© {new Date().getFullYear()} RRMS</p>
      </div>
    </footer>
  )
}

export default Footer
