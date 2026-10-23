import { useEffect, useState } from 'react'
import MenuItemCard from '../components/menu/MenuItemCard'
import { getMenuItems } from '../services/menuService'
import type { MenuItem } from '../types/menu'
import './MenuPage.css'

function MenuPage() {
  const [items, setItems] = useState<MenuItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let active = true

    async function loadMenu() {
      setLoading(true)
      setError('')

      try {
        const data = await getMenuItems()

        if (active) {
          setItems(data)
        }
      } catch (err) {
        if (active) {
          setError(
            err instanceof Error ? err.message : 'Failed to load the menu.',
          )
        }
      } finally {
        if (active) {
          setLoading(false)
        }
      }
    }

    void loadMenu()

    return () => {
      active = false
    }
  }, [reloadKey])

  return (
    <div className="menu-page">
      <section className="menu-page__hero" aria-labelledby="menu-heading">
        <div className="menu-page__hero-content">
          <p>Made with care</p>
          <h1 id="menu-heading">Our Restaurant Menu</h1>
          <span>
            Discover dishes prepared with quality ingredients and a passion
            for memorable dining.
          </span>
        </div>
      </section>

      <section className="menu-page__content" aria-label="Restaurant dishes">
        <div className="menu-page__intro">
          <div>
            <p className="menu-page__eyebrow">Explore our dishes</p>
            <h2>Something for every taste</h2>
          </div>
          <p>
            Our selection is updated by the restaurant team and loaded directly
            from the menu service.
          </p>
        </div>

        {loading && (
          <div className="menu-page__grid" aria-live="polite" aria-busy="true">
            <p className="visually-hidden">Loading menu...</p>
            {[1, 2, 3].map((placeholder) => (
              <div
                className="menu-page__skeleton"
                key={placeholder}
                aria-hidden="true"
              />
            ))}
          </div>
        )}

        {!loading && error && (
          <div className="menu-page__state menu-page__state--error" role="alert">
            <h2>We could not load the menu</h2>
            <p>{error}</p>
            <button type="button" onClick={() => setReloadKey((key) => key + 1)}>
              Try again
            </button>
          </div>
        )}

        {!loading && !error && items.length === 0 && (
          <div className="menu-page__state">
            <h2>The menu is being prepared</h2>
            <p>Please check back soon for our latest dishes.</p>
          </div>
        )}

        {!loading && !error && items.length > 0 && (
          <div className="menu-page__grid">
            {items.map((item) => (
              <MenuItemCard item={item} key={item.id} />
            ))}
          </div>
        )}
      </section>
    </div>
  )
}

export default MenuPage
