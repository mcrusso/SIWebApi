using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SIWebApi.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using System.Web.Http.Results;
using System.Web.Http.Routing;

namespace SIWebApi.Controllers
{
    public class IdentityBaseApiController : ApiController
    {
        public IdentityBaseApiController()
        { }

        private ApplicationUserManager _appUserManager;
        public ApplicationUserManager AppUserManager
        {
            get
            {
                return _appUserManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            internal set
            {
                _appUserManager = value;
            }
        }

        private ApplicationRoleManager _appRoleManager = null;
        public ApplicationRoleManager AppRoleManager
        {
            get
            {
                if (_appRoleManager == null)
                    return Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
                return _appRoleManager;
            }
            internal set { _appRoleManager = value; }
        }

        private ApplicationSignInManager _appSignInManager = null;
        public ApplicationSignInManager AppSignInManager
        {
            get { return _appSignInManager ?? Request.GetOwinContext().GetUserManager<ApplicationSignInManager>(); }
            internal set { _appSignInManager = value; }
        }

        private ISecureDataFormat<AuthenticationTicket> _appAccessTokenFormat;
        public ISecureDataFormat<AuthenticationTicket> AppAccessTokenFormat
        {
            get { return _appAccessTokenFormat; }
            internal set { _appAccessTokenFormat = value; }
        }

        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
                return InternalServerError();

            if (result.Succeeded)
                return null;

            return Content(HttpStatusCode.BadRequest, result.Errors);
        }

        protected IHttpActionResult GetErrorResult(ModelStateDictionary modelState)
        {
            if (modelState == null)
                return InternalServerError();

            if (modelState.IsValid)
                return null;

            //var errors = new List<string>();
            //foreach (ModelState ms in modelState.Values)
            //  foreach (ModelError error in ms.Errors)
            //    errors.Add(error.ErrorMessage);

            return Content(HttpStatusCode.BadRequest, modelState.Values.SelectMany(s => s.Errors.Select(ss => ss.ErrorMessage)));
        }

        protected IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }
    }
}