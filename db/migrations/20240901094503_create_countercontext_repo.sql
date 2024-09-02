-- migrate:up

CREATE TABLE counterreference_repository (
    id UUID PRIMARY KEY,
    item jsonb NOT NULL 
)

-- migrate:down

