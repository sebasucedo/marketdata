using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain.entities;

public class Quote
{
    public required string Symbol { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal AskPrice { get; set; }
    public decimal AskSize { get; set; }
    public decimal BidPrice { get; set; }
    public decimal BidSize { get; set; }
    public required string Tape { get; set; }
}
