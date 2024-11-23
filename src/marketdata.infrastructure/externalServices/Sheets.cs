using marketdata.domain;
using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.externalServices;

public class Sheets : ITradeGateway
{
    public Task<IEnumerable<Trade>> GetAll()
    {
        throw new NotImplementedException();
    }

    public Task<bool> Process(Trade trade)
    {
        throw new NotImplementedException();
    }
}
