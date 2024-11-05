using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.alpaca;

internal class Constants
{
    internal static class Actions
    {
        public const string AUTH = "auth";
        public const string SUBSCRIBE = "subscribe";
    }

    internal static class MessageTypes
    {
        public const string TRADE = "t";
        public const string SUCCESS = "success";
        public const string ERROR = "error";
    }
}
