-- ONLY on a disposable educational database and ONLY before applications
-- begin writing data to status_changed_at; otherwise this drops those values.
BEGIN;
ALTER TABLE public."Reservations"
  DROP COLUMN IF EXISTS status_changed_at;
COMMIT;
