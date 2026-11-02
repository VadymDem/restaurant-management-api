export type ReservationStatus =
  | 'Pending'
  | 'Confirmed'
  | 'Cancelled'
  | 'Completed'

export interface RestaurantTable {
  id: number
  number: number
  capacity: number
  location: string | null
  isActive: boolean
}

export interface CreateReservationRequest {
  tableId: number
  reservationDate: string
  reservationTime: string
  guestsCount: number
  notes: string | null
}

export interface Reservation {
  id: number
  userId: string
  tableId: number
  reservationDate: string
  reservationTime: string
  guestsCount: number
  notes: string | null
  status: ReservationStatus
}

export interface AvailabilityFilters {
  date: string
  time: string
  guests: number
}
