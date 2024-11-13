using Dapper;
using marketdata.domain;
using marketdata.domain.entities;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.persistance;

public class QuoteDAO(IDbConnection connection) : IQuoteGateway
{
    private readonly IDbConnection _connection = connection;

    public async Task<bool> Process(Quote quote)
    {
        var sql = @"
                    INSERT INTO Quotes (Symbol, Timestamp, AskPrice, AskSize, BidPrice, BidSize, Tape)
                    VALUES (@Symbol, @Timestamp, @AskPrice, @AskSize, @BidPrice, @BidSize, @Tape);
                   ";

        try
        {
            var affectedRows = await _connection.ExecuteAsync(sql, quote);

            return affectedRows == 1;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to insert quote into the database. Symbol: {Symbol}, Timestamp: {Timestamp}, AskPrice: {AskPrice}, BidPrice: {BidPrice}, Tape: {Tape}",
                              quote.Symbol, quote.Timestamp, quote.AskPrice, quote.BidPrice, quote.Tape);
            throw new InvalidOperationException("An error occurred while inserting the quote into the database.", ex);
        }
    }
}
