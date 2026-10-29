CREATE TABLE users (
    id            UUID NOT NULL DEFAULT gen_random_uuid(),
    name          VARCHAR(100) NOT NULL,
    email         VARCHAR(255) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    role          VARCHAR(20) NOT NULL DEFAULT 'User',
    created_at    TIMESTAMP NOT NULL DEFAULT now(),
    CONSTRAINT users_pkey PRIMARY KEY (id)
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email ON users (email);


CREATE TABLE public."Tables" (
    id BIGINT GENERATED ALWAYS AS IDENTITY NOT NULL,
    table_number INTEGER NOT NULL,
    capacity INTEGER NOT NULL,
    location TEXT,
    is_active BOOLEAN NOT NULL DEFAULT true,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT "Tables_pkey" PRIMARY KEY (id),
    CONSTRAINT "Tables_table_number_key" UNIQUE (table_number),
    CONSTRAINT "Tables_capacity_check" CHECK (capacity > 0)
);

CREATE TABLE "MenuItems" (
    id           BIGINT GENERATED ALWAYS AS IDENTITY NOT NULL,
    name         TEXT NOT NULL,
    description  TEXT NULL,
    price        NUMERIC(10, 2) NOT NULL,
    category     TEXT NULL,
    image_url    TEXT NULL,
    is_available BOOLEAN NOT NULL DEFAULT true,
    created_at   TIMESTAMPTZ NOT NULL DEFAULT now(),
    CONSTRAINT "MenuItems_pkey" PRIMARY KEY (id),
    CONSTRAINT "MenuItems_price_check" CHECK (price > 0)
);

CREATE TABLE admins (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username      VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL
);


CREATE TABLE public."Reservations" (
    id BIGINT GENERATED ALWAYS AS IDENTITY NOT NULL,
    user_id UUID NOT NULL,
    table_id BIGINT NOT NULL,
    reservation_date DATE NOT NULL,
    reservation_time TIME WITHOUT TIME ZONE NOT NULL,
    guests_count INTEGER NOT NULL,
    status TEXT NOT NULL DEFAULT 'Pending',
    notes TEXT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT "Reservations_pkey"
        PRIMARY KEY (id),

    CONSTRAINT "Reservations_table_id_fkey"
        FOREIGN KEY (table_id)
        REFERENCES public."Tables"(id)
        ON DELETE RESTRICT,

    CONSTRAINT "Reservations_user_id_fkey"
        FOREIGN KEY (user_id)
        REFERENCES public.users(id)
        ON DELETE CASCADE,

    CONSTRAINT "Reservations_guests_count_check"
        CHECK (guests_count > 0),

    CONSTRAINT "Reservations_status_check"
        CHECK (status IN (
            'Pending',
            'Confirmed',
            'Cancelled',
            'Completed'
        ))
);

CREATE INDEX IF NOT EXISTS ix_reservations_user_id
ON public."Reservations" (user_id);

CREATE INDEX IF NOT EXISTS ix_reservations_table_id
ON public."Reservations" (table_id);

CREATE INDEX IF NOT EXISTS ix_reservations_date
ON public."Reservations" (reservation_date);

CREATE UNIQUE INDEX IF NOT EXISTS ux_reservations_table_slot
ON public."Reservations" (
    table_id,
    reservation_date,
    reservation_time
)
WHERE status <> 'Cancelled';

CREATE OR REPLACE FUNCTION public.check_reservation_table()
RETURNS TRIGGER
LANGUAGE plpgsql
AS $function$
DECLARE
    v_capacity INTEGER;
    v_active BOOLEAN;
BEGIN
    SELECT capacity, is_active
    INTO v_capacity, v_active
    FROM public."Tables"
    WHERE id = NEW.table_id;

    IF NOT FOUND THEN
        RETURN NEW;
    END IF;

    IF NEW.guests_count > v_capacity THEN
        RAISE EXCEPTION
            'Кількість гостей (%) перевищує місткість столика (%)',
            NEW.guests_count, v_capacity
            USING ERRCODE = 'check_violation';
    END IF;

    IF v_active = false AND NEW.status <> 'Cancelled' THEN
        RAISE EXCEPTION
            'Столик % зараз неактивний',
            NEW.table_id
            USING ERRCODE = 'check_violation';
    END IF;

    RETURN NEW;
END;
$function$;

CREATE TRIGGER trg_reservations_check_table
BEFORE INSERT OR UPDATE
ON public."Reservations"
FOR EACH ROW
EXECUTE FUNCTION public.check_reservation_table();
