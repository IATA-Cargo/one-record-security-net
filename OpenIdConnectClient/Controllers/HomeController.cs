﻿namespace OpenIdConnectClient.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authentication;
    using Microsoft.AspNetCore.Http;
    using System.IdentityModel.Tokens.Jwt;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using WebOpenIdConnectClient.Models;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Authentication;
    using WebOpenIdConnectClient.Controllers;
    using UraClient;

    [Authorize]
    public class HomeController : PageControolerBase
    {

        /// <summary>
        /// Intialize class
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="configuration"></param>
        public HomeController(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
            ) : base(configuration, httpContextAccessor)
        {
            
        }

        #region Actions

        /// <summary>
        /// Home page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await LoadCommonValue4ViewAsync("Home");
            return View();
        }

        /// <summary>
        /// Simple Api page
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Api(string id)
        {
            await LoadCommonValue4ViewAsync("Sample Api");

            var Tokens = new List<ShowValue>();
            var AccessToken = "";
            var IdToken = "";
            try
            {
                var properties = await Request.HttpContext.AuthenticateAsync();
                AccessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                IdToken = await _httpContextAccessor.HttpContext.GetTokenAsync("id_token");
                if (properties != null && properties.Properties?.Items != null)
                {
                    Tokens = (from c in properties.Properties.Items
                              select new ShowValue
                              {
                                  Key = c.Key,
                                  Value = c.Value
                              }).ToList();
                }
            }
            catch (Exception)
            {
            }

            var vm = new CommonViewModel
            {
                IdToken = IdToken,
                AccessToken = AccessToken,
                ApiUri = GetConfig("ApiVerify"),
                ApiRequestHeader = "",
                ApiRequestRespose = "",
                AuthenticatedSubject = "",
            };

            try
            {
                var jwtToken = new JwtSecurityToken(IdToken);
                if (jwtToken == null && jwtToken.Claims == null)
                {
                    vm.VerifyMessage = "Verify error: jwt is not found data";
                    return View(vm);
                }

                if (string.IsNullOrEmpty(AccessToken))
                {
                    vm.VerifyMessage = "Verify error: access token is not found";
                    return View(vm);
                }
                
                var rstMessage = "";
                vm.VerifyMessage = rstMessage;
                X509Certificate2 cert = new CertUtils(string.Empty)
                    .LoadCertPfx(GetConfig("Cert:SingedCert"), GetConfig("Cert:SingedCertPass"));
                
                vm.ApiRequestHeader = JsonFormatPretty(new { id_token = IdToken });
                rstMessage = callApiVerify(IdToken, cert);

                vm.VerifyMessage = rstMessage;
                var resultObj = convert2Object<VerifyResponse>(rstMessage);

                vm.VerifyResult = resultObj?.message == Constances.VERIFY_OK;
                vm.ApiRequestRespose = JsonFormatPrettyStr(rstMessage);
                vm.AuthenticatedSubject = GetAuthenSubject(rstMessage);
                vm.ResultData = GetDataResult(rstMessage);
            }
            catch (Exception ex)
            {
                vm.VerifyResult = false;
                vm.VerifyMessage = ex.Message;
            }

            return View(vm);
        }

        /// <summary>
        /// Profile page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            await LoadCommonValue4ViewAsync("Profile");

            List<ShowValue> Tokens = new List<ShowValue>();
            var AccessToken = "";
            var IdToken = "";
            var authenObject = "";
            var username = "";
            var role = "";
            try
            {
                if (Request.HttpContext.User.Claims != null)
                {
                    var claims = (from c in Request.HttpContext.User.Claims
                              select new ShowValue
                              {
                                  Key = c.Type,
                                  Value = c.Value
                              }).ToList();

                    username = claims.FirstOrDefault(i => (i.Key == "name"))?.Value + "";
                    role = claims.FirstOrDefault(i => (i.Key == "role"))?.Value + "";
                }

                AccessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                IdToken = await _httpContextAccessor.HttpContext.GetTokenAsync("id_token");

                var properties = await Request.HttpContext.AuthenticateAsync();
                if (properties != null && properties.Properties?.Items != null)
                {
                    Tokens = (from c in properties.Properties.Items
                              select new ShowValue
                              {
                                  Key = c.Key,
                                  Value = c.Value
                              }).ToList();
                }
                
                try
                {
                    authenObject = JsonConvert.SerializeObject(Tokens);
                }
                catch (Exception) { }
            }
            catch (Exception)
            {
            }

            var vm = new CommonViewModel
            {
                IdToken = IdToken,
                AccessToken = AccessToken,
                VerifyMessage = authenObject,
                Username = username,
                Role = role,
            };

            return View(vm);
        }

        /// <summary>
        /// Signout page
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Signout()
        {
            try
            {
                await LoadCommonValue4ViewAsync("Signout");
                await Request.HttpContext.SignOutAsync();

                ViewData["HasSession"] = "";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// View details of user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> DownloadIndentity()
        {

            ViewData["MenuVerify"] = GetConfig("MenuVerifyShow");
            var contentType = "application/force-download";
            var fileName = "identity.json";
            try
            {
                var properties = await Request.HttpContext.AuthenticateAsync();
                var data = new Dictionary<string, string>();
                var datastr = "";
                if (properties != null && properties.Properties?.Items != null)
                {
                    foreach (var c in properties.Properties.Items)
                    {
                        if (data.ContainsKey(c.Key)) continue;
                        data.Add(c.Key, c.Value);
                    }
                    
                }
                datastr = JsonFormatPretty(data);
                return File(System.Text.Encoding.UTF8.GetBytes(datastr), contentType, fileName);
            }
            catch (Exception)
            {
                return File(System.Text.Encoding.UTF8.GetBytes(""), contentType, fileName);
            }
        }

        #endregion

        #region Private functions

        /// <summary>
        /// Set common data for view
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private async Task LoadCommonValue4ViewAsync(string title)
        {
            ViewData["MenuVerify"] = GetConfig("MenuVerifyShow");
            ViewData["HasSession"] = await HasSessionAsync();
            ViewData["Title"] = title;
        }

        /// <summary>
        /// Call api to verify token
        /// </summary>
        /// <param name="token"></param>
        /// <param name="certClient"></param>
        /// <returns></returns>
        private string callApiVerify(string token, X509Certificate2 certClient = null)
        {
            try
            {
                HttpClient client = null;
                if (certClient == null)
                {

                    client = new HttpClient();
                }
                else
                {
                    var handler = new HttpClientHandler
                    {
                        ClientCertificateOptions = ClientCertificateOption.Manual,
                        SslProtocols = SslProtocols.Tls12
                    };
                    handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, error) => {
                        return true;
                    };
                    handler.ClientCertificates.Add(certClient);
                    client = new HttpClient(handler);
                }
                
                client.BaseAddress = new Uri(GetConfig("ApiVerify"));
                
                // Add an Accept header for JSON format.
                client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                
                var dataObjects = "";
                // List data response.
                HttpResponseMessage response = client.GetAsync("").Result;
                if (response.IsSuccessStatusCode)
                {
                    dataObjects = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    dataObjects = "Error: " + response.StatusCode + ". " + response.RequestMessage + "  " +  response.Content.ReadAsStringAsync().Result;
                }

                client.Dispose();

                return dataObjects;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Get result data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetDataResult(string data)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(data)) return string.Empty;
                var jsonObj = JsonConvert.DeserializeObject<VerifyResponse>(data);
                string json = JsonConvert.SerializeObject(jsonObj?.result, Formatting.Indented);
                return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Get authen dataa
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private string GetAuthenSubject(string data)
        {
            try
            {
                string json = string.Empty;
                if (string.IsNullOrWhiteSpace(data)) return string.Empty;
                var jsonObj = JsonConvert.DeserializeObject<VerifyResponse>(data);
                if (jsonObj?.authenticatedSubject?.type == Constances.TYPE_SSL_MUTUAL)
                {
                    var jsonMutual = JsonConvert.DeserializeObject<VerifyResponseMutual>(data);
                    json = JsonConvert.SerializeObject(jsonMutual?.authenticatedSubject, Formatting.Indented);
                }
                else
                {
                    json = JsonConvert.SerializeObject(jsonObj?.authenticatedSubject, Formatting.Indented);
                }
                return json;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }


        #endregion Private functions
    }

}
