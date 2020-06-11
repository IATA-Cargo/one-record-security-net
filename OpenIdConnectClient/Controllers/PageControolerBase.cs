namespace WebOpenIdConnectClient.Controllers
{
    using IdentityModel.Client;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.IdentityModel.Tokens;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class PageControolerBase : Controller
    {

        private IConfiguration _configuration;
        protected IHttpContextAccessor _httpContextAccessor;

        /// <summary>
        /// Intialize base class and get injection services
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="httpContextAccessor"></param>
        public PageControolerBase(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;

        }

        /// <summary>
        /// Get config value
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        protected string GetConfig(string key)
        {
            if (_configuration == null) return string.Empty;
            try
            {
                return _configuration[key];
            }
            catch (Exception) { }
            return string.Empty;
        }

        /// <summary>
        /// Validate jwt token with public key get in oidc server and signature in token
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<string> validateIdTokenJwTAsync(string token)
        {

            try
            {
                JwtSecurityToken j = new JwtSecurityToken(token);
                var iss = "";
                var aud = "";
                var tokenClaims = j?.Claims;

                if (tokenClaims != null)
                {
                    iss = tokenClaims.FirstOrDefault(i => i.Type == "iss")?.Value + "";
                    aud = tokenClaims.FirstOrDefault(i => i.Type == "aud")?.Value + "";
                }

                // Define the client to access the IdentityServer Discovery-Endpoint
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync(iss);

                // Check if the token data exists in the request, parse is to a correct token
                var keylist = new List<SecurityKey>();
                foreach (var webKey in disco.KeySet.Keys)
                {
                    var exp = IdentityModel.Base64Url.Decode(webKey.E);
                    var mod = IdentityModel.Base64Url.Decode(webKey.N);
                    var key = new RsaSecurityKey(new System.Security.Cryptography.RSAParameters() { Modulus = mod, Exponent = exp });
                    keylist.Add(key);
                }

                // get the public key from the discovery-endpoint
                var keys = disco.KeySet.Keys;

                //define the parameters for validation of the token
                var parameters = new TokenValidationParameters
                {
                    ValidIssuer = disco.Issuer,
                    ValidAudience = aud,
                    IssuerSigningKeys = keylist,
                };

                var handler = new JwtSecurityTokenHandler();
                handler.InboundClaimTypeMap.Clear();

                //validate the token using the defined parameters, return the token when validation is succesful
                var claims = handler.ValidateToken(j.RawData, parameters, out var validatedtoken);

                if (validatedtoken != null) return string.Empty;

                return "Error";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            
        }

        /// <summary>
        /// Test validate token with call Introspection api of oidc server
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        protected async Task<string> validateIdTokenViaApiAsync(string token)
        {
            try
            {
                JwtSecurityToken j = new JwtSecurityToken(token);
                var iss = "";
                var aud = "";
                var tokenClaims = j?.Claims;

                if (tokenClaims != null)
                {
                    iss = tokenClaims.FirstOrDefault(i => i.Type == "iss")?.Value + "";
                    aud = tokenClaims.FirstOrDefault(i => i.Type == "aud")?.Value + "";
                }

                // Define the client to access the IdentityServer Discovery-Endpoint
                var client = new HttpClient();
                var disco = await client.GetDiscoveryDocumentAsync(iss);

                var nvc = new List<KeyValuePair<string, string>>();
                nvc.Add(new KeyValuePair<string, string>("token", token));
                HttpRequestMessage httpRequest = new HttpRequestMessage(HttpMethod.Post, disco.IntrospectionEndpoint);

                HttpClient client1 = new HttpClient();
                Task<HttpResponseMessage> response = client.SendAsync(httpRequest);
                var result = response.Result;

                return "Invalide";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Reformat to the pretty json string
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string JsonFormatPrettyStr(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data)) return string.Empty;
                dynamic jsonObj = JsonConvert.DeserializeObject(data);
                string json = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
                return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Check has session to authen
        /// </summary>
        /// <returns></returns>
        protected async Task<string> HasSessionAsync()
        {
            try
            {
                var properties = await Request.HttpContext.AuthenticateAsync();
                if (properties == null || properties.Properties?.Items == null) return "";

                foreach (var c in properties.Properties.Items)
                {
                    if ((c.Key + "").Contains("id_token"))
                    {
                        return c.Value;
                    }
                }
                return "";
            }
            catch (Exception)
            {
                return "";
            }
        }

        /// <summary>
        /// Convert object to the pretty string json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected string JsonFormatPretty(dynamic data)
        {
            try
            {
                if (data == null) return string.Empty;
                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Convert string to the object
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        protected T convert2Object<T>(string jsonData)
        {
            try
            {
                if (string.IsNullOrEmpty(jsonData)) return default(T);
                var obj = JsonConvert.DeserializeObject<T>(jsonData);
                return obj;
            }
            catch (Exception)
            {
                return default(T);
            }
        }
    }


}
