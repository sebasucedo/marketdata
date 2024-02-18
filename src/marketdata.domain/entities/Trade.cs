using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain.entities;

public class Trade
{
    public required string Symbol { get; set; }
    public DateTime Timestamp { get; set; }
    public int TradeId { get; set; }
    public decimal Price { get; set; }
    public decimal Quantity { get; set; }
    public required string Tape { get; set; } // Tape A: NYSE, Tape B: NYSE MKT (ex AMEX), Tape C: Nasdaq 
    public decimal VolumeWeightedAveragePrice { get; set; }
}
