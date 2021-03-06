﻿using Newtonsoft.Json;
using OidcPasswordFlow.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using RestSharp;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace OidcPasswordFlow.Controllers
{
    
    public class HomeController : BaseController
    {
        /// <summary>
        /// Intialize class
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="logger"></param>
        public HomeController(IConfiguration configuration, ILogger<HomeController> logger) 
            : base(configuration, logger)
        {
            
        }

        /// <summary>
        /// Default page
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            try
            {
                GetToken();

                return View();
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Errors), new { error = "Unauthorized", details = ex.Message });
            }
        }

        /// <summary>
        /// Error page
        /// </summary>
        /// <param name="error">The error content</param>
        /// <param name="details">The details of error</param>
        /// <returns></returns>
        public IActionResult Errors(string error, string details)
        {
            ViewBag.error = error;
            ViewBag.details = details;
            return View();
        }

        /// <summary>
        /// Get access token and id token from token service
        /// </summary>
        /// <returns></returns>
        private WiseIdPasswordResponse GetToken() {

            string host = GetConfig("TokenService:host");
            string username = GetConfig("TokenService:username");
            string password = GetConfig("TokenService:password");
            string appId = GetConfig("TokenService:appId");
            string appKey = GetConfig("TokenService:appKey");
            string certPath = GetConfig("TokenService:certPath");

            WiseIdPassword data = new WiseIdPassword(username, password, appId, appKey, certPath);

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(appId)
                || string.IsNullOrEmpty(appKey) || string.IsNullOrEmpty(certPath) || string.IsNullOrEmpty(host))
            {
                throw new Exception("Application configuration error");
            }

            if (string.IsNullOrEmpty(data.thumbprint) || string.IsNullOrEmpty(data.password)){
			    throw new Exception("Configuration data error");
            }

            string strBody = "";
		    try {

                var client = new RestClient(host);
                var request = new RestRequest(Method.POST);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");

                var datas = data.BuildFormData();
                foreach (var item in datas)
                {
                    request.AddParameter(item.Key, item.Value);
                }

                IRestResponse response = client.Execute(request);
                strBody = response?.Content;

                if (response.StatusCode == 0)
                {
                    throw new Exception("Can not connect to token server");
                }

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Retrieve token unsuccessful");
                }

		    } catch (Exception ex) {
			    logException(ex);
			    throw new Exception("Can not connect to token server");
		    }

		    try{
			    return JsonConvert.DeserializeObject<WiseIdPasswordResponse>(strBody);
		    } catch (Exception ex) {
			    logException(ex);
			    throw new Exception("Can not retrieve token data");
		    }

	    }
    }
}
