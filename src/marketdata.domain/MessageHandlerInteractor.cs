﻿using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace marketdata.domain;

public class MessageHandlerInteractor(ITradeGateway tradeGateway)
{
    private readonly ITradeGateway _tradeGateway = tradeGateway;

    //[{"T":"success","msg":"authenticated"}]

    //[{"T":"b","S":"MSFT","o":419.86,"h":419.86,"l":419.86,"c":419.86,"v":131,"t":"2024-03-28T16:58:00Z","n":3,"vw":419.846565},{"T":"t","S":"AAPL","i":4617,"x":"V","p":171.24,"s":100,"c":["@"],"z":"C","t":"2024-03-28T16:59:00.114612043Z"},{ "T":"q","S":"AAPL","bx":"V","bp":171.22,"bs":2,"ax":"V","ap":171.24,"as":1,"c":["R"],"z":"C","t":"2024-03-28T16:59:00.11462648Z"}]

    public async Task Process(string message)
    {
        using JsonDocument doc = JsonDocument.Parse(message);

        if (doc.RootElement.ValueKind == JsonValueKind.Array)
            foreach (var element in doc.RootElement.EnumerateArray())
                await ParseElement(element);
        else
            await ParseElement(doc.RootElement);
    }

    private async Task ParseElement(JsonElement element)
    {
        if (element.TryGetProperty("T", out JsonElement typeElement))
        {
            string? type = typeElement.GetString();

            switch (type)
            {
                case "success":
                    var message = element.GetProperty("msg").GetString();
                    break;
                case "t":
                    await ExcecuteTrade(element);
                    break;
            }
        }
    }

    private async Task ExcecuteTrade(JsonElement element)
    {
        //{ "T":"t","S":"AAPL","i":5674,"x":"V","p":171.64,"s":6,"c":["@","I"],"z":"C","t":"2024-03-28T18:13:56.149020008Z"}
        //{ "T":"t","S":"AAPL","i":6093,"x":"V","p":171.695,"s":100,"c":["@"],"z":"C","t":"2024-03-28T18:42:11.73120176Z"}
        var text = element.GetRawText();
        var t = JsonSerializer.Deserialize<TradeDTO>(text);
        if (t is null)
            return;

        var trade = new Trade
        {
            Symbol = t.Symbol,
            Tape = t.Tape,
            Price = t.Price,
            Quantity = t.Quantity,
            Timestamp = DateTime.Parse(t.Timestamp),
        };
        await _tradeGateway.Save(trade);
    }
}


public class TradeDTO
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