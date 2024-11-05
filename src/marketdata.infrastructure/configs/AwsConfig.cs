using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.configs;

public class AwsConfig
{
    public required string Region { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public SqsConfig Sqs { get; set; } = null!;
}

public class SqsConfig
{
    public required string ScopePrefix { get; set; }
}
