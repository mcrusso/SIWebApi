using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using SIWebApi.Providers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace SIWebApi.Utils
{
    public static class TokenUtils
    {
        public static string GenerateToken(IIdentity user)
        {
            if (user != null)
            {
                ClaimsIdentity oAuthIdentity = (ClaimsIdentity)user;

                AuthenticationProperties properties = ApplicationOAuthProvider.CreateProperties(user.Name);

                AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, properties);
                DateTimeOffset currentUtc = new SystemClock().UtcNow;
                ticket.Properties.IssuedUtc = currentUtc;
                ticket.Properties.ExpiresUtc = currentUtc.AddMinutes(Double.Parse(ConfigurationManager.AppSettings["AccessTokenExpireTimeSpanFromMinutes"].ToString()));

                var result = Startup.BearerAuthOptions.AccessTokenFormat.Protect(ticket);
                return result;
            }
            return null;
        }
    }
}