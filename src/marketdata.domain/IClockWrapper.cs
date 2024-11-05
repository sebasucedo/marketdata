using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain;

public interface IClockWrapper
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
}
