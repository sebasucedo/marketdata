CREATE DATABASE IF NOT EXISTS marketdata;

USE marketdata;

CREATE TABLE IF NOT EXISTS Trades (
    Symbol VARCHAR(255) NOT NULL,
    Timestamp DATETIME NOT NULL,
    TradeId INT NOT NULL AUTO_INCREMENT,
    Price DECIMAL(19,4) NOT NULL,
    Quantity DECIMAL(19,4) NOT NULL,
    Tape CHAR(1) NOT NULL CHECK (Tape IN ('A', 'B', 'C')),
    VolumeWeightedAveragePrice DECIMAL(19,4) NOT NULL,
    PRIMARY KEY (TradeId)
);
