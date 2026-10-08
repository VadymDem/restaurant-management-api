import type { ReactNode } from 'react'
import Header from './Header'
import Footer from './Footer'

interface LayoutProps {
  children: ReactNode
  fullWidth?: boolean
}

function Layout({ children, fullWidth = false }: LayoutProps) {
  return (
    <div className="app">
      <Header />

      <main className={`app-main${fullWidth ? ' app-main--full' : ''}`}>
        {fullWidth ? children : <div className="container">{children}</div>}
      </main>

      <Footer />
    </div>
  )
}

export default Layout
