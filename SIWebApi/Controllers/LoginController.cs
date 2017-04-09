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
using Microsoft.AspNet.Identity.EntityFramework;

namespace SIWebApi.Controllers
{
    /// <summary>
    /// Manage autorizations
    /// </summary>
    [AllowAnonymous]
    [RoutePrefix("api/auth")]
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

        /// <summary>
        /// Register new user
        /// </summary>
        /// <returns>
        /// Return the id of created user 
        /// </returns>
        [HttpPost]
        [AllowAnonymous]
        [Route("Register")]
        [ResponseType(typeof(string))]
        public IHttpActionResult Register([FromBody]RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return GetErrorResult(ModelState);

            try
            {
                var user = new ApplicationUser()
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreationDate = DateTime.Now,
                    LastUpdate = DateTime.Now
                };

                IdentityResult addUserResult = this.AppUserManager.Create(user, model.Password);

                if (!addUserResult.Succeeded)
                    return GetErrorResult(addUserResult);

                var currentUser = AppUserManager.FindByName(user.UserName);

                string roleName = ConfigurationManager.AppSettings["DefaultRoleOnRegistration"];
                if (!AppRoleManager.RoleExists(roleName))
                    AppRoleManager.Create(new IdentityRole(roleName));

                var addUserToRole = AppUserManager.AddToRole(currentUser.Id, roleName);
                if (!addUserToRole.Succeeded)
                    return GetErrorResult(addUserToRole);
                SendConfirmEmail(currentUser.Id, model.ConfirmRedirectUrl, model.ConfirmTemplateHtml);
                return Ok(currentUser.Id);
            }
            catch (Exception ex)
            {
                return Content(HttpStatusCode.ExpectationFailed, ex.Message);
            }
        }

        /// <summary>
        /// Required confirm e-mail for user
        /// </summary>
        /// <param name="obj">Parameters to required send confirm e-mail</param>
        [AllowAnonymous]
        [HttpPost]
        [Route("SendConfirmEmail")]
        public IHttpActionResult SendConfirmEmail([FromBody]SendCodeViewModel obj)
        {
            var appUser = this.AppUserManager.FindByName(obj.UserName);
            SendConfirmEmail(appUser.Id, obj.ConfirmRedirectUrl, obj.ConfirmTemplateHtml);
            return Ok();
        }

        private void SendConfirmEmail(string idUser, string redirectUrl, string templateHtml)
        {
            string code = this.AppUserManager.GenerateEmailConfirmationToken(idUser);

            if (string.IsNullOrEmpty(redirectUrl))
                redirectUrl = string.Format("{0}/#/account/confirmEmail", ConfigurationManager.AppSettings["WebEndPoint"]);
            var callBackUrl = string.Format("{0}?idUser={1}&code={2}", redirectUrl, idUser, HttpUtility.UrlEncode(code));

            var message = string.Format("<p class=\"heading\" style=\"font-family: Helvetica, Arial, sans-serif; font-weight: 700; font-size: 16px; color: #59595b; margin-bottom: 20px; text-align: center;\" align=\"center\">E' un piacere averti con Noi!</p><p class=\"heading\" style=\"font-family: Helvetica, Arial, sans-serif; font-size: 16px; color: #59595b; margin-bottom: 20px; text-align: center;\" align=\"center\">Per confermare e completare la configurazione dell'account,<br/>si prega di cliccare qui sotto:</p><div class=\"cta margin-top-30px margin-bottom-50px\" style=\"text-align: center; margin-top: 30px !important; margin-bottom: 50px !important;\" align=\"center\"><!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\"href=\"{0}\"style=\"height:40px;v-text-anchor:middle;width:300px;\" arcsize=\"63%\" stroke=\"f\"fillcolor=\"#45ba6b\"><w:anchorlock/><center><![endif]--><a href = \"{0}\" style=\"color:#59595b;background-color:#45ba6b;border-radius:25px;color:#ffffff;display:inline-block;font-family: 'Open Sans', Helvetica, Arial, sans-serif;font-size:14px;letter-spacing:1px;font-weight:bold;line-height:40px;text-align:center;text-decoration:none;width:300px;-webkit-text-size-adjust:none;\">CONFERMA IL TUO ACCOUNT</a><!--[if mso]></center></v:roundrect><![endif]--></div>", callBackUrl);
            this.AppUserManager.SendEmail(idUser, "Benvenuto in Regusto", message);
        }

        /// <summary>
        /// Confirm e-mail for new user
        /// </summary>
        /// <param name="userId">The ID of user registered</param>
        /// <param name="code">Code generated for registration </param>
        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmail")]
        public IHttpActionResult ConfirmEmail(string userId = "", string code = "")
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
                return Content(HttpStatusCode.BadRequest, "User Id and Code are required");

            IdentityResult result = this.AppUserManager.ConfirmEmail(userId, code);
            if (!result.Succeeded)
                return GetErrorResult(result);
            return Ok();
        }

        /// <summary>
        /// Users administrator set new password for user
        /// </summary>
        [Route("SetPassword")]
        public IHttpActionResult SetPassword(SetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return GetErrorResult(ModelState);

            var removePwd = this.AppUserManager.RemovePassword(model.UserId);
            if (!removePwd.Succeeded)
                return GetErrorResult(removePwd);

            IdentityResult addPwd = this.AppUserManager.AddPassword(model.UserId, model.NewPassword);
            if (!addPwd.Succeeded)
                return GetErrorResult(addPwd);
            return Ok();
        }

        /// <summary>
        /// User Required Change password
        /// </summary>
        [Route("ChangePassword")]
        public IHttpActionResult ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return GetErrorResult(ModelState);

            IdentityResult result = this.AppUserManager.ChangePassword(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);

            if (!result.Succeeded)
                return GetErrorResult(result);
            return Ok();
        }

        /// <summary>
        /// Send reset password to user
        /// </summary>
        /// <param name="obj">Parameters to required send reset password</param>
        [AllowAnonymous]
        [HttpPost]
        [Route("SendResetPassword")]
        public IHttpActionResult SendResetPassword([FromBody]SendCodeViewModel obj)
        {
            var appUser = this.AppUserManager.FindByName(obj.UserName);
            SendResetPassword(appUser.Id, obj.ConfirmRedirectUrl, obj.ConfirmTemplateHtml);
            return Ok();
        }

        private void SendResetPassword(string idUser, string redirectUrl, string templateHtml)
        {

            string code = this.AppUserManager.GeneratePasswordResetToken(idUser);

            if (string.IsNullOrEmpty(redirectUrl))
                redirectUrl = string.Format("{0}/#/account/resetPassword", ConfigurationManager.AppSettings["WebEndPoint"]);
            var callBackUrl = string.Format("{0}?idUser={1}&code={2}", redirectUrl, idUser, HttpUtility.UrlEncode(code));

            var message = string.Format("<p class=\"heading\" style=\"font-family: Helvetica, Arial, sans-serif; font-weight: 700; font-size: 16px; color: #59595b; margin-bottom: 20px; text-align: center;\" align=\"center\">Hai smarrito la password?<br/>Non ti preoccupare ci pensiamo noi!</p><p class=\"heading\" style=\"font-family: Helvetica, Arial, sans-serif; font-size: 16px; color: #59595b; margin-bottom: 20px; text-align: center;\" align=\"center\">Per completare il recupero della password,<br/>si prega di cliccare qui sotto:</p><div class=\"cta margin-top-30px margin-bottom-50px\" style=\"text-align: center; margin-top: 30px !important; margin-bottom: 50px !important;\" align=\"center\"><!--[if mso]><v:roundrect xmlns:v=\"urn:schemas-microsoft-com:vml\" xmlns:w=\"urn:schemas-microsoft-com:office:word\"href=\"{0}\"style=\"height:40px;v-text-anchor:middle;width:300px;\" arcsize=\"63%\" stroke=\"f\"fillcolor=\"#45ba6b\"><w:anchorlock/><center><![endif]--><a href = \"{0}\" style=\"color:#59595b;background-color:#45ba6b;border-radius:25px;color:#ffffff;display:inline-block;font-family: 'Open Sans', Helvetica, Arial, sans-serif;font-size:14px;letter-spacing:1px;font-weight:bold;line-height:40px;text-align:center;text-decoration:none;width:300px;-webkit-text-size-adjust:none;\">RESET PASSWORD</a><!--[if mso]></center></v:roundrect><![endif]--></div>", callBackUrl);

            this.AppUserManager.SendEmail(idUser, "Recupera la tua password di Regusto", message);
        }

        /// <summary>
        /// Confirm e-mail for new user
        /// </summary>
        /// <param name="userId">The ID of user registered</param>
        /// <param name="code">Code generated for registration </param>
        /// <param name="newPassword">New e-mail</param>
        [AllowAnonymous]
        [HttpGet]
        [Route("ResetPassword")]
        public IHttpActionResult ResetPassword(string userId, string code, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(newPassword))
                return Content(HttpStatusCode.BadRequest, "User Id, Code and New Password are required");

            IdentityResult result = this.AppUserManager.ResetPassword(userId, code, newPassword);
            if (!result.Succeeded)
                return GetErrorResult(result);
            return Ok();
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

