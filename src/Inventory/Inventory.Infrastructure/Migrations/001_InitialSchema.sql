-- Inventory Service - Initial Schema
-- Database: inventory_db

CREATE TABLE IF NOT EXISTS warehouses (
    id UUID PRIMARY KEY,
    code VARCHAR(20) NOT NULL,
    name VARCHAR(200) NOT NULL,
    description VARCHAR(1000),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT ix_warehouses_code UNIQUE (code)
);

CREATE INDEX IF NOT EXISTS ix_warehouses_is_active ON warehouses(is_active);

CREATE TABLE IF NOT EXISTS warehouse_locations (
    id UUID PRIMARY KEY,
    warehouse_id UUID NOT NULL REFERENCES warehouses(id),
    aisle VARCHAR(20) NOT NULL,
    rack VARCHAR(20) NOT NULL,
    level VARCHAR(20) NOT NULL,
    name VARCHAR(100),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT ix_warehouse_locations_unique UNIQUE (warehouse_id, aisle, rack, level)
);

CREATE INDEX IF NOT EXISTS ix_warehouse_locations_warehouse_id ON warehouse_locations(warehouse_id);

CREATE TABLE IF NOT EXISTS product_snapshots (
    product_id UUID PRIMARY KEY,
    sku VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    category_id UUID NOT NULL,
    brand_id UUID NOT NULL,
    tracking_type INT NOT NULL DEFAULT 0,
    minimum_stock NUMERIC(18,4) NOT NULL DEFAULT 0,
    reorder_point NUMERIC(18,4) NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    last_synced_at TIMESTAMPTZ NOT NULL
);

CREATE TABLE IF NOT EXISTS lots (
    id UUID PRIMARY KEY,
    lot_number VARCHAR(100) NOT NULL,
    product_id UUID NOT NULL,
    manufacturing_date DATE,
    expiration_date DATE,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT ix_lots_lotnumber_productid UNIQUE (lot_number, product_id)
);

CREATE TABLE IF NOT EXISTS stock_entries (
    id UUID PRIMARY KEY,
    product_id UUID NOT NULL,
    warehouse_id UUID NOT NULL,
    location_id UUID NOT NULL,
    lot_id UUID,
    quantity_on_hand NUMERIC(18,4) NOT NULL DEFAULT 0,
    quantity_reserved NUMERIC(18,4) NOT NULL DEFAULT 0,
    average_cost NUMERIC(18,4) NOT NULL DEFAULT 0,
    minimum_stock NUMERIC(18,4) NOT NULL DEFAULT 0,
    reorder_point NUMERIC(18,4) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT ix_stock_entries_unique UNIQUE (product_id, warehouse_id, location_id, lot_id)
);

CREATE INDEX IF NOT EXISTS ix_stock_entries_product_id ON stock_entries(product_id);
CREATE INDEX IF NOT EXISTS ix_stock_entries_warehouse_id ON stock_entries(warehouse_id);

CREATE TABLE IF NOT EXISTS stock_reservations (
    id UUID PRIMARY KEY,
    reservation_number VARCHAR(30) NOT NULL,
    product_id UUID NOT NULL,
    warehouse_id UUID NOT NULL,
    location_id UUID NOT NULL,
    lot_id UUID,
    reserved_quantity NUMERIC(18,4) NOT NULL,
    sales_order_id UUID NOT NULL,
    status INT NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL,
    updated_at TIMESTAMPTZ NOT NULL,
    expires_at TIMESTAMPTZ,
    CONSTRAINT ix_stock_reservations_number UNIQUE (reservation_number)
);

CREATE INDEX IF NOT EXISTS ix_stock_reservations_sales_order_id ON stock_reservations(sales_order_id);
CREATE INDEX IF NOT EXISTS ix_stock_reservations_status ON stock_reservations(status);

CREATE TABLE IF NOT EXISTS inventory_movements (
    id UUID PRIMARY KEY,
    movement_number VARCHAR(30) NOT NULL,
    movement_type INT NOT NULL,
    status INT NOT NULL DEFAULT 0,
    warehouse_id UUID NOT NULL,
    destination_warehouse_id UUID,
    reference VARCHAR(100),
    notes VARCHAR(1000),
    date TIMESTAMPTZ NOT NULL,
    confirmed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT ix_inventory_movements_number UNIQUE (movement_number)
);

CREATE INDEX IF NOT EXISTS ix_inventory_movements_warehouse_id ON inventory_movements(warehouse_id);
CREATE INDEX IF NOT EXISTS ix_inventory_movements_date ON inventory_movements(date);
CREATE INDEX IF NOT EXISTS ix_inventory_movements_status ON inventory_movements(status);

CREATE TABLE IF NOT EXISTS movement_lines (
    id UUID PRIMARY KEY,
    movement_id UUID NOT NULL REFERENCES inventory_movements(id),
    product_id UUID NOT NULL,
    source_location_id UUID NOT NULL,
    destination_location_id UUID,
    lot_id UUID,
    quantity NUMERIC(18,4) NOT NULL,
    unit_cost NUMERIC(18,4) NOT NULL,
    notes VARCHAR(500)
);

CREATE INDEX IF NOT EXISTS ix_movement_lines_movement_id ON movement_lines(movement_id);
CREATE INDEX IF NOT EXISTS ix_movement_lines_product_id ON movement_lines(product_id);

CREATE TABLE IF NOT EXISTS physical_counts (
    id UUID PRIMARY KEY,
    count_number VARCHAR(30) NOT NULL,
    warehouse_id UUID NOT NULL,
    status INT NOT NULL DEFAULT 0,
    started_at TIMESTAMPTZ NOT NULL,
    completed_at TIMESTAMPTZ,
    notes VARCHAR(1000),
    CONSTRAINT ix_physical_counts_number UNIQUE (count_number)
);

CREATE INDEX IF NOT EXISTS ix_physical_counts_warehouse_id ON physical_counts(warehouse_id);

CREATE TABLE IF NOT EXISTS count_lines (
    id UUID PRIMARY KEY,
    count_id UUID NOT NULL REFERENCES physical_counts(id),
    product_id UUID NOT NULL,
    location_id UUID NOT NULL,
    lot_id UUID,
    system_quantity NUMERIC(18,4) NOT NULL,
    counted_quantity NUMERIC(18,4),
    is_adjusted BOOLEAN NOT NULL DEFAULT FALSE
);

CREATE INDEX IF NOT EXISTS ix_count_lines_count_id ON count_lines(count_id);

-- MassTransit Outbox tables
CREATE TABLE IF NOT EXISTS outbox_state (
    outbox_id UUID NOT NULL,
    lock_id UUID NOT NULL,
    row_lock_id UUID NOT NULL,
    inbox_message_id UUID,
    inbox_consumer_id UUID,
    last_sequence_number BIGINT,
    created TIMESTAMPTZ NOT NULL,
    delivered TIMESTAMPTZ,
    last_delivered_sequence_number BIGINT,
    delivery_count INT,
    max_delivery_count INT NOT NULL,
    delivery_error TEXT,
    row_version BYTEA,
    PRIMARY KEY (outbox_id)
);

CREATE TABLE IF NOT EXISTS outbox_message (
    sequence_number BIGSERIAL NOT NULL,
    enqueue_time TIMESTAMPTZ,
    sent_time TIMESTAMPTZ NOT NULL,
    headers TEXT,
    properties TEXT,
    inbox_message_id UUID,
    inbox_consumer_id UUID,
    outbox_id UUID,
    message_id UUID NOT NULL,
    content_type VARCHAR(256) NOT NULL,
    message_type TEXT NOT NULL,
    body TEXT NOT NULL,
    conversation_id UUID,
    correlation_id UUID,
    initiator_id UUID,
    request_id UUID,
    source_address TEXT,
    destination_address TEXT,
    response_address TEXT,
    fault_address TEXT,
    expiration_time TIMESTAMPTZ
);

CREATE TABLE IF NOT EXISTS inbox_state (
    id BIGSERIAL NOT NULL,
    message_id UUID NOT NULL,
    consumer_id UUID NOT NULL,
    lock_id UUID NOT NULL,
    row_lock_id UUID NOT NULL,
    received TIMESTAMPTZ NOT NULL,
    receive_count INT NOT NULL,
    expiration_time TIMESTAMPTZ,
    consumed TIMESTAMPTZ,
    delivered TIMESTAMPTZ,
    last_sequence_number BIGINT,
    row_version BYTEA,
    PRIMARY KEY (id),
    CONSTRAINT uq_inbox_state_message_consumer UNIQUE (message_id, consumer_id)
);
