using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Facebook;
using Microsoft.Owin.Security.Google;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security.Twitter;
using Owin;
using SIWebApi.Mapping;
using SIWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace SIWebApi.Providers
{
    public static class AuthConfig
    {
        // For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864

        public static GoogleOAuth2AuthenticationOptions googleAuthOptions { get; private set; }
        public static FacebookAuthenticationOptions facebookAuthOptions { get; private set; }
        public static TwitterAuthenticationOptions twitterAuthOptions { get; private set; }
        public static void Start(IAppBuilder app)
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
                //AuthorizeEndpointPath = new PathString("/sso/account/ExternalLogin"),
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

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //   consumerKey: "",
            //   consumerSecret: "");

            //app.UseFacebookAuthentication(
            //   appId: "307934259401024",
            //   appSecret: "2a10c3d605abee36ccd71fc4be16494b");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //  ClientId = "168456505483-p39d0h09jqe67qrh1of827u547tphqq7.apps.googleusercontent.com",
            //  ClientSecret = "YTwylx8dfBJBwrgihXSbNEvO"
            //});



           
            //    twitterAuthOptions = new TwitterAuthenticationOptions()
            //    {
            //        ConsumerKey = ConfigurationManager.AppSettings["Twitter-ClientId"],
            //        ConsumerSecret = ConfigurationManager.AppSettings["Twitter-ClientSecret"],
            ////        BackchannelCertificateValidator = new CertificateSubjectKeyIdentifierValidator(new[]
            ////      {
            ////    "A5EF0B11CEC04103A34A659048B21CE0572D7D47", // VeriSign Class 3 Secure Server CA - G2
            ////    "0D445C165344C1827E1D20AB25F40163D8BE79A5", // VeriSign Class 3 Secure Server CA - G3
            ////    "7FD365A7C2DDECBBF03009F34339FA02AF333133", // VeriSign Class 3 Public Primary Certification Authority - G5
            ////    "39A55D933676616E73A761DFA16A7E59CDE66FAD", // Symantec Class 3 Secure Server CA - G4
            ////    "01C3968ACDBD57AE7DFAFF9552311608CF23A9F9", //DigiCert SHA2 High Assurance Server C‎A 
            ////    "B13EC36903F8BF4701D498261A0802EF63642BC3" //DigiCert High Assurance EV Root CA
            ////}),
            //        Provider = new TwitterAuthProvider()
            //    };

            //    app.UseTwitterAuthentication(twitterAuthOptions);

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

        private static string _publicClientId = "self";
        public static string PublicClientId
        {
            get { return _publicClientId; }
            private set { _publicClientId = value; }
        }
    }
}