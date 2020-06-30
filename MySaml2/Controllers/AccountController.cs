using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sustainsys.Saml2.AspNetCore2;

namespace MySaml2.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return Content("Index");
        }

        [AllowAnonymous]
        public IActionResult Login(string returnUrl = null)
        {
            return new ChallengeResult(
                Saml2Defaults.Scheme,
                new AuthenticationProperties
                {
                    // It looks like this parameter is ignored, so I set ReturnUrl in Startup.cs
                    RedirectUri = Url.Action(nameof(LoginCallback), new { returnUrl })
                });
        }

        //public IActionResult LoginCallback()
        //{
        //    return Content("LoginCallback");
        //}

        [AllowAnonymous]
        public async Task<IActionResult> LoginCallback(string returnUrl)
        {

            var authenticateResult = await HttpContext.AuthenticateAsync("Application");

            //_log.Information("Authenticate result: {@authenticateResult}", authenticateResult);

            // I get false here and no information on claims etc.
            if (!authenticateResult.Succeeded)
            {
                return Unauthorized();
            }

            // HttpContext.User does not contain any data either


            // code below is not executed
            var claimsIdentity = new ClaimsIdentity("Application");
            claimsIdentity.AddClaim(authenticateResult.Principal.FindFirst(ClaimTypes.NameIdentifier));

            //_log.Information("Logged in user with following claims: {@Claims}", authenticateResult.Principal.Claims);

            await HttpContext.SignInAsync("Application", new ClaimsPrincipal(claimsIdentity));

            return LocalRedirect(returnUrl ?? "/Home/Index");
        }
    }
}
