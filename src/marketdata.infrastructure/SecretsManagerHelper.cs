using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure;

public class SecretsManagerHelper
{
    public static async Task<IConfigurationRoot> GetConfigurationFromPlainText()
    {
        string region = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_REGION)
                            ?? throw new InvalidOperationException(Constants.EnvironmentVariables.AWS_REGION);
        string accessKey = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_ACCESS_KEY)
                            ?? throw new InvalidOperationException(Constants.EnvironmentVariables.AWS_ACCESS_KEY);
        string secretKey = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_SECRET_KEY)
                            ?? throw new InvalidOperationException(Constants.EnvironmentVariables.AWS_SECRET_KEY);
        string secretName = Environment.GetEnvironmentVariable(Constants.EnvironmentVariables.AWS_SECRET_NAME)
                            ?? throw new InvalidOperationException(Constants.EnvironmentVariables.AWS_SECRET_NAME);

        var credential = new BasicAWSCredentials(accessKey, secretKey);
        using var client = new AmazonSecretsManagerClient(credential, RegionEndpoint.GetBySystemName(region));
        var secretValue = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretName,
            VersionStage = Constants.SecretsManager.AWSCURRENT
        });

        var secretString = secretValue.SecretString ?? throw new InvalidOperationException(secretName);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(secretString));
        using var reader = new StreamReader(stream);

        var builder = new ConfigurationBuilder()
                          .SetBasePath(AppContext.BaseDirectory)
                          .AddJsonStream(reader.BaseStream)
                          .AddEnvironmentVariables();

        IConfigurationRoot configuration = builder.Build();

        return configuration;
    }
}
