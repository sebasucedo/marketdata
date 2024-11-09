using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
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
        public const string QUOTE = "q";
        public const string BAR_MINUTE = "b";
        public const string BAR_DAILY = "d";
        public const string BAR_UPDATED = "u";
        public const string SUCCESS = "success";
        public const string ERROR = "error";
    }
}
