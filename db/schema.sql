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

CREATE TABLE tables (
    id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    number   INT NOT NULL UNIQUE CHECK (number > 0),
    capacity INT NOT NULL CHECK (capacity > 0)
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

CREATE TABLE reservations (
    id                    UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    customer_name         VARCHAR(100) NOT NULL,
    customer_email        VARCHAR(255) NOT NULL,
    phone_number          VARCHAR(30)  NOT NULL,
    number_of_people      INT NOT NULL CHECK (number_of_people > 0),
    reservation_date_time TIMESTAMP NOT NULL,
    special_requests      TEXT,
    status                VARCHAR(20) NOT NULL DEFAULT 'pending'
                          CHECK (status IN ('pending', 'confirmed', 'cancelled', 'completed')),
    user_id               UUID REFERENCES users(id) ON DELETE SET NULL,
    table_id              UUID NOT NULL REFERENCES tables(id)
);

CREATE INDEX idx_reservations_user_id   ON reservations(user_id);
CREATE INDEX idx_reservations_table_id  ON reservations(table_id);
CREATE INDEX idx_reservations_date_time ON reservations(reservation_date_time);
