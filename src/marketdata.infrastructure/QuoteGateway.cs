using marketdata.domain;
using marketdata.domain.entities;
using marketdata.infrastructure.events;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure;

public class QuoteGateway(IBus bus, IClockWrapper clockWrapper) : IQuoteGateway
{
    private readonly IBus _bus = bus;
    private readonly IClockWrapper _clockWrapper = clockWrapper;

    public async Task<bool> Process(Quote quote)
    {
        QuoteEvent quoteEvent = new()
        {
            Quote = quote,
            CreatedAt = _clockWrapper.Now,
        };

        try
        {
            await _bus.Publish(quoteEvent);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "");
        }
        return false;
    }
}
