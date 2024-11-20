using marketdata.domain;
using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

public class AlpacaMessageHandler(ITradeGateway tradeGateway,
                                  IQuoteGateway quoteGateway) : IMessageHandler
{
    private readonly ITradeGateway _tradeGateway = tradeGateway;
    private readonly IQuoteGateway _quoteGateway = quoteGateway;

    //[{"T":"success","msg":"authenticated"}]

    //[{"T":"b","S":"MSFT","o":419.86,"h":419.86,"l":419.86,"c":419.86,"v":131,"t":"2024-03-28T16:58:00Z","n":3,"vw":419.846565},{"T":"t","S":"AAPL","i":4617,"x":"V","p":171.24,"s":100,"c":["@"],"z":"C","t":"2024-03-28T16:59:00.114612043Z"},{ "T":"q","S":"AAPL","bx":"V","bp":171.22,"bs":2,"ax":"V","ap":171.24,"as":1,"c":["R"],"z":"C","t":"2024-03-28T16:59:00.11462648Z"}]

    public async Task Process(string message)
    {
        //System.Text.Json.JsonReaderException: 'Expected a value, but instead reached end of data. LineNumber: 0 | BytePositionInLine: 4096.'
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
                case Constants.MessageTypes.SUCCESS:
                    string message = element.GetProperty("msg").GetString() ?? Constants.MessageTypes.SUCCESS;
                    Serilog.Log.Debug(message);
                    break;
                case Constants.MessageTypes.ERROR:
                    await NotifyError(element);
                    break;
                case Constants.MessageTypes.TRADE:
                    await ExecuteTrade(element);
                    break;
                case Constants.MessageTypes.QUOTE:
                    await ExecuteQuote(element);
                    break;
            }
        }
    }

    private async Task ExecuteTrade(JsonElement element)
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

    private async Task ExecuteQuote(JsonElement element)
    {
        //{ "T":"b","S":"SPY","o":388.985,"h":389.13,"l":388.975,"c":389.12,"v":49378,"t":"2021-02-22T19:15:00Z" }
        var text = element.GetRawText();
        var q = JsonSerializer.Deserialize<QuoteDTO>(text);
        if (q is null)
            return;

        var quote = new Quote
        {
            Symbol = q.Symbol,
            Tape = q.Tape,
            AskPrice = q.AskPrice,
            AskSize = q.AskSize,
            BidPrice = q.BidPrice,
            BidSize = q.BidSize,
            Timestamp = DateTime.Parse(q.Timestamp),
        };

        await _quoteGateway.Process(quote);
    }

    readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private async Task NotifyError(JsonElement element)
    {
        var text = element.GetRawText();
        var error = JsonSerializer.Deserialize<ErrorDTO>(text, options);

        Serilog.Log.Error(text);
    }
}
