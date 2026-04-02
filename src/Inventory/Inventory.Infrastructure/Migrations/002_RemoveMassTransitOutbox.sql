-- Remove MassTransit outbox tables (replaced by custom transactional outbox)
DROP TABLE IF EXISTS outbox_message;
DROP TABLE IF EXISTS outbox_state;
DROP TABLE IF EXISTS inbox_state;
