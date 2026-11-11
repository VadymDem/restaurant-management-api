-- Run in the same psql session as the migration and 02_after.sql.
-- Snapshot existing business records for data-preservation verification.
CREATE TEMP TABLE rrms_reservations_before AS
SELECT id, user_id, table_id, reservation_date, reservation_time,
       guests_count, status, notes, created_at
FROM public."Reservations";
SELECT COUNT(*) AS reservations_before FROM rrms_reservations_before;
SELECT status, COUNT(*) AS count_before
FROM rrms_reservations_before GROUP BY status ORDER BY status;
