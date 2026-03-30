-- ============================================================
-- Catalogs Service - Initial Schema
-- Database: catalogs_db
-- ============================================================

-- Brands
CREATE TABLE IF NOT EXISTS brands (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name            VARCHAR(100)    NOT NULL,
    description     VARCHAR(500),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_brands_name UNIQUE (name)
);

CREATE INDEX ix_brands_name ON brands (name);
CREATE INDEX ix_brands_is_active ON brands (is_active);

-- Categories
CREATE TABLE IF NOT EXISTS categories (
    id                  UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    name                VARCHAR(100)    NOT NULL,
    description         VARCHAR(500),
    parent_category_id  UUID            REFERENCES categories(id),
    is_active           BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at          TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_categories_name UNIQUE (name)
);

CREATE INDEX ix_categories_name ON categories (name);
CREATE INDEX ix_categories_parent_id ON categories (parent_category_id);
CREATE INDEX ix_categories_is_active ON categories (is_active);

-- Products
CREATE TABLE IF NOT EXISTS products (
    id              UUID            PRIMARY KEY DEFAULT gen_random_uuid(),
    sku             VARCHAR(50)     NOT NULL,
    name            VARCHAR(200)    NOT NULL,
    description     VARCHAR(1000),
    price           NUMERIC(18,4)   NOT NULL,
    currency        CHAR(3)         NOT NULL,
    category_id     UUID            NOT NULL REFERENCES categories(id),
    brand_id        UUID            NOT NULL REFERENCES brands(id),
    is_active       BOOLEAN         NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ     NOT NULL DEFAULT NOW(),
    CONSTRAINT uq_products_sku UNIQUE (sku),
    CONSTRAINT chk_products_price CHECK (price >= 0)
);

CREATE UNIQUE INDEX ix_products_sku ON products (sku);
CREATE INDEX ix_products_category_id ON products (category_id);
CREATE INDEX ix_products_brand_id ON products (brand_id);
CREATE INDEX ix_products_is_active ON products (is_active);
CREATE INDEX ix_products_name ON products (name);

-- ============================================================
-- MassTransit Outbox Tables (auto-managed by EF Core)
-- These are created by EF Core migrations, shown here for reference.
-- ============================================================
-- OutboxState, OutboxMessage, InboxState
-- (run EF Core migrations to generate these)
