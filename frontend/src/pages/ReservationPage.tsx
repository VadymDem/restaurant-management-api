import { useEffect, useMemo, useState, type FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import { createReservation, getAvailableTables } from '../services/reservationService'
import type { RestaurantTable } from '../types/reservation'
import './ReservationPage.css'

function toLocalDateInputValue(date: Date) {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

function ReservationPage() {
  const navigate = useNavigate()
  const today = useMemo(() => toLocalDateInputValue(new Date()), [])
  const [date, setDate] = useState('')
  const [time, setTime] = useState('')
  const [guests, setGuests] = useState(2)
  const [selectedTableId, setSelectedTableId] = useState<number | null>(null)
  const [tables, setTables] = useState<RestaurantTable[]>([])
  const [isLoadingTables, setIsLoadingTables] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [availabilityError, setAvailabilityError] = useState('')
  const [formError, setFormError] = useState('')

  const hasSearchDetails = Boolean(date && time && guests > 0)
  const selectedTable = tables.find((table) => table.id === selectedTableId)

  function resetAvailability() {
    setTables([])
    setSelectedTableId(null)
    setAvailabilityError('')
    setIsLoadingTables(false)
    setFormError('')
  }

  useEffect(() => {
    if (!hasSearchDetails) {
      return
    }

    let active = true
    const timer = window.setTimeout(async () => {
      setIsLoadingTables(true)
      setAvailabilityError('')

      try {
        const availableTables = await getAvailableTables({ date, time, guests })

        if (active) {
          setTables(availableTables)
          setSelectedTableId((currentId) =>
            availableTables.some((table) => table.id === currentId)
              ? currentId
              : null,
          )
        }
      } catch (error) {
        if (active) {
          setTables([])
          setSelectedTableId(null)
          setAvailabilityError(
            error instanceof Error
              ? error.message
              : 'Could not check table availability.',
          )
        }
      } finally {
        if (active) {
          setIsLoadingTables(false)
        }
      }
    }, 250)

    return () => {
      active = false
      window.clearTimeout(timer)
    }
  }, [date, guests, hasSearchDetails, time])

  async function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFormError('')

    if (!date || !time) {
      setFormError('Choose a date and time for your visit.')
      return
    }

    if (new Date(`${date}T${time}`) <= new Date()) {
      setFormError('Please choose a date and time in the future.')
      return
    }

    if (!selectedTableId) {
      setFormError('Select one of the available tables.')
      return
    }

    setIsSubmitting(true)

    try {
      const reservation = await createReservation({
        tableId: selectedTableId,
        reservationDate: date,
        reservationTime: time,
        guestsCount: guests,
        notes: null,
      })

      navigate('/my-reservations', {
        replace: true,
        state: { reservationCreated: true, reservation },
      })
    } catch (error) {
      setFormError(
        error instanceof Error
          ? error.message
          : 'Could not create the reservation. Please try again.',
      )
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="reservation-page">
      <section className="reservation-hero" aria-labelledby="reservation-heading">
        <div className="reservation-hero__content">
          <p className="reservation-eyebrow">A table for every occasion</p>
          <h1 id="reservation-heading">Reserve your evening</h1>
          <p>
            Choose the perfect time, tell us who is joining, and we will take
            care of the rest.
          </p>
        </div>
      </section>

      <section className="reservation-content">
        <div className="reservation-content__intro">
          <div>
            <span>01</span>
            <p>Visit details</p>
          </div>
          <div>
            <span>02</span>
            <p>Choose a table</p>
          </div>
          <div>
            <span>03</span>
            <p>Confirm</p>
          </div>
        </div>

        <form className="reservation-form" onSubmit={handleSubmit} noValidate>
          <div className="reservation-form__section">
            <div className="reservation-form__heading">
              <p className="reservation-eyebrow">Plan your visit</p>
              <h2>When shall we expect you?</h2>
            </div>

            <div className="reservation-fields">
              <label className="reservation-field">
                <span>Date</span>
                <input
                  type="date"
                  min={today}
                  value={date}
                  onChange={(event) => {
                    setDate(event.target.value)
                    resetAvailability()
                  }}
                  disabled={isSubmitting}
                  required
                />
              </label>

              <label className="reservation-field">
                <span>Time</span>
                <input
                  type="time"
                  step="1800"
                  value={time}
                  onChange={(event) => {
                    setTime(event.target.value)
                    resetAvailability()
                  }}
                  disabled={isSubmitting}
                  required
                />
              </label>

              <label className="reservation-field">
                <span>Guests</span>
                <select
                  value={guests}
                  onChange={(event) => {
                    setGuests(Number(event.target.value))
                    resetAvailability()
                  }}
                  disabled={isSubmitting}
                >
                  {Array.from({ length: 12 }, (_, index) => index + 1).map(
                    (guestCount) => (
                      <option value={guestCount} key={guestCount}>
                        {guestCount} {guestCount === 1 ? 'guest' : 'guests'}
                      </option>
                    ),
                  )}
                </select>
              </label>
            </div>
          </div>

          <div className="reservation-form__section reservation-form__section--tables">
            <div className="reservation-form__heading">
              <p className="reservation-eyebrow">Available seating</p>
              <h2>Choose your table</h2>
            </div>

            {!hasSearchDetails && (
              <div className="reservation-state">
                <span aria-hidden="true">✦</span>
                <p>Select a date and time to see available tables.</p>
              </div>
            )}

            {hasSearchDetails && isLoadingTables && (
              <div className="reservation-table-grid" aria-live="polite" aria-busy="true">
                <p className="visually-hidden">Checking available tables...</p>
                {[1, 2, 3].map((item) => (
                  <div className="reservation-table-skeleton" key={item} />
                ))}
              </div>
            )}

            {hasSearchDetails && !isLoadingTables && availabilityError && (
              <div className="reservation-state reservation-state--error" role="alert">
                <p>{availabilityError}</p>
              </div>
            )}

            {hasSearchDetails &&
              !isLoadingTables &&
              !availabilityError &&
              tables.length === 0 && (
                <div className="reservation-state">
                  <span aria-hidden="true">○</span>
                  <p>No tables match these details. Try another time or party size.</p>
                </div>
              )}

            {hasSearchDetails && !isLoadingTables && tables.length > 0 && (
              <div className="reservation-table-grid">
                {tables.map((table) => (
                  <label
                    className={`reservation-table${
                      selectedTableId === table.id ? ' reservation-table--selected' : ''
                    }`}
                    key={table.id}
                  >
                    <input
                      type="radio"
                      name="tableId"
                      value={table.id}
                      checked={selectedTableId === table.id}
                      onChange={() => {
                        setSelectedTableId(table.id)
                        setFormError('')
                      }}
                      disabled={isSubmitting}
                    />
                    <span className="reservation-table__number">{table.number}</span>
                    <span className="reservation-table__details">
                      <strong>Table {table.number}</strong>
                      <small>
                        Up to {table.capacity} guests
                        {table.location ? ` · ${table.location}` : ''}
                      </small>
                    </span>
                    <span className="reservation-table__check" aria-hidden="true">✓</span>
                  </label>
                ))}
              </div>
            )}
          </div>

          <div className="reservation-form__section reservation-form__section--final">
            <aside className="reservation-summary" aria-label="Reservation summary">
              <p>Your reservation</p>
              <dl>
                <div>
                  <dt>Date</dt>
                  <dd>{date || 'Not selected'}</dd>
                </div>
                <div>
                  <dt>Time</dt>
                  <dd>{time || 'Not selected'}</dd>
                </div>
                <div>
                  <dt>Party</dt>
                  <dd>{guests} {guests === 1 ? 'guest' : 'guests'}</dd>
                </div>
                <div>
                  <dt>Table</dt>
                  <dd>{selectedTable ? `No. ${selectedTable.number}` : 'Not selected'}</dd>
                </div>
              </dl>

              {formError && (
                <p className="reservation-form__error" role="alert">{formError}</p>
              )}

              <button
                className="reservation-submit"
                type="submit"
                disabled={isSubmitting || isLoadingTables}
              >
                {isSubmitting ? 'Confirming...' : 'Confirm reservation'}
                {!isSubmitting && <span aria-hidden="true">→</span>}
              </button>
            </aside>
          </div>
        </form>
      </section>
    </div>
  )
}

export default ReservationPage
