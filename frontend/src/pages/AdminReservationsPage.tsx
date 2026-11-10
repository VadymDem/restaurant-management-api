import { useEffect, useMemo, useState } from 'react'
import AdminNavigation from '../components/admin/AdminNavigation'
import {
  getAdminReservations,
  getAdminTables,
  updateAdminReservationStatus,
} from '../services/adminService'
import type {
  Reservation,
  ReservationStatus,
  RestaurantTable,
} from '../types/reservation'
import './AdminWorkspace.css'

const statusFilters: Array<'All' | ReservationStatus> = [
  'All',
  'Pending',
  'Confirmed',
  'Completed',
  'Cancelled',
]

function formatDate(value: string) {
  return new Date(`${value}T00:00:00`).toLocaleDateString('en-GB', {
    day: 'numeric',
    month: 'short',
    year: 'numeric',
  })
}

function isToday(value: string) {
  const now = new Date()
  const localDate = [
    now.getFullYear(),
    String(now.getMonth() + 1).padStart(2, '0'),
    String(now.getDate()).padStart(2, '0'),
  ].join('-')

  return value === localDate
}

function AdminReservationsPage() {
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [tables, setTables] = useState<RestaurantTable[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [updatingId, setUpdatingId] = useState<number | null>(null)
  const [error, setError] = useState('')
  const [success, setSuccess] = useState('')
  const [statusFilter, setStatusFilter] = useState<'All' | ReservationStatus>('All')
  const [dateFilter, setDateFilter] = useState('')
  const [search, setSearch] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  useEffect(() => {
    let active = true

    async function loadData() {
      setIsLoading(true)
      setError('')

      try {
        const [reservationData, tableData] = await Promise.all([
          getAdminReservations(),
          getAdminTables(),
        ])

        if (active) {
          setReservations(reservationData)
          setTables(tableData)
        }
      } catch (loadError) {
        if (active) {
          setError(
            loadError instanceof Error
              ? loadError.message
              : 'Could not load reservations.',
          )
        }
      } finally {
        if (active) {
          setIsLoading(false)
        }
      }
    }

    void loadData()

    return () => {
      active = false
    }
  }, [reloadKey])

  const tableById = useMemo(
    () => new Map(tables.map((table) => [table.id, table])),
    [tables],
  )

  const filteredReservations = useMemo(() => {
    const normalizedSearch = search.trim().toLowerCase()

    return reservations.filter((reservation) => {
      const table = tableById.get(reservation.tableId)
      const matchesStatus =
        statusFilter === 'All' || reservation.status === statusFilter
      const matchesDate =
        !dateFilter || reservation.reservationDate === dateFilter
      const matchesSearch =
        !normalizedSearch ||
        String(reservation.id).includes(normalizedSearch) ||
        reservation.userId.toLowerCase().includes(normalizedSearch) ||
        String(table?.number ?? reservation.tableId).includes(normalizedSearch)

      return matchesStatus && matchesDate && matchesSearch
    })
  }, [dateFilter, reservations, search, statusFilter, tableById])

  const pendingCount = reservations.filter(
    (reservation) => reservation.status === 'Pending',
  ).length
  const confirmedCount = reservations.filter(
    (reservation) => reservation.status === 'Confirmed',
  ).length
  const todayCount = reservations.filter((reservation) =>
    isToday(reservation.reservationDate),
  ).length

  async function handleStatusChange(
    reservation: Reservation,
    status: ReservationStatus,
  ) {
    if (
      status === 'Cancelled' &&
      !window.confirm(`Cancel reservation #${reservation.id}?`)
    ) {
      return
    }

    setUpdatingId(reservation.id)
    setError('')
    setSuccess('')

    try {
      const updated = await updateAdminReservationStatus(
        reservation.id,
        status,
      )
      setReservations((current) =>
        current.map((item) => (item.id === updated.id ? updated : item)),
      )
      setSuccess(`Reservation #${reservation.id} is now ${status.toLowerCase()}.`)
    } catch (updateError) {
      setError(
        updateError instanceof Error
          ? updateError.message
          : 'Could not update reservation status.',
      )
    } finally {
      setUpdatingId(null)
    }
  }

  function clearFilters() {
    setStatusFilter('All')
    setDateFilter('')
    setSearch('')
  }

  return (
    <div className="admin-workspace">
      <section className="admin-workspace__hero">
        <div className="admin-workspace__hero-content">
          <div>
            <p className="admin-workspace__eyebrow">Administration</p>
            <h1>Reservations</h1>
            <p>Review every booking and keep each table running on time.</p>
          </div>
          <AdminNavigation />
        </div>
      </section>

      <main className="admin-workspace__content">
        <section className="admin-metrics" aria-label="Reservation summary">
          <article>
            <span>Today</span>
            <strong>{todayCount}</strong>
            <small>scheduled visits</small>
          </article>
          <article>
            <span>Pending</span>
            <strong>{pendingCount}</strong>
            <small>need a decision</small>
          </article>
          <article>
            <span>Confirmed</span>
            <strong>{confirmedCount}</strong>
            <small>ready to welcome</small>
          </article>
          <article>
            <span>All bookings</span>
            <strong>{reservations.length}</strong>
            <small>total records</small>
          </article>
        </section>

        {error && (
          <div className="admin-alert admin-alert--error" role="alert">
            <span>!</span>
            <p>{error}</p>
            {isLoading === false && reservations.length === 0 && (
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

        <section className="admin-panel">
          <div className="admin-panel__heading">
            <div>
              <p className="admin-workspace__eyebrow">Booking list</p>
              <h2>Manage reservations</h2>
            </div>
            <span>{filteredReservations.length} shown</span>
          </div>

          <div className="admin-toolbar">
            <div className="admin-filter-tabs" aria-label="Filter by status">
              {statusFilters.map((status) => (
                <button
                  className={statusFilter === status ? 'is-active' : ''}
                  type="button"
                  onClick={() => setStatusFilter(status)}
                  key={status}
                >
                  {status}
                </button>
              ))}
            </div>

            <div className="admin-toolbar__fields">
              <label>
                <span className="visually-hidden">Search reservations</span>
                <input
                  type="search"
                  placeholder="Search ID, customer or table"
                  value={search}
                  onChange={(event) => setSearch(event.target.value)}
                />
              </label>
              <label>
                <span className="visually-hidden">Filter by date</span>
                <input
                  type="date"
                  value={dateFilter}
                  onChange={(event) => setDateFilter(event.target.value)}
                />
              </label>
            </div>
          </div>

          {isLoading && (
            <div className="admin-loading" aria-live="polite" aria-busy="true">
              <span />
              <p>Loading reservations...</p>
            </div>
          )}

          {!isLoading && !error && reservations.length === 0 && (
            <div className="admin-empty">
              <span>R</span>
              <h3>No reservations yet</h3>
              <p>New bookings will appear here as soon as guests reserve a table.</p>
            </div>
          )}

          {!isLoading && reservations.length > 0 && filteredReservations.length === 0 && (
            <div className="admin-empty admin-empty--compact">
              <h3>No matching reservations</h3>
              <p>Try changing or clearing the current filters.</p>
              <button type="button" onClick={clearFilters}>Clear filters</button>
            </div>
          )}

          {!isLoading && filteredReservations.length > 0 && (
            <div className="admin-reservation-list">
              {filteredReservations.map((reservation) => {
                const table = tableById.get(reservation.tableId)
                const isTerminal =
                  reservation.status === 'Cancelled' ||
                  reservation.status === 'Completed'

                return (
                  <article className="admin-reservation" key={reservation.id}>
                    <div className="admin-reservation__when">
                      <span>{formatDate(reservation.reservationDate)}</span>
                      <strong>{reservation.reservationTime.slice(0, 5)}</strong>
                    </div>

                    <div className="admin-reservation__main">
                      <div className="admin-reservation__title">
                        <div>
                          <small>Reservation #{reservation.id}</small>
                          <h3>Table {table?.number ?? reservation.tableId}</h3>
                        </div>
                        <span
                          className={`reservation-status reservation-status--${reservation.status.toLowerCase()}`}
                        >
                          {reservation.status}
                        </span>
                      </div>

                      <dl className="admin-reservation__details">
                        <div>
                          <dt>Customer</dt>
                          <dd title={reservation.userId}>
                            {reservation.userId.slice(0, 8)}…
                          </dd>
                        </div>
                        <div>
                          <dt>Party size</dt>
                          <dd>{reservation.guestsCount} guests</dd>
                        </div>
                        <div>
                          <dt>Capacity</dt>
                          <dd>{table ? `${table.capacity} seats` : '—'}</dd>
                        </div>
                      </dl>

                    </div>

                    <div className="admin-reservation__actions">
                      {isTerminal ? (
                        <span>No actions available</span>
                      ) : (
                        <>
                          {reservation.status === 'Pending' && (
                            <button
                              className="admin-action admin-action--primary"
                              type="button"
                              disabled={updatingId === reservation.id}
                              onClick={() => void handleStatusChange(reservation, 'Confirmed')}
                            >
                              Confirm
                            </button>
                          )}
                          <button
                            className="admin-action"
                            type="button"
                            disabled={updatingId === reservation.id}
                            onClick={() => void handleStatusChange(reservation, 'Completed')}
                          >
                            Complete
                          </button>
                          <button
                            className="admin-action admin-action--danger"
                            type="button"
                            disabled={updatingId === reservation.id}
                            onClick={() => void handleStatusChange(reservation, 'Cancelled')}
                          >
                            Cancel
                          </button>
                        </>
                      )}
                    </div>
                  </article>
                )
              })}
            </div>
          )}
        </section>
      </main>
    </div>
  )
}

export default AdminReservationsPage
