import type { MenuItem } from '../../types/menu'
import './MenuItemCard.css'

interface MenuItemCardProps {
  item: MenuItem
}

const priceFormatter = new Intl.NumberFormat('en-US', {
  minimumFractionDigits: 2,
  maximumFractionDigits: 2,
})

function MenuItemCard({ item }: MenuItemCardProps) {
  return (
    <article className="menu-item-card">
      <div className="menu-item-card__content">
        <p className="menu-item-card__eyebrow">Chef&apos;s selection</p>
        <h2>{item.itemName}</h2>
        <p className="menu-item-card__description">
          {item.description?.trim() ||
            'A carefully prepared dish from our restaurant menu.'}
        </p>
      </div>

      <div className="menu-item-card__price">
        <span>Price</span>
        <strong>{priceFormatter.format(item.price)}</strong>
      </div>
    </article>
  )
}

export default MenuItemCard
