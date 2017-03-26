using SIWebApi.Mapping;
using SIWebApi.Utils;
using SIWebApi.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Configuration;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Description;
using System.Web.Http.Filters;
namespace SIWebApi.Controllers
{
    /// <summary>
    /// Manage autorizations
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/token")]
    [EnableCors("*", "*", "*")]
    public class LoginController : IdentityBaseApiController
    {
        private DB db = new DB();

        /// <summary>
        /// Required valid token for user
        /// </summary>
        /// <param name="credentials">User credentials</param>
        [HttpPost]
        [Route("")]
        [ResponseType(typeof(TokenViewModel))]
        public async Task<IHttpActionResult> GetToken([FromBody]LoginViewModel credentials)
        {
            if (!ModelState.IsValid)
                return GetErrorResult(ModelState);

            var user = await this.AppUserManager.FindByEmailAsync(credentials.Email);
            if (user == null)
                return Content(HttpStatusCode.NotFound, "Incorrect E-Mail or Password.");

            if (!await AppUserManager.IsEmailConfirmedAsync(user.Id))
            {
                string code = await SendEmailConfirmationTokenAsync(user.Id, "Confirm your account resend");
                return Content(HttpStatusCode.Forbidden, "You must have a confirmed e-mail to log on.");
            }

            var result = await this.AppUserManager.FindAsync(user.Email, credentials.Password);
            if (result == null)
                return Content(HttpStatusCode.NotFound, "Incorrect E-Mail or Password.");

            if (result.LockoutEnabled)
                return Content(HttpStatusCode.Forbidden, "User is locked.");

            var timeValidRange = TimeSpan.FromMinutes(Double.Parse(ConfigurationManager.AppSettings["AccessTokenExpireTimeSpanFromMinutes"].ToString()));
            var identity = await this.AppSignInManager.CreateUserIdentityAsync(result);
            var _res = TokenUtils.GenerateToken(identity);
            return Ok(new TokenViewModel()
            {
                AccessToken = _res,
                ExpiresIn = (int)timeValidRange.TotalSeconds,
                Expires = DateTime.Now.AddDays(timeValidRange.Days),
                Issued = DateTime.Now,
                UserName = user.UserName,
                TokenType = "bearer"
            });
        }

       

        /// <summary>
        /// Get reissue token 
        /// </summary>
        /// <param></param>
        /// <returns></returns>
        [Authorize]
        [HttpGet]
        public IHttpActionResult ReissueToken()
        {
            var identity = ((ClaimsPrincipal)User).Identity as ClaimsIdentity;
            var timeSpanValidRange = TimeSpan.FromMinutes(Double.Parse(ConfigurationManager.AppSettings["AccessTokenExpireTimeSpanFromMinutes"].ToString()));
            var _res = TokenUtils.GenerateToken(identity);
            return Ok(new TokenViewModel()
            {
                AccessToken = _res,
                ExpiresIn = (int)timeSpanValidRange.TotalSeconds,
                Expires = DateTime.Now.AddDays(timeSpanValidRange.Days),
                Issued = DateTime.Now,
                UserName = identity.GetUserName(),
                TokenType = "bearer"
            });
        }

        private async Task<string> SendEmailConfirmationTokenAsync(string userId, string subject)
        {
            string code = await AppUserManager.GenerateEmailConfirmationTokenAsync(userId);
            await this.AppUserManager.SendEmailAsync(userId, "Confirm your account", "Please confirm your account by clicking <a href=\"" + code + "\">here</a>");
            return code;
        }

    }

    public class NoResponseHeader : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            HttpContext.Current.Response.Headers.Remove("X-Powered-By");
            HttpContext.Current.Response.Headers.Remove("X-AspNet-Version");
            HttpContext.Current.Response.Headers.Remove("X-AspNetMvc-Version");
            HttpContext.Current.Response.Headers.Remove("Server");
            HttpContext.Current.Response.Headers.Remove("Access-Control-Allow-Origin");
            HttpContext.Current.Response.Headers.Remove("Access-Control-Allow-Methods");
            HttpContext.Current.Response.Headers.Remove("Access-Control-Allow-Credentials");
            HttpContext.Current.Response.Headers.Remove("Access-Control-Allow-Headers");

            HttpContext.Current.Response.Headers.Remove("Cache-Control");
            HttpContext.Current.Response.Headers.Remove("Pragma");
            HttpContext.Current.Response.Headers.Remove("Content-Type");
            HttpContext.Current.Response.Headers.Remove("Expires");
            HttpContext.Current.Response.Headers.Remove("X-SourceFiles");
            HttpContext.Current.Response.Headers.Remove("Date");
            HttpContext.Current.Response.Headers.Remove("Content-Length");
            HttpContext.Current.Response.Headers.Remove("Set-Cookie");
        }
    }
}

