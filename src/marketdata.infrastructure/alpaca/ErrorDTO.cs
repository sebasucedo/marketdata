using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

internal class ErrorDTO
{
    public int Code { get; set; }
    [JsonPropertyName("msg")]
    public required string Message { get; set; }
}
