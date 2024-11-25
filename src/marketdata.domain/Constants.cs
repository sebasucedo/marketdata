using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AWS_REGION = "REGION";
        public const string AWS_ACCESS_KEY = "ACCESS_KEY";
        public const string AWS_SECRET_KEY = "SECRET_KEY";
        public const string AWS_SECRET_NAME = "SECRET_NAME";
    }

    public static class ClaimTypes
    {
        public const string ACCESS_TOKEN = "AccessToken";
        public const string ID_TOKEN = "IdToken";
        public const string REFRESH_TOKEN = "RefreshToken";
    }
}
