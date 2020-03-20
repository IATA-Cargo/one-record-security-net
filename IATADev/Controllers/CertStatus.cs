using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IATADev.Controllers
{
    class CertStatus
    {
        public const string BadCrl              = "The live CRL expires.";
        public const string NoCrl               = "No CDP URL found or there are more than one CDP CRL found.";
        public const string BadExtension        = "The CRL does not include revocation info.";
        public const string BadRevocationReason = "The certificate is revoked without revocation reason info.";
        public const string BadIssuer           = "Unable to load the issuer certificate.";
        public const string IssuerNotMatch      = "Issuer not match - There might be a problem with the OCSP Service";
        public const string BadSerial           = "Serial value not match - There might be a problem with the OCSP Service.";

        public string ErrorMessage
        {
            get;
            private set;
        }
            
        public string Status
        {
            get;
            private set;
        }

        public string RevocationDate
        {
            get;
            private set;
        }

        public int? RevocationReason
        {
            get;
            private set;
        }

        public CertStatus(  string status, 
                            string revocationDate = null, 
                            int? revocationReason = null, 
                            string errorMessage = null)
        {
            Status = status;
            RevocationDate = revocationDate;
            RevocationReason = revocationReason;
            ErrorMessage = errorMessage;
        }

        public static CertStatus Good
        {
            get
            {
                return new CertStatus("Good");
            }
        }

        public static CertStatus Unknown(string errorMessage = null)
        {
            return new CertStatus("Unknown", null, null, errorMessage);
        }

        public static CertStatus Revoked(string revocationDate, int? revocationReason)
        {
            return new CertStatus("Revoked", revocationDate, revocationReason);
        }

    }
}