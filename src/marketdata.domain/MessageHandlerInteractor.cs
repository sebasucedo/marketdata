using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.domain;

public class MessageHandlerInteractor(ITradeGateway tradeGateway)
{
    private readonly ITradeGateway _tradeGateway = tradeGateway;

    public async Task Process(string message)
    {
        var news = JsonSerializer.Deserialize<List<Message>>(message);
        if (news == null)
            return;

        foreach (var n in news)
        {
            if (n.T.Equals("t", StringComparison.CurrentCultureIgnoreCase))
            {
                var trade = new Trade
                {
                    Symbol = "xxx",
                    Tape = "C"
                };
                await _tradeGateway.Save(trade);
            }
        }
    }

    public class Message
    {
        public string T { get; set; }
        public string Msg { get; set; }
    }
}
