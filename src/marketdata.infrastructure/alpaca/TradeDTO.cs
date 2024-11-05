using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

internal class TradeDTO
{
    [JsonPropertyName("S")]
    public required string Symbol { get; set; }
    [JsonPropertyName("z")]
    public required string Tape { get; set; }
    [JsonPropertyName("t")]
    public required string Timestamp { get; set; }
    [JsonPropertyName("p")]
    public decimal Price { get; set; }
    [JsonPropertyName("s")]
    public decimal Quantity { get; set; }
}
