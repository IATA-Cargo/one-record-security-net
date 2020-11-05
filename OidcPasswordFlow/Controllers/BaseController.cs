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

        /// <summary>
        /// Base function to get the configuration of application
        /// </summary>
        /// <param name="key">The key in configuration</param>
        /// <param name="defaultValue">The returned default value</param>
        /// <returns></returns>
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

        /// <summary>
        /// Base function to log error with exception
        /// </summary>
        /// <param name="ex">The Exception object</param>
        public void logException(Exception ex)
        {
            if (this._logger == null) return;
            if (ex == null) return;

            this._logger.LogError(ex.Message, ex);
        }


    }

}
