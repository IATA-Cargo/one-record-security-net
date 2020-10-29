using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.IIS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;
using UraClient;

namespace Dummy1API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DummyOneRecord : ControllerBase
    {
        private IConfiguration _configuration;
        static IList<TrustOidcIP> _trustIAPList;
        public DummyOneRecord(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        /// <summary>
        /// The Dummy 1R API - it does
        /// - Validate TLS Client Certificate
        /// - Validate IDToken
        /// - Check if IAP is trusted
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var clientCert = Request.HttpContext.Connection.ClientCertificate;
                
                //Validate TLS Client Certificate
                ValidateTLSClientCertificate(clientCert);

                //Validate IDToken and Check if IAP is trsuted
                await ValidateIDTokenAndSignature(Request);

                //Return Dummy Data and information about authenticated 1R ID
                return ProcessRequest(clientCert);
            }
            catch (Exception ex)
            {
                return ProcessException(ex);
            }
        }

        /// <summary>
        /// Validate IDToken and Check if IAP is trusted.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task ValidateIDTokenAndSignature(HttpRequest request)
        {
            if (request == null)
            {
                throw new UnauthorizedAccessException("Unauthenticated.");
            }

            if (request.Headers == null)
            {
                throw new UnauthorizedAccessException("Authorization Header not found.");
            }

            var headers = request.Headers.ToList();

            #region Get and validate inputs
            var token = GetHeaderWithKey(headers, "Authorization").Replace("Bearer ", "");
            if (string.IsNullOrEmpty(token))
            {
                throw new UnauthorizedAccessException("IDToken not found.");
            }
            #endregion
            
            //Parse the IDToken
            var jwtToken = new JwtSecurityToken(token);
            if (jwtToken == null && jwtToken.Claims == null)
            {
                throw new UnauthorizedAccessException("Mailformed IDToken.");
            }

            var iss     = jwtToken.Claims.FirstOrDefault(i => i.Type == "iss")?.Value;
            var at_hash = jwtToken.Claims.FirstOrDefault(i => i.Type == "at_hash")?.Value;
            if (string.IsNullOrWhiteSpace(iss))
            {
                throw new UnauthorizedAccessException("Unable to find the token issuer.");
            }

            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(iss);
            if (disco.IsError)
            {
                throw new UnauthorizedAccessException("Unable to load information from the token issuer URL.");
            }

            var is4Client = new UraClient.OIDCUtil(disco.JwksUri);
            jwtToken = ValidateIDToken(token, is4Client.RSAParameters);
            
            var ski = is4Client.SKI;
            if (string.IsNullOrEmpty(ski))
            {
                throw new UnauthorizedAccessException("Unsupported algorithm: The valid algorithm is SHA256RSA.");
            }
            ValidateIAP(ski);
        }

        protected string GetHeaderWithKey(List<KeyValuePair<string, StringValues>> headers, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (headers == null) return string.Empty;


            return headers.FirstOrDefault(i => i.Key == key).Value + "";
        }

        private JsonResult CreateResultResponse(X509Certificate2 cert)
        {
            return new JsonResult(new OneRecordDummyResponse
            {
                subcriberID = new OneRecordTLSID
                {
                    desc = Constants.TYPE_USER_CERTIFICATE,
                    subjectDN = cert?.Subject,
                    validFrom = "" + cert?.NotBefore,
                    validTo = "" + cert?.NotAfter,
                    issuerDN = cert?.Issuer,
                    lastAuthenticatedAt = "" + DateTime.UtcNow,
                    oneRecordIDList = CertValidator.ParseOneRecordIDs(cert)
                },
                result = new OneRecordDummyData(),
                timestamp = DateTime.UtcNow.Ticks,
                message = Constants.VERIFY_OK,
            });
        }

        void ValidateTLSClientCertificate(X509Certificate2 clientCert)
        {
            if (clientCert == null)
            {
                throw new UnauthorizedAccessException("1R TLS Client Certificate for Subcriber is required.");
            }
            string cacheFolder = _configuration["CacheFolder"];
            var certStatus = (new CertValidator(cacheFolder)).Validate(clientCert.RawData);
            if (certStatus.Status != "Good")
            {
                throw new UnauthorizedAccessException(certStatus.ErrorMessage);
            }
        }

        /// <summary>
        /// Validate if IAP of the given token is TRUSTED
        /// </summary>
        /// <param name="ski"></param>
        void ValidateIAP(string ski)
        {
            /*
             * This validation should be done by the service which implement OneRecord APIs.
             * Please note that SKI often changes due to key rollover (rotation) of the IAP
             */
            return;
            /*
            var iapList = GetTrustIAPList();
            foreach(var iap in iapList)
            {
                if (iap.Identifier.Equals(ski)) 
                    return;
            }
            throw new UnauthorizedAccessException("Untrusted IAP error.");
            */
        }

        JwtSecurityToken ValidateIDToken(string jwtToken, RSAParameters param)
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(param);
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime   = true,
                RequireSignedTokens     = true,
                ValidateAudience        = false,
                ValidateIssuer          = false,
                ValidateLifetime        = false,
                IssuerSigningKey        = new RsaSecurityKey(rsa)
            };

            SecurityToken validatedSecurityToken = null;
            var handler = new JwtSecurityTokenHandler();
            handler.ValidateToken(jwtToken, validationParameters, out validatedSecurityToken);
            return validatedSecurityToken as JwtSecurityToken;
        }

        private JsonResult ProcessRequest(X509Certificate2 cert)
        {
            return CreateResultResponse(cert);
        }

        private JsonResult ProcessException(Exception ex)
        {
            if (ex is UnauthorizedAccessException)
            {
                Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            }
            else if (ex is BadHttpRequestException)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            return new JsonResult(new
            {
                Message = ex.Message
            });
        }
        IList<TrustOidcIP> GetTrustIAPList()
        {
            if (_trustIAPList == null)
            {
                _trustIAPList = new List<TrustOidcIP>() { 
                    new TrustOidcIP
                    {
                        Identifier = "c40642c7e970dcc0dd3c368ec3410fe68e647c6f",
                        Subject = "WISEID OIDC Demo",
                        Url = ""
                    }
                };
            }
            return _trustIAPList;
        }
        internal class TrustOidcIP
        {
            /// <summary>
            /// Subject Key Identifier
            /// </summary>
            public string Identifier
            {
                set;
                get;
            }
            public string Subject
            {
                set;
                get;
            }
            public string Url
            {
                set;
                get;
            }
        }
    }
}
