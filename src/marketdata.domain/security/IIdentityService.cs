using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain.security;

public interface IIdentityService
{
    Task<AuthenticationResponse?> AuthenticateUser(string username, string password);
    Task<bool> Validate(string token);
}
