-- init-scripts/init.sql
CREATE TABLE IF NOT EXISTS vote_results (
    option VARCHAR(50) PRIMARY KEY,
    votes INT DEFAULT 0
);
