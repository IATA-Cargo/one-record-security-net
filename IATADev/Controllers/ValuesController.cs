using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IATADev.Controllers
{
    public class ValuesController : ApiController
    {

        // GET api/values/5
        public string Get(int id)
        {
            try
            {
                var clientCert = GetClientCert();
                if (clientCert == null)
                {
                    throw new UnauthorizedAccessException();
                }

                var certStatus = (new CertValidator()).Validate(clientCert.RawData);
                if (certStatus.Status == "Good")
                {
                    return "Your client cert is: " + clientCert.Subject;
                }
                else
                {
                    throw new UnauthorizedAccessException(certStatus.ErrorMessage);
                }
            }
            catch(Exception ex)
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
                var clientCert  = System.Web.HttpContext.Current.Request.ClientCertificate;
                var bytes       = clientCert.Certificate;
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(bytes);
            }
            catch
            {

            }
            return null;
        }
    }
}
