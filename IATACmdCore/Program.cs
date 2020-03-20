using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IATACmdCore
{
    class ApiRequest
    {
        private IConfigurationRoot _configuration;

        HttpStatusCode StatusCode
        {
            get
            {
                return _statusCode;
            }
        }
        protected HttpStatusCode _statusCode = HttpStatusCode.OK;

        protected string _apiUrl = null;
        protected string _rawMessage = string.Empty;
        protected string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }
        public string RawMessage
        {
            get
            {
                return _rawMessage;
            }
        }
        public ApiRequest(string apiUrl = null, IConfigurationRoot configuration = null)
        {
            _configuration = configuration;
            _apiUrl = apiUrl;
        }
        async Task RequestAsync(int id)
        {

            var handler = new HttpClientHandler();
            handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //handler.SslProtocols = SslProtocols.Tls12;
            if (!string.IsNullOrEmpty(_configuration["SSLCertThmbprint"]))
            {
                //Perfome SSL Pinning. It is required to get SHA1 thumbprint of the SSL Server Certificate
                //and put it in the appSettings
                //Maybe the Key Identifier should be used. But the first version we should simply use SHA1 thumbprint.
                handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, error) =>
                {
                    var fixedSID = _configuration["SSLCertThmbprint"];
                    var ch = X509Chain.Create();
                    ch.ChainPolicy = new X509ChainPolicy
                    {
                        RevocationFlag = X509RevocationFlag.EndCertificateOnly,
                        RevocationMode = X509RevocationMode.NoCheck
                    };

                    //It is required to build the certificate chain upto the TRUSTED ROOT located in 
                    //TRUST ROOT Certficate Store 
                    if (ch.Build(cert as X509Certificate2))
                    {
                        foreach (var s in ch.ChainStatus)
                        {
                            if (s.Status != X509ChainStatusFlags.NoError)
                            {
                                return false;
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }

                    //And finally compare the thumbprints.  
                    var cert2 = cert as X509Certificate2;
                    string sidValue = cert2.Thumbprint.ToLower();

                    if (string.IsNullOrEmpty(sidValue))
                        sidValue = BitConverter.ToString(
                            (new SHA1Managed()).ComputeHash(cert2.GetPublicKey())).Replace("-", string.Empty).ToLower();

                    if (string.IsNullOrEmpty(sidValue))
                    {
                        return false;
                    }

                    if (sidValue == fixedSID)
                    {
                        return true;
                    }
                    return false;
                };
            }

            //Loading the client certificate to setup SSL Client Authentication.
            var clientCertFile = _configuration["SSLClientCert"];
            var clientCertFilePwd = _configuration["SSLClientCertPwd"];
            var clientCert = new X509Certificate2(clientCertFile, clientCertFilePwd);
            handler.ClientCertificates.Add(clientCert);

            var client = new HttpClient(handler);
            var url = _apiUrl + id;
            var outgoing = new Uri(url);
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, outgoing);
            var response = await client.SendAsync(httpRequest).ConfigureAwait(false);
            _statusCode = response.StatusCode;
            _rawMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// Create a sample request under SSL Client Authentication 
        /// </summary>
        /// <param name="id"></param>
        public void Request(int id)
        {
            RequestAsync(id).GetAwaiter().GetResult();
        }
    }
    class Program
    {
        public static IConfigurationRoot _configuration;

        static void Main(string[] args)
        {
            buildConfigApplication();

            //(new ApiRequest("https://localhost/IATADev/api/values/", _configuration)).Request(1);
            (new ApiRequest("https://localhost/iatadevcore/weatherforecast/", _configuration)).Request(1);
        }

        static void buildConfigApplication()
        {
           
            var services = new ServiceCollection();

            // Set up configuration sources.
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSetting.json");

            _configuration = builder.Build();

        }
    }
}
