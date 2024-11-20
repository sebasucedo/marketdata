using marketdata.domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace marketdata.infrastructure.finnhub;

public class FinnhubMessageHandler : IMessageHandler
{
    public async Task Process(string message)
    {
        //{"type":"ping"}

        //{"data":[{"c":["1","12"],"p":226.89,"s":"AAPL","t":1731599126006,"v":30},{"c":["1","12"],"p":226.8988,"s":"AAPL","t":1731599125982,"v":1},{"c":["1","12"],"p":226.8812,"s":"AAPL","t":1731599125982,"v":1},{"c":["1","12"],"p":226.88,"s":"AAPL","t":1731599125982,"v":1}],"type":"trade"}
        using JsonDocument doc = JsonDocument.Parse(message);
    
    
    }
}
