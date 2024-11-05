using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain;

public interface IMessageHandler
{
    Task Process(string message);
}
