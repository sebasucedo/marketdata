using marketdata.domain.entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.events;

internal class QuoteEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required Quote Quote { get; set; }
    public DateTime CreatedAt { get; set; }

}
