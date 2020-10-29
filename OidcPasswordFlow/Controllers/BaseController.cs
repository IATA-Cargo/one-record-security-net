namespace OidcPasswordFlow.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;


    public class BaseController : Controller
    {
        private IConfiguration _configuration;
        private readonly ILogger<BaseController> _logger;

        public BaseController(IConfiguration configuration, ILogger<BaseController> logger)
        {
            this._configuration = configuration;
            this._logger = logger;
        }

        protected string GetConfig(string key, string defaultValue = "")
        {
            try
            {
                return _configuration[key];
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public void logException(Exception ex)
        {
            if (this._logger == null) return;
            if (ex == null) return;

            this._logger.LogError(ex.Message, ex);
        }


    }

}
