CREATE TABLE users (
    id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email         VARCHAR(255) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    name          VARCHAR(100) NOT NULL
);

CREATE TABLE tables (
    id       UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    number   INT NOT NULL UNIQUE CHECK (number > 0),
    capacity INT NOT NULL CHECK (capacity > 0)
);

CREATE TABLE menu (
    id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    item_name   VARCHAR(150) NOT NULL,
    description TEXT,
    price       NUMERIC(10, 2) NOT NULL CHECK (price >= 0)
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
