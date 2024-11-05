using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.infrastructure.configs;

public class Config(IConfiguration config)
{
    public AwsConfig Aws { get; set; } = config.GetSection(nameof(Aws)).Get<AwsConfig>()!;
}
