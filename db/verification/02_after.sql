-- Only valid after 01_before.sql and the migration, in the same psql session.
DO $$
DECLARE
  missing_rows BIGINT;
  added_rows BIGINT;
  column_exists BOOLEAN;
  nonnull_new_values BIGINT;
BEGIN
  SELECT EXISTS (
    SELECT 1 FROM information_schema.columns
    WHERE table_schema='public' AND table_name='Reservations'
      AND column_name='status_changed_at' AND data_type='timestamp with time zone'
  ) INTO column_exists;
  IF NOT column_exists THEN
    RAISE EXCEPTION 'Migration check failed: status_changed_at TIMESTAMPTZ is missing';
  END IF;

  SELECT COUNT(*) INTO missing_rows FROM (
    SELECT id, user_id, table_id, reservation_date, reservation_time,
           guests_count, status, notes, created_at FROM rrms_reservations_before
    EXCEPT
    SELECT id, user_id, table_id, reservation_date, reservation_time,
           guests_count, status, notes, created_at FROM public."Reservations"
  ) q;
  SELECT COUNT(*) INTO added_rows FROM (
    SELECT id, user_id, table_id, reservation_date, reservation_time,
           guests_count, status, notes, created_at FROM public."Reservations"
    EXCEPT
    SELECT id, user_id, table_id, reservation_date, reservation_time,
           guests_count, status, notes, created_at FROM rrms_reservations_before
  ) q;
  SELECT COUNT(*) INTO nonnull_new_values FROM public."Reservations" WHERE status_changed_at IS NOT NULL;
  IF missing_rows <> 0 OR added_rows <> 0 OR nonnull_new_values <> 0 THEN
    RAISE EXCEPTION 'Migration validation failed: missing %, added/modified %, populated new field %', missing_rows, added_rows, nonnull_new_values;
  END IF;
  RAISE NOTICE 'PASS: existing reservation rows and fields preserved; nullable column added';
END $$;
SELECT COUNT(*) AS reservations_after FROM public."Reservations";
SELECT status, COUNT(*) AS count_after
FROM public."Reservations" GROUP BY status ORDER BY status;
