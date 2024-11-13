USE marketdata;

CREATE TABLE IF NOT EXISTS Quotes (
    QuoteId INT NOT NULL AUTO_INCREMENT,
    Symbol VARCHAR(255) NOT NULL,
    Timestamp DATETIME NOT NULL,
    AskPrice DECIMAL(19,4) NOT NULL,
    AskSize DECIMAL(19,4) NOT NULL,
    BidPrice DECIMAL(19,4) NOT NULL,
    BidSize DECIMAL(19,4) NOT NULL,
    Tape CHAR(1) NOT NULL CHECK (Tape IN ('A', 'B', 'C')),
    PRIMARY KEY (QuoteId),
    INDEX idx_symbol_timestamp (Symbol, Timestamp)
);
