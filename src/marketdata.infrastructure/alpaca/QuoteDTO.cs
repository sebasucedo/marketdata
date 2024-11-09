using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

internal class QuoteDTO
{
    [JsonPropertyName("S")]
    public required string Symbol { get; set; }
    [JsonPropertyName("z")]
    public required string Tape { get; set; }
    [JsonPropertyName("ap")]
    public decimal AskPrice { get; set; }
    [JsonPropertyName("as")]
    public decimal AskSize { get; set; }
    [JsonPropertyName("bp")]
    public decimal BidPrice { get; set; }
    [JsonPropertyName("bs")]
    public decimal BidSize { get; set; }
    [JsonPropertyName("t")]
    public required string Timestamp { get; set; }
}
