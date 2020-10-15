namespace WebOpenIdConnectClient.Controllers
{
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;

    public class AccountController : PageControllerBase
    {
        /// <summary>
        /// Intialize class
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public AccountController(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
            ) : base(configuration, httpContextAccessor)
        {

        }

        /// <summary>
        /// Show Access Denied page
        /// </summary>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

    }
}
