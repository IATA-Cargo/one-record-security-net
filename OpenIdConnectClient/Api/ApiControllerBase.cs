namespace WebOpenIdConnectClient.Api
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The base class for all api
    /// </summary>
    public class ApiControllerBase : ControllerBase
    {

        private IConfiguration _configuration;

        /// <summary>
        /// Intialize base class and get injection services
        /// </summary>
        /// <param name="configuration"></param>
        public ApiControllerBase( IConfiguration configuration )
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Get header value
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetHeaderWithKey(List<KeyValuePair<string, StringValues>> headers, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (headers == null) return string.Empty;


            return headers.FirstOrDefault(i => i.Key == key).Value + "";
        }

        /// <summary>
        /// Get a config string
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetConfigString(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_configuration == null) return string.Empty;

            try
            {
                return _configuration[key];
            }
            catch (Exception)
            {
            }
            return string.Empty;
        }

        /// <summary>
        /// Create a exception response, may be this place to write log
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected JsonResult CreateExceptionResponse(Exception ex)
        {
            return new JsonResult(new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Message = "Error",
                Details = "" + ex?.Message
            });
        }

        /// <summary>
        /// Create a error response
        /// </summary>
        /// <param name="headers"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        protected JsonResult CreateErrorResponse(string message, string details)
        {
            return new JsonResult(new
            {
                Timestamp = DateTime.UtcNow.Ticks,
                Message = (string.IsNullOrEmpty(message) ? "Error" : message),
                Details = details
            });
        }
    }
}
