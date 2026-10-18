
import { useEffect, useState, type FormEvent } from 'react'
import {
  createMenuItem,
  deleteMenuItem,
  getMenuItems,
  updateMenuItem,
} from '../services/menuService'
import type { MenuItem, MenuItemRequest } from '../types/menu'
import './AdminMenuPage.css'

const emptyForm = {
  itemName: '',
  description: '',
  price: '',
}

function AdminMenuPage() {
  const [items, setItems] = useState<MenuItem[]>([])
  const [form, setForm] = useState({ ...emptyForm })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [deletingId, setDeletingId] = useState<number | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')

  useEffect(() => {
    let active = true

    async function loadMenu() {
      try {
        const data = await getMenuItems()
        if (active) {
          setItems(data)
        }
      } catch (err) {
        if (active) {
          setError(
            err instanceof Error ? err.message : 'Failed to load menu.',
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
  }, [])

  function resetForm() {
    setForm({ ...emptyForm })
    setEditingId(null)
  }

  function handleEdit(item: MenuItem) {
    setEditingId(item.id)
    setForm({
      itemName: item.itemName,
      description: item.description ?? '',
      price: String(item.price),
    })
    setError('')
    setSuccess('')
    window.scrollTo({ top: 0, behavior: 'smooth' })
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setSuccess('')

    const name = form.itemName.trim()
    const priceText = form.price.trim()
    const price = Number(priceText)

    if (!name) {
      setError('Menu item name is required.')
      return
    }

    if (
      !priceText ||
      !Number.isFinite(price) ||
      price < 0.01 ||
      price > 1000000 ||
      Math.round(price * 100) !== price * 100
    ) {
      setError('Enter a valid price between 0.01 and 1000000.00 with up to two decimal places.')
      return
    }

    const request: MenuItemRequest = {
      itemName: name,
      description: form.description.trim(),
      price,
    }

    setSaving(true)

    try {
      if (editingId !== null) {
        const updated = await updateMenuItem(editingId, request)
        setItems((current) =>
          current.map((item) => (item.id === editingId ? updated : item)),
        )
        setSuccess('Menu item updated successfully.')
      } else {
        const created = await createMenuItem(request)
        setItems((current) => [...current, created])
        setSuccess('Menu item created successfully.')
      }

      resetForm()
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to save menu item.',
      )
    } finally {
      setSaving(false)
    }
  }

  async function handleDelete(item: MenuItem) {
    const confirmed = window.confirm(
      `Are you sure you want to delete "${item.itemName}"?`,
    )

    if (!confirmed) {
      return
    }

    setDeletingId(item.id)
    setError('')
    setSuccess('')

    try {
      await deleteMenuItem(item.id)
      setItems((current) =>
        current.filter((existing) => existing.id !== item.id),
      )

      if (editingId === item.id) {
        resetForm()
      }

      setSuccess('Menu item deleted successfully.')
    } catch (err) {
      setError(
        err instanceof Error ? err.message : 'Failed to delete menu item.',
      )
    } finally {
      setDeletingId(null)
    }
  }

  return (
    <section className="admin-menu">
      <div className="admin-menu__heading">
        <p className="admin-menu__eyebrow">Administration</p>
        <h1>Menu Management</h1>
        <p>Create, edit, and delete restaurant menu items.</p>
      </div>

      {error && (
        <p className="admin-menu__message admin-menu__message--error" role="alert">
          {error}
        </p>
      )}

      {success && (
        <p className="admin-menu__message admin-menu__message--success" role="status">
          {success}
        </p>
      )}

      <div className="admin-menu__panel">
        <h2>{editingId !== null ? 'Edit Menu Item' : 'Add Menu Item'}</h2>

        <form onSubmit={handleSubmit} className="admin-menu__form">
          <label>
            Item name
            <input
              type="text"
              value={form.itemName}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  itemName: event.target.value,
                }))
              }
              required
              maxLength={200}
              disabled={saving}
            />
          </label>

          <label>
            Description
            <textarea
              value={form.description}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  description: event.target.value,
                }))
              }
              rows={3}
              disabled={saving}
            />
          </label>

          <label>
            Price
            <input
              type="number"
              min="0.01"
              max="1000000"
              step="0.01"
              value={form.price}
              onChange={(event) =>
                setForm((current) => ({
                  ...current,
                  price: event.target.value,
                }))
              }
              required
              disabled={saving}
            />
          </label>

          <div className="admin-menu__actions">
            <button
              type="submit"
              className="admin-menu__button admin-menu__button--primary"
              disabled={saving}
            >
              {saving
                ? 'Saving...'
                : editingId !== null
                  ? 'Save Changes'
                  : 'Add Item'}
            </button>

            {editingId !== null && (
              <button
                type="button"
                className="admin-menu__button"
                onClick={resetForm}
                disabled={saving}
              >
                Cancel
              </button>
            )}
          </div>
        </form>
      </div>

      <div className="admin-menu__panel">
        <h2>Current Menu</h2>

        {loading ? (
          <p>Loading menu...</p>
        ) : items.length === 0 ? (
          <p>No menu items found.</p>
        ) : (
          <div className="admin-menu__table-wrapper">
            <table className="admin-menu__table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Description</th>
                  <th>Price</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {items.map((item) => (
                  <tr key={item.id}>
                    <td>{item.itemName}</td>
                    <td>{item.description || '—'}</td>
                    <td>{item.price.toFixed(2)}</td>
                    <td>
                      <div className="admin-menu__row-actions">
                        <button
                          type="button"
                          className="admin-menu__button"
                          onClick={() => handleEdit(item)}
                          disabled={saving || deletingId !== null}
                        >
                          Edit
                        </button>
                        <button
                          type="button"
                          className="admin-menu__button admin-menu__button--danger"
                          onClick={() => void handleDelete(item)}
                          disabled={saving || deletingId !== null}
                        >
                          {deletingId === item.id ? 'Deleting...' : 'Delete'}
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </section>
  )
}

export default AdminMenuPage