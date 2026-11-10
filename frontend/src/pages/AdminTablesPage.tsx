import { useEffect, useMemo, useState, type FormEvent } from 'react'
import AdminNavigation from '../components/admin/AdminNavigation'
import {
  createAdminTable,
  deactivateAdminTable,
  getAdminTables,
  updateAdminTable,
} from '../services/adminService'
import type { RestaurantTable } from '../types/reservation'
import './AdminWorkspace.css'

const emptyForm = { number: '', capacity: '' }

function AdminTablesPage() {
  const [tables, setTables] = useState<RestaurantTable[]>([])
  const [form, setForm] = useState({ ...emptyForm })
  const [editingId, setEditingId] = useState<number | null>(null)
  const [filter, setFilter] = useState<'All' | 'Active' | 'Inactive'>('All')
  const [isLoading, setIsLoading] = useState(true)
  const [isSaving, setIsSaving] = useState(false)
  const [deactivatingId, setDeactivatingId] = useState<number | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let active = true

    async function loadTables() {
      setIsLoading(true)
      setError('')

      try {
        const data = await getAdminTables()

        if (active) {
          setTables(data)
        }
      } catch (loadError) {
        if (active) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : 'Could not load restaurant tables.',
          )
        }
      } finally {
        if (active) {
          setIsLoading(false)
        }
      }
    }

    void loadTables()

    return () => {
      active = false
    }
  }, [reloadKey])

  const filteredTables = useMemo(
    () =>
      tables.filter((table) => {
        if (filter === 'Active') return table.isActive
        if (filter === 'Inactive') return !table.isActive
        return true
      }),
    [filter, tables],
  )

  const activeTables = tables.filter((table) => table.isActive)
  const totalSeats = activeTables.reduce(
    (total, table) => total + table.capacity,
    0,
  )
  const largestTable = activeTables.reduce(
    (largest, table) => Math.max(largest, table.capacity),
    0,
  )

  function resetForm() {
    setForm({ ...emptyForm })
    setEditingId(null)
  }

  function startEditing(table: RestaurantTable) {
    setEditingId(table.id)
    setForm({
      number: String(table.number),
      capacity: String(table.capacity),
    })
    setError('')
    setSuccess('')
    window.scrollTo({ top: 250, behavior: 'smooth' })
  }

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setError('')
    setSuccess('')

    const number = Number(form.number)
    const capacity = Number(form.capacity)

    if (!Number.isInteger(number) || number < 1 || number > 10_000) {
      setError('Table number must be a whole number between 1 and 10000.')
      return
    }

    if (!Number.isInteger(capacity) || capacity < 1 || capacity > 1_000) {
      setError('Capacity must be a whole number between 1 and 1000.')
      return
    }

    setIsSaving(true)

    try {
      if (editingId === null) {
        const created = await createAdminTable({ number, capacity })
        setTables((current) =>
          [...current, created].sort((first, second) => first.number - second.number),
        )
        setSuccess(`Table ${created.number} was added successfully.`)
      } else {
        const updated = await updateAdminTable(editingId, { number, capacity })
        setTables((current) =>
          current
            .map((table) => (table.id === updated.id ? updated : table))
            .sort((first, second) => first.number - second.number),
        )
        setSuccess(`Table ${updated.number} was updated successfully.`)
      }

      resetForm()
    } catch (saveError) {
      setError(
        saveError instanceof Error
          ? saveError.message
          : 'Could not save the table.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  async function handleDeactivate(table: RestaurantTable) {
    if (!window.confirm(`Deactivate table ${table.number}?`)) {
      return
    }

    setDeactivatingId(table.id)
    setError('')
    setSuccess('')

    try {
      await deactivateAdminTable(table.id)
      setTables((current) =>
        current.map((item) =>
          item.id === table.id ? { ...item, isActive: false } : item,
        ),
      )
      setSuccess(`Table ${table.number} is now inactive.`)

      if (editingId === table.id) {
        resetForm()
      }
    } catch (deactivateError) {
      setError(
        deactivateError instanceof Error
          ? deactivateError.message
          : 'Could not deactivate the table.',
      )
    } finally {
      setDeactivatingId(null)
    }
  }

  return (
    <div className="admin-workspace">
      <section className="admin-workspace__hero admin-workspace__hero--tables">
        <div className="admin-workspace__hero-content">
          <div>
            <p className="admin-workspace__eyebrow">Administration</p>
            <h1>Restaurant tables</h1>
            <p>Shape the dining room and keep seating capacity up to date.</p>
          </div>
          <AdminNavigation />
        </div>
      </section>

      <main className="admin-workspace__content">
        <section className="admin-metrics" aria-label="Table summary">
          <article>
            <span>Active tables</span>
            <strong>{activeTables.length}</strong>
            <small>available for booking</small>
          </article>
          <article>
            <span>Total capacity</span>
            <strong>{totalSeats}</strong>
            <small>active seats</small>
          </article>
          <article>
            <span>Largest table</span>
            <strong>{largestTable}</strong>
            <small>guests at once</small>
          </article>
          <article>
            <span>Inactive</span>
            <strong>{tables.length - activeTables.length}</strong>
            <small>archived tables</small>
          </article>
        </section>

        {error && (
          <div className="admin-alert admin-alert--error" role="alert">
            <span>!</span>
            <p>{error}</p>
            {!isLoading && tables.length === 0 && (
              <button type="button" onClick={() => setReloadKey((key) => key + 1)}>
                Try again
              </button>
            )}
          </div>
        )}

        {success && (
          <div className="admin-alert admin-alert--success" role="status">
            <span>✓</span>
            <p>{success}</p>
          </div>
        )}

        <div className="admin-tables-layout">
          <section className="admin-panel admin-table-form-panel">
            <div className="admin-panel__heading">
              <div>
                <p className="admin-workspace__eyebrow">
                  {editingId === null ? 'New seating' : 'Table details'}
                </p>
                <h2>{editingId === null ? 'Add a table' : 'Edit table'}</h2>
              </div>
            </div>

            <form className="admin-table-form" onSubmit={handleSubmit} noValidate>
              <label>
                <span>Table number</span>
                <input
                  type="number"
                  min="1"
                  max="10000"
                  placeholder="e.g. 12"
                  value={form.number}
                  onChange={(event) =>
                    setForm((current) => ({ ...current, number: event.target.value }))
                  }
                  disabled={isSaving}
                  required
                />
              </label>

              <label>
                <span>Seating capacity</span>
                <input
                  type="number"
                  min="1"
                  max="1000"
                  placeholder="e.g. 4"
                  value={form.capacity}
                  onChange={(event) =>
                    setForm((current) => ({ ...current, capacity: event.target.value }))
                  }
                  disabled={isSaving}
                  required
                />
              </label>

              <button className="admin-form-submit" type="submit" disabled={isSaving}>
                {isSaving
                  ? 'Saving...'
                  : editingId === null
                    ? 'Add table'
                    : 'Save changes'}
                {!isSaving && <span aria-hidden="true">→</span>}
              </button>

              {editingId !== null && (
                <button
                  className="admin-form-cancel"
                  type="button"
                  onClick={resetForm}
                  disabled={isSaving}
                >
                  Cancel editing
                </button>
              )}
            </form>
          </section>

          <section className="admin-panel">
            <div className="admin-panel__heading admin-panel__heading--tables">
              <div>
                <p className="admin-workspace__eyebrow">Dining room</p>
                <h2>Current tables</h2>
              </div>
              <div className="admin-filter-tabs" aria-label="Filter tables">
                {(['All', 'Active', 'Inactive'] as const).map((option) => (
                  <button
                    className={filter === option ? 'is-active' : ''}
                    type="button"
                    onClick={() => setFilter(option)}
                    key={option}
                  >
                    {option}
                  </button>
                ))}
              </div>
            </div>

            {isLoading && (
              <div className="admin-loading" aria-live="polite" aria-busy="true">
                <span />
                <p>Loading tables...</p>
              </div>
            )}

            {!isLoading && !error && tables.length === 0 && (
              <div className="admin-empty admin-empty--compact">
                <h3>No tables configured</h3>
                <p>Add the first table using the form.</p>
              </div>
            )}

            {!isLoading && tables.length > 0 && filteredTables.length === 0 && (
              <div className="admin-empty admin-empty--compact">
                <h3>No {filter.toLowerCase()} tables</h3>
                <p>Choose another filter to see the rest of the dining room.</p>
              </div>
            )}

            {!isLoading && filteredTables.length > 0 && (
              <div className="admin-table-grid">
                {filteredTables.map((table) => (
                  <article
                    className={`admin-table-card${
                      table.isActive ? '' : ' admin-table-card--inactive'
                    }`}
                    key={table.id}
                  >
                    <div className="admin-table-card__top">
                      <span className="admin-table-card__number">{table.number}</span>
                      <span className={`admin-table-card__status${table.isActive ? '' : ' is-inactive'}`}>
                        {table.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </div>
                    <div className="admin-table-card__body">
                      <small>Table {table.number}</small>
                      <strong>{table.capacity}</strong>
                      <span>{table.capacity === 1 ? 'seat' : 'seats'}</span>
                    </div>
                    <div className="admin-table-card__actions">
                      <button
                        type="button"
                        onClick={() => startEditing(table)}
                        disabled={isSaving || deactivatingId !== null}
                      >
                        Edit
                      </button>
                      {table.isActive && (
                        <button
                          className="is-danger"
                          type="button"
                          onClick={() => void handleDeactivate(table)}
                          disabled={isSaving || deactivatingId !== null}
                        >
                          {deactivatingId === table.id ? 'Deactivating...' : 'Deactivate'}
                        </button>
                      )}
                    </div>
                  </article>
                ))}
              </div>
            )}
          </section>
        </div>
      </main>
    </div>
  )
}

export default AdminTablesPage
