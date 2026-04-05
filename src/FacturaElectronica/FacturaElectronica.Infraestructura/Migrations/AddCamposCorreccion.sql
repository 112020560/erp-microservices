-- Migration: Add correction fields to electronic_invoice table
-- Date: 2026-02-11
-- Description: Add fields for marking invoices that require correction

ALTER TABLE FE.electronic_invoice ADD requiereCorreccion BIT NOT NULL DEFAULT 0;
ALTER TABLE FE.electronic_invoice ADD notasCorreccion VARCHAR(500) NULL;
ALTER TABLE FE.electronic_invoice ADD fechaMarcadoCorreccion DATETIME NULL;

GO
