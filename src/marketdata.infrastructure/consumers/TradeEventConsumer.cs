using marketdata.domain;
using marketdata.infrastructure.events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.consumers;

internal class TradeEventConsumer(ITradeGateway tradeGateway) : IConsumer<TradeEvent>
{
    private readonly ITradeGateway _tradeGateway = tradeGateway;

    public async Task Consume(ConsumeContext<TradeEvent> context)
    {
        var tradeEvent = context.Message;
        var trade = tradeEvent.Trade;

        try
        {
            await _tradeGateway.Process(trade);
        }
		catch (Exception ex)
		{
			Serilog.Log.Error(ex, "");
		}
    }
}

internal class TradeEventErrorConsumer : IConsumer<Fault<TradeEvent>>
{
    public Task Consume(ConsumeContext<Fault<TradeEvent>> context)
    {
        throw new NotImplementedException();
    }
}