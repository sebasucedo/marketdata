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

public class TradeDAO(IDbConnection connection) : ITradeGateway
{
    private readonly IDbConnection _connection = connection;

    public async Task<IEnumerable<Trade>> GetAll()
    {
        var query = @"SELECT * FROM Trades";

        var q = await _connection.QueryAsync<Trade>(query);

        return q.ToList();
    }

    public async Task<bool> Save(Trade trade)
    {
        var sql = @"INSERT INTO Trades (Symbol, Timestamp, Price, Quantity, Tape, VolumeWeightedAveragePrice)
                    VALUES (@Symbol, @Timestamp, @Price, @Quantity, @Tape, @VolumeWeightedAveragePrice)";

        var affectedRows = await _connection.ExecuteAsync(sql, trade);

        return affectedRows == 1;
    }
}
