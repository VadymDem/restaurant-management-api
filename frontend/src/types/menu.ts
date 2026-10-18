
export interface MenuItem {
  id: number
  itemName: string
  description: string | null
  price: number
}

export interface MenuItemRequest {
  itemName: string
  description: string
  price: number
}
