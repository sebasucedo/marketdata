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

public class TradeGateway(IBus bus, IClockWrapper clockWrapper) : ITradeGateway
{
    private readonly IBus _bus = bus;
    private readonly IClockWrapper _clockWrapper = clockWrapper;

    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Save(Trade trade)
    {
        TradeEvent tradeEvent = new()
        {
            Trade = trade,
            CreatedAt = _clockWrapper.Now,
        };

        try
        {
            await _bus.Publish(tradeEvent);
            return true;
        }
        catch (Exception ex)
        {
            Serilog.Log.Error(ex, "");
        }
        return false;
    }
}
