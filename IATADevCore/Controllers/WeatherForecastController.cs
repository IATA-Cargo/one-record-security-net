using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace IATADevCore.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IConfiguration _configuration;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }


        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            try
            {
                var clientCert = GetClientCert();
                if (clientCert == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var certStatus = (new CertValidator(_configuration)).Validate(clientCert.RawData);
                if (certStatus.Status == "Good")
                {
                    return "Your client cert is: " + clientCert.Subject;
                }
                else
                {
                    throw new UnauthorizedAccessException(certStatus.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                if (ex is UnauthorizedAccessException)
                {
                    throw ex;
                }

                return "500-Internal Server Error.";
            }
        }

        System.Security.Cryptography.X509Certificates.X509Certificate2 GetClientCert()
        {
            try
            {
                var clientCert = this.Request.HttpContext.Connection.ClientCertificate;
                //var clientCert = System.Web.HttpContext.Current.Request.ClientCertificate;
                if (clientCert == null) return null;

                var bytes = clientCert.RawData;
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(bytes);
            }
            catch
            {

            }
            return null;
        }
    }
}
