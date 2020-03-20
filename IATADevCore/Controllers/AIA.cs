using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IATADevCore.Controllers
{
    /// <summary>
    /// Storing URLs of AIA Issuer and AIA OCSP
    /// </summary>
    class AIA
    {
        public string Issuer
        {
            get;
            set;
        }

        public string Ocsp
        {
            get;
            set;
        }
    }
}