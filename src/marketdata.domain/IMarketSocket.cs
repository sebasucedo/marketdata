using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain;

public interface IMarketSocket
{
    event EventHandler<string>? MessageReceived;
    Task Connect(CancellationToken stoppingToken);
    Task Listen(CancellationToken stoppingToken);
    Task Subscribe(string[] symbols);
}
