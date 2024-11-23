using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain.security;

public interface ITokenValidator
{
    Task<bool> Validate(string token);
}
