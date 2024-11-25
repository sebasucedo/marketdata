using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace marketdata.domain.security;

public record AuthenticationResponse(string? AccessToken,
                                     string? IdToken,
                                     string? RefreshToken,
                                     int? ExpiresIn,
                                     string? ChallengeName,
                                     string? Session);