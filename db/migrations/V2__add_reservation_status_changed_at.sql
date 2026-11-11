-- RRMS educational additive migration.
-- Apply only to a disposable PostgreSQL database cloned from the v1.0 baseline.
-- Does not implement an approval workflow or change the existing reservation status.
BEGIN;
ALTER TABLE public."Reservations"
  ADD COLUMN IF NOT EXISTS status_changed_at TIMESTAMPTZ NULL;
COMMIT;
