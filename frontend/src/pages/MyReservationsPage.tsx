import { useEffect, useMemo, useState } from 'react'
import { Link, useLocation } from 'react-router-dom'
import { getAvailableTables, getMyReservations } from '../services/reservationService'
import type { Reservation, RestaurantTable } from '../types/reservation'
import './MyReservationsPage.css'

function formatReservationDate(value: string) {
  return new Date(`${value}T00:00:00`).toLocaleDateString('en-GB', {
    weekday: 'short',
    day: 'numeric',
    month: 'long',
    year: 'numeric',
  })
}

function formatReservationTime(value: string) {
  return value.slice(0, 5)
}

function isUpcoming(reservation: Reservation) {
  const reservationDate = new Date(
    `${reservation.reservationDate}T${reservation.reservationTime}`,
  )

  return (
    reservationDate > new Date() &&
    reservation.status !== 'Cancelled' &&
    reservation.status !== 'Completed'
  )
}

function MyReservationsPage() {
  const location = useLocation()
  const [reservations, setReservations] = useState<Reservation[]>([])
  const [tables, setTables] = useState<RestaurantTable[]>([])
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState('')
  const [reloadKey, setReloadKey] = useState(0)

  const showCreatedMessage = Boolean(
    (location.state as { reservationCreated?: boolean } | null)
      ?.reservationCreated,
  )

  useEffect(() => {
    let active = true

    async function loadReservations() {
      setIsLoading(true)
      setError('')

      try {
        const [reservationData, tableData] = await Promise.all([
          getMyReservations(),
          getAvailableTables().catch(() => [] as RestaurantTable[]),
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
              : 'Could not load your reservations.',
          )
        }
      } finally {
        if (active) {
          setIsLoading(false)
        }
      }
    }

    void loadReservations()

    return () => {
      active = false
    }
  }, [reloadKey])

  const tableById = useMemo(
    () => new Map(tables.map((table) => [table.id, table])),
    [tables],
  )

  const upcomingReservations = reservations.filter(isUpcoming)
  const previousReservations = reservations.filter(
    (reservation) => !isUpcoming(reservation),
  )

  function renderReservation(reservation: Reservation) {
    const table = tableById.get(reservation.tableId)

    return (
      <article className="my-reservation-card" key={reservation.id}>
        <div className="my-reservation-card__date" aria-hidden="true">
          <strong>
            {new Date(`${reservation.reservationDate}T00:00:00`).getDate()}
          </strong>
          <span>
            {new Date(`${reservation.reservationDate}T00:00:00`)
              .toLocaleDateString('en-GB', { month: 'short' })}
          </span>
        </div>

        <div className="my-reservation-card__content">
          <div className="my-reservation-card__title">
            <div>
              <p>{formatReservationDate(reservation.reservationDate)}</p>
              <h3>{formatReservationTime(reservation.reservationTime)}</h3>
            </div>
            <span
              className={`reservation-status reservation-status--${reservation.status.toLowerCase()}`}
            >
              {reservation.status}
            </span>
          </div>

          <div className="my-reservation-card__meta">
            <span>
              <small>Party</small>
              <strong>
                {reservation.guestsCount}{' '}
                {reservation.guestsCount === 1 ? 'guest' : 'guests'}
              </strong>
            </span>
            <span>
              <small>Table</small>
              <strong>{table ? `No. ${table.number}` : `ID ${reservation.tableId}`}</strong>
            </span>
            {table?.location && (
              <span>
                <small>Location</small>
                <strong>{table.location}</strong>
              </span>
            )}
          </div>

        </div>

        <span className="my-reservation-card__reference">
          #{String(reservation.id).padStart(4, '0')}
        </span>
      </article>
    )
  }

  return (
    <div className="my-reservations-page">
      <section className="my-reservations-hero">
        <div>
          <p className="reservation-eyebrow">Your visits</p>
          <h1>My reservations</h1>
          <p>Everything you need for your next evening with us, in one place.</p>
        </div>
        <Link to="/reservations">
          Reserve another table <span aria-hidden="true">→</span>
        </Link>
      </section>

      <section className="my-reservations-content" aria-live="polite">
        {showCreatedMessage && (
          <div className="my-reservations-success" role="status">
            <span aria-hidden="true">✓</span>
            <div>
              <strong>Your reservation has been received.</strong>
              <p>We look forward to welcoming you.</p>
            </div>
          </div>
        )}

        {isLoading && (
          <div className="my-reservations-loading" aria-busy="true">
            <div />
            <div />
          </div>
        )}

        {!isLoading && error && (
          <div className="my-reservations-empty my-reservations-empty--error" role="alert">
            <span aria-hidden="true">!</span>
            <h2>We could not load your reservations</h2>
            <p>{error}</p>
            <button type="button" onClick={() => setReloadKey((key) => key + 1)}>
              Try again
            </button>
          </div>
        )}

        {!isLoading && !error && reservations.length === 0 && (
          <div className="my-reservations-empty">
            <span aria-hidden="true">R</span>
            <h2>Your table is waiting</h2>
            <p>You have no reservations yet. Plan your first visit with us.</p>
            <Link to="/reservations">Make a reservation</Link>
          </div>
        )}

        {!isLoading && !error && upcomingReservations.length > 0 && (
          <div className="my-reservations-group">
            <div className="my-reservations-group__heading">
              <div>
                <p className="reservation-eyebrow">Coming up</p>
                <h2>Upcoming reservations</h2>
              </div>
              <span>{upcomingReservations.length}</span>
            </div>
            <div className="my-reservations-list">
              {upcomingReservations.map(renderReservation)}
            </div>
          </div>
        )}

        {!isLoading && !error && previousReservations.length > 0 && (
          <div className="my-reservations-group my-reservations-group--past">
            <div className="my-reservations-group__heading">
              <div>
                <p className="reservation-eyebrow">Your history</p>
                <h2>Previous reservations</h2>
              </div>
              <span>{previousReservations.length}</span>
            </div>
            <div className="my-reservations-list">
              {previousReservations.map(renderReservation)}
            </div>
          </div>
        )}
      </section>
    </div>
  )
}

export default MyReservationsPage
