namespace OpenIdConnectClient.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using System.Threading.Tasks;
    using System.IdentityModel.Tokens.Jwt;
    using Microsoft.AspNetCore.Http;
    using System.Net.Http;
    using WebOpenIdConnectClient.Api;
    using System.Collections.Generic;
    using WebOpenIdConnectClient.Models;

    [Route("[controller]")]
    public class IdentityController : ApiControllerBase
    {
        public IdentityController(IConfiguration configuration) : base(configuration)
        {
        }

        /// <summary>
        /// Get Method of uri
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var clientCert = Request.HttpContext.Connection.ClientCertificate;

                // Validate signature
                var (msgErrorSign, certOut, type) = await ValidateSignatureAsync(Request);
                if (!string.IsNullOrEmpty(msgErrorSign))
                {
                    return CreateErrorResponse(msgErrorSign, clientCert == null ? "Client cert emtpy." : "");
                }

                // Return client cert information
                if (clientCert != null)
                {
                    var sans = new UraClient.CertUtils("").LoadSansCert(clientCert);
                    return CreateResultResponse(clientCert, sans);
                }

                // Return signed cert in config, maybe this cert must get from URA
                return CreateResultResponse(certOut);
            }
            catch (Exception ex)
            {
                return CreateExceptionResponse(ex);
            }
        }

        #region Private functions

        /// <summary>
        /// Validate signaute with id token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private async Task<(string, X509Certificate2, int)> ValidateSignatureAsync(HttpRequest request)
        {
            if (request == null) return ("Request headers not found", null, 0);
            if (request.Headers == null) return ("Request headers not found", null, 0);

            var headers = request.Headers.ToList();

            #region Get and validate inputs

            var accessToken = GetHeaderWithKey(headers, "Authorization").Replace("Bearer ", "");
            if (string.IsNullOrEmpty(accessToken)) return ("Token not found", null, 0);

            #endregion

            #region GetIss

            string iss = string.Empty;
            string at_hash = string.Empty;
            var jwtToken = new JwtSecurityToken(accessToken);
            if (jwtToken == null && jwtToken.Claims == null)
            {
                return ("Token is invalid", null, 0);
            }

            iss = jwtToken.Claims.FirstOrDefault(i => i.Type == "iss")?.Value;
            at_hash = jwtToken.Claims.FirstOrDefault(i => i.Type == "at_hash")?.Value;

            if (string.IsNullOrWhiteSpace(iss))
            {
                return ("iss uri is empty", null, 0);
            }

            #endregion

            #region Get ski

            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(iss);
            if (disco.IsError)
            {
                return ("iss uri is invalid", null, 0);
            }

            var is4Client = new UraClient.Identity4Connect();
            var ski = is4Client.GetSKI(disco.JwksUri, out string error);
            if (string.IsNullOrEmpty(ski) || !string.IsNullOrEmpty(error))
            {
                return ("Can not get ski: " + error, null, 0);
            }

            #endregion

            #region Get signing certificate from ura or in local storage

            var cmsClient = new UraClient.CMSConnect();
            var cmdCert = GetConfigString("Cert:SingedCert");
            var cmdCertPass = GetConfigString("Cert:SingedCertPass");
            var cert = cmsClient.GetCert(disco.JwksUri, out string cmsError, cmdCert, cmdCertPass);
            if (cert == null)
                return ("Signature not match", null, 0);

            #endregion Get signing certificate

            return (string.Empty, cert, 1);
        }

        /// <summary>
        /// Create response to client
        /// </summary>
        /// <param name="cert">Certificate show</param>
        /// <param name="sans">Sans of certificate</param>
        /// <returns>Object for client</returns>
        private JsonResult CreateResultResponse(X509Certificate2 cert, List<string> sans = null)
        {
            if (sans == null)
            {
                return new JsonResult(new VerifyResponse
                {
                    authenticatedSubject = new AuthenticatedSubject
                    {
                        type = Constances.TYPE_USER_CERTIFICATE,
                        subjectDN = cert?.Subject,
                        validFrom = "" + cert?.NotBefore,
                        validTo = "" + cert?.NotAfter,
                        issuerDN = cert?.Issuer,
                        lastAuthenticatedAt = "" + DateTime.UtcNow,
                    },
                    result = new VerifyResponseData(),
                    timestamp = DateTime.UtcNow.Ticks,
                    message = Constances.VERIFY_OK,
                });
            }
            else
            {
                return new JsonResult(new VerifyResponseMutual
                {
                    authenticatedSubject = new AuthenticatedSubjectMutual
                    {
                        type = Constances.TYPE_SSL_MUTUAL,
                        subjectDN = cert?.Subject,
                        validFrom = "" + cert?.NotBefore,
                        validTo = "" + cert?.NotAfter,
                        issuerDN = cert?.Issuer,
                        lastAuthenticatedAt = "" + DateTime.UtcNow,
                        sans = sans
                    },
                    result = new VerifyResponseData(),
                    timestamp = DateTime.UtcNow.Ticks,
                    message = Constances.VERIFY_OK,
                });
            }
        }

        #endregion

    }
}