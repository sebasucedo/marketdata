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

public class TradeDAO(string connectionString) : ITradeGateway
{
    private readonly string _connectionString = connectionString;

    public async Task<IEnumerable<Trade>> GetAll()
    {
        using IDbConnection connection = new MySqlConnection(_connectionString);

        var query = @"SELECT * FROM Trades";

        var q = await connection.QueryAsync<Trade>(query);

        return q.ToList();
    }

    public async Task<bool> Save(Trade trade)
    {
        using IDbConnection connection = new MySqlConnection(_connectionString);

        var sql = @"INSERT INTO Trades (Symbol, Timestamp, Price, Quantity, Tape, VolumeWeightedAveragePrice)
                    VALUES (@Symbol, @Timestamp, @Price, @Quantity, @Tape, @VolumeWeightedAveragePrice)";

        var affectedRows = await connection.ExecuteAsync(sql, trade);

        return affectedRows == 1;
    }
}
