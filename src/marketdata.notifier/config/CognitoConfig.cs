namespace marketdata.notifier.config;

public class CognitoConfig
{
    public required string UserPoolId { get; set; }
    public required string ClientId { get; set; }
    public required string ClientSecret { get; set; }
}
