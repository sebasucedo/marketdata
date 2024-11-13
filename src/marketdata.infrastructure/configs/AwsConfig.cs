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
    public CloudWatchConfig? CloudWatch { get; set; }
    public CognitoConfig? Cognito { get; set; }
}

public class CloudWatchConfig
{
    public required string LogGroupName { get; set; }
    public required string LogStreamName { get; set; }
}

public class SqsConfig
{
    public required string ScopePrefix { get; set; }
}

public class CognitoConfig
{
    public required string UserPoolId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
