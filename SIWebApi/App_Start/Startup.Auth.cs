using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Owin;
using SIWebApi.Providers;
using SIWebApi.Models;
using System.Configuration;
using Microsoft.AspNet.Identity.Owin;
using SIWebApi.Utils;
using SIWebApi.Mapping;

namespace SIWebApi
{
    public partial class Startup
    {

        public static string PublicClientId { get; private set; }

        static Startup()
        {
            PublicClientId = "siwebapi";

        }

        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(() => new DB());
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            BearerAuthOptions = new OAuthAuthorizationServerOptions
            {
                Provider = new ApplicationOAuthProvider(PublicClientId),
                TokenEndpointPath = new PathString("/Token"),            
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(Double.Parse(ConfigurationManager.AppSettings["AccessTokenExpireTimeSpanFromMinutes"].ToString())),
                AllowInsecureHttp = true,
            };
            app.UseOAuthBearerTokens(BearerAuthOptions);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            CookieAuthOptions = new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/Account/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Enables the application to validate the security stamp when the user logs in.
                    // This is a security feature which is used when you change a password or add an external login to your account.  
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, ApplicationUser>(
                    validateInterval: TimeSpan.FromMinutes(30),
                    regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            };
            app.UseCookieAuthentication(CookieAuthOptions);

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Enables the application to temporarily store user information when they are verifying the second factor in the two-factor authentication process.
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));

            // Enables the application to remember the second login verification factor such as phone or email.
            // Once you check this option, your second step of verification during the login process will be remembered on the device where you logged in from.
            // This is similar to the RememberMe option when you log in.
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

         


        }
        private static OAuthAuthorizationServerOptions _bearerAuthOptions;
        public static OAuthAuthorizationServerOptions BearerAuthOptions
        {
            get { return _bearerAuthOptions; }
            private set { _bearerAuthOptions = value; }
        }

        private static CookieAuthenticationOptions _cookieAuthOptions;
        public static CookieAuthenticationOptions CookieAuthOptions
        {
            get { return _cookieAuthOptions; }
            private set { _cookieAuthOptions = value; }
        }
    }
}
