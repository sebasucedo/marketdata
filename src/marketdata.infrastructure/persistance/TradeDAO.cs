using Dapper;
using marketdata.domain;
using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.persistance;

public class TradeDAO(IDbConnection connection) : ITradeGateway
{
    private readonly IDbConnection _connection = connection;

    public async Task<IEnumerable<Trade>> GetAll()
    {
        var query = @"SELECT * FROM Trades";

        var q = await _connection.QueryAsync<Trade>(query);

        return q.ToList();
    }

    public async Task<bool> Process(Trade trade)
    {
        var sql = @"INSERT INTO Trades (Symbol, Timestamp, Price, Quantity, Tape, VolumeWeightedAveragePrice)
                    VALUES (@Symbol, @Timestamp, @Price, @Quantity, @Tape, @VolumeWeightedAveragePrice)";

        try
        {
            var affectedRows = await _connection.ExecuteAsync(sql, trade);

            return affectedRows == 1;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "Failed to insert trade into the database. Symbol: {Symbol}, Timestamp: {Timestamp}, Price: {Price}, Quantity: {Quantity}, Tape: {Tape}, VWAP: {VWAP}",
                              trade.Symbol, trade.Timestamp, trade.Price, trade.Quantity, trade.Tape, trade.VolumeWeightedAveragePrice);
            throw new InvalidOperationException("An error occurred while inserting the trade into the database.", ex);

        }
    }
}
