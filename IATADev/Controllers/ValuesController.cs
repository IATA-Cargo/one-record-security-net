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
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            var clientCert = GetClientCert();
            
            if (clientCert == null)
            {
                return "Bad Request";
            }

            return "Your client cert is: " + clientCert.Subject;
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        System.Security.Cryptography.X509Certificates.X509Certificate2 GetClientCert()
        {
            try
            {
                var clientCert = System.Web.HttpContext.Current.Request.ClientCertificate;
                var bytes = clientCert.Certificate;
                return new System.Security.Cryptography.X509Certificates.X509Certificate2(bytes);
            }
            catch
            {

            }
            return null;
        }
    }
}
