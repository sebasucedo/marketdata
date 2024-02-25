using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain;

public interface ITradeGateway
{
    Task<IEnumerable<Trade>> GetAll();
    Task<bool> Save(Trade trade);
}
