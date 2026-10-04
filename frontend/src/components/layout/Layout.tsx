import type { ReactNode } from 'react'
import Header from './Header'
import Footer from './Footer'

interface LayoutProps {
  children: ReactNode
}

function Layout({ children }: LayoutProps) {
  return (
    <div className="app">
      <Header />

      <main className="app-main">
        <div className="container">{children}</div>
      </main>

      <Footer />
    </div>
  )
}

export default Layout