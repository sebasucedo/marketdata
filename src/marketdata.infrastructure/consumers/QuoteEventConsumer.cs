using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.consumers;

internal class QuoteEventConsumer(IQuoteGateway quoteGateway) : IConsumer<QuoteEvent>
{
    private readonly IQuoteGateway _quoteGateway = quoteGateway;

    public async Task Consume(ConsumeContext<QuoteEvent> context)
    {
        var quoteEvent = context.Message;
        var quote = quoteEvent.Quote;

        try
        {
            await _quoteGateway.Process(quote);
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "");
        }
    }
}
