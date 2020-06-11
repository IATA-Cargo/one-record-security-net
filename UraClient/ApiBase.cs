namespace UraClient
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Security.Cryptography.X509Certificates;
    using UraClient.Model;

    public enum APIBaseMethod
    {
        GET,
        POST
    }

    public class ApiBase
    {

        #region Constructors and properties
        private const string CONST_API_CONTENT_TYPE = "application/json";

        /// <summary>
        /// Client certificate to connect to URA
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Enabel SSL3 
        /// </summary>
        public bool EnabelSSL3 { get; set; }

        /// <summary>
        /// Content type of api request
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// Constructor of class
        /// </summary>
        /// <param name="enabelSSL3"></param>
        /// <param name="cert"></param>
        /// <param name="contentType"></param>
        protected ApiBase(bool enabelSSL3, X509Certificate2 cert = null, string contentType = "")
        {
            ContentType = contentType;
            if (string.IsNullOrEmpty(contentType)) ContentType = CONST_API_CONTENT_TYPE;
            EnabelSSL3 = enabelSSL3;
            Certificate = null;
        }

        #endregion

        #region Main functions

        /// <summary>
        /// Post to a url
        /// </summary>
        /// <typeparam name="T">Type pf return object</typeparam>
        /// <param name="url">url</param>
        /// <param name="headers">headers of request</param>
        /// <param name="body">body of request</param>
        /// <param name="error">error</param>
        /// <param name="pathParam">pathParam</param>
        /// <returns>T Object</returns>
        protected T Post<T>(string url, List<APIBaseParameter> headers, dynamic body, out string error, List<APIBaseParameter> pathParam = null)
        {
            error = string.Empty;
            dynamic rst = _execute(url, APIBaseMethod.POST, headers, body, pathParam);
            error = rst.Error?.Message;
            if (rst?.Error != null) return default(T);

            var res = string2Json<T>(rst.ResponseString);

            return res;
        }

        /// <summary>
        /// Post to a url
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="headers">headers of request</param>
        /// <param name="body">body of request</param>
        /// <param name="error">error</param>
        /// <param name="pathParam">pathParam</param>
        /// <returns>string content of request</returns>
        protected string Post(string url, List<APIBaseParameter> headers, dynamic body, out string error, List<APIBaseParameter> pathParam = null)
        {
            error = string.Empty;
            var rst = _execute(url, APIBaseMethod.POST, headers, body, pathParam);
            error = rst.Error?.Message;
            return rst?.ResponseString;

        }

        /// <summary>
        /// Get to a url
        /// </summary>
        /// <typeparam name="T">Type pf return object</typeparam>
        /// <param name="url">url</param>
        /// <param name="headers">headers of request</param>
        /// <param name="error">error</param>
        /// <param name="pathParam">pathParam</param>
        /// <returns>T Object</returns>
        protected T Get<T>(string url, List<APIBaseParameter> headers, out string error, List<APIBaseParameter> pathParam = null)
        {

            error = string.Empty;
            var rst = _execute(url, APIBaseMethod.GET, headers, null, pathParam);
            error = rst.Error?.Message;
            if (rst?.Error != null) return default(T);

            var res = string2Json<T>(rst.ResponseString);

            return res;
        }

        /// <summary>
        /// Get to a url
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="headers">headers of request</param>
        /// <param name="error">error</param>
        /// <param name="pathParam">pathParam</param>
        /// <returns>string content of request</returns>
        protected string Get(string url, List<APIBaseParameter> headers, out string error, List<APIBaseParameter> pathParam = null)
        {
            error = string.Empty;
            var rst = _execute(url, APIBaseMethod.GET, headers, null, pathParam);
            error = rst?.Error?.Message;

            return rst?.ResponseString;
        }

        #endregion

        #region Utility functions

        /// <summary>
        /// Load certificate from local sertificate
        /// </summary>
        /// <param name="thumbprint">The thumbprint of certificate</param>
        /// <returns>Certificate or null</returns>
        protected X509Certificate2 LoadCertificateFromLocalMachine(string thumbprint)
        {
            X509Certificate2 ret = null;
            X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            if (store == null)
            {
                return null;
            }
            store.Open(OpenFlags.ReadOnly);

            string certThumbprint = thumbprint;
            if (string.IsNullOrEmpty(certThumbprint))
                return null;

            X509Certificate2Collection oSelectedCollection = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);

            if (oSelectedCollection.Count > 0 && oSelectedCollection[0].HasPrivateKey)
            {
                ret = oSelectedCollection[0];
            }

            store.Close();
            return ret;
        }

        /// <summary>
        /// Load certificate from file
        /// </summary>
        /// <param name="path">The path of certificate file</param>
        /// <param name="password">The password of certificate file</param>
        /// <returns>Certificate or null</returns>
        protected X509Certificate2 LoadCertificateFromFile(string path, string password = "")
        {
            try
            {
                if (string.IsNullOrEmpty(password))
                {
                    return new X509Certificate2(File.ReadAllBytes(path));
                }
                return new X509Certificate2(File.ReadAllBytes(path), password);
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Convert json object to string
        /// </summary>
        /// <param name="obj">Object</param>
        /// <returns>json string</returns>
        protected string json2String(dynamic obj)
        {
            if (obj == null) return "{ }";

            return JsonConvert.SerializeObject(obj);
        }

        /// <summary>
        /// Convert string to json object
        /// </summary>
        /// <param name="str">string data</param>
        /// <returns>Object</returns>
        protected dynamic string2Json(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return null;
            return JsonConvert.DeserializeObject<dynamic>(str);
        }

        /// <summary>
        /// Convert string to T object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="str"></param>
        /// <returns></returns>
        protected T string2Json<T>(string str)
        {
            if (string.IsNullOrWhiteSpace(str)) return default(T);
            return JsonConvert.DeserializeObject<T>(str);
        }

        #endregion

        #region Private functions

        private APIBaseResponse _execute(string url, APIBaseMethod method, List<APIBaseParameter> headers, dynamic body, List<APIBaseParameter> pathParam = null)
        {
            var rst = new APIBaseResponse
            {
                ResponseStatus = -1,
            };

            try
            {
                rst.ResponseString = string.Empty;
                var stringBody = string.Empty;

                // Make Url
                var uri = buildUrl(url, method, body, pathParam);

                // Make body
                if (body != null && method != APIBaseMethod.GET)
                {
                    stringBody = json2String(body);
                    //stringBody = stringBody.Replace("\\\\", "\\");
                }

                return _executeBase(uri, method, headers, stringBody);
            }
            catch (Exception ex)
            {
                if (rst.Error == null) rst.Error = ex;
            }
            return rst;
        }

        private APIBaseResponse _executeBase(string url, APIBaseMethod method, List<APIBaseParameter> headers, string strBody)
        {
            var rst = new APIBaseResponse
            {
                ResponseStatus = -1,
            };

            try
            {
                rst.ResponseString = string.Empty;

                try
                {
                    if (EnabelSSL3)
                    {
                        try
                        {
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls12;
                        }
                        catch (Exception) { }
                    }

                    var uri = new Uri(url);
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                    request.ContentType = ContentType;
                    request.Method = method + string.Empty;
                    request.KeepAlive = false;
                    //request.UseDefaultCredentials = true;

                    // Add header
                    if (headers != null)
                    {
                        foreach (var header in headers)
                        {
                            if (string.IsNullOrEmpty(header.Key)) continue;
                            if (header.Key.ToLower() == "content-type")
                            {
                                continue;
                            }
                            if (header.Key.ToLower() == "host")
                            {
                                //request.Host = (string.IsNullOrEmpty(header.StringValue) ? (header.Value + "") : header.StringValue);
                                //request.Headers.Add(header.Key, (string.IsNullOrEmpty(header.StringValue) ? (header.Value + "") : header.StringValue));
                                continue;
                            }
                            request.Headers.Add(header.Key, (string.IsNullOrEmpty(header.StringValue) ? (header.Value + "") : header.StringValue));
                        }
                    }


                    // Add cert
                    if (Certificate != null)
                    {
                        request.ClientCertificates.Clear();
                        request.ClientCertificates.Add(Certificate);
                    }
                    else
                    {
                        if (uri.Scheme.ToLower() == "https")
                        {
                            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                        }
                    }

                    if (method != APIBaseMethod.GET && !string.IsNullOrWhiteSpace(strBody))
                    {
                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            streamWriter.Write(strBody);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                    }

                    var response = (WebResponse)request.GetResponse();
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        rst.ResponseString = streamReader.ReadToEnd();
                    }
                    rst.ResponseStatus = 200;
                }
                catch (WebException e)
                {
                    rst.Error = e;
                    if (e.Response != null)
                    {
                        using (WebResponse wresponse = e.Response)
                        {

                            HttpWebResponse httpResponse = (HttpWebResponse)wresponse;
                            rst.ResponseStatus = (int)httpResponse.StatusCode;
                            using (Stream data = wresponse.GetResponseStream())
                            using (var reader = new StreamReader(data))
                            {
                                rst.ResponseString = reader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (rst.Error == null) rst.Error = ex;
            }
            return rst;
        }

        private string buildUrl(string url, APIBaseMethod method, dynamic body, List<APIBaseParameter> pathParam = null)
        {
            var ret = (url + "").Trim();
            var hasParam = ret.Contains("?");
            var hasParamVal = ret.Contains("=");
            var tmp = string.Empty;
            if (pathParam != null)
            {
                foreach (var param in pathParam)
                {
                    tmp += "&" + param.Key + "=" + (string.IsNullOrEmpty(param.StringValue) ? (param.Value + "") : param.StringValue);
                }
            }
            var vBody = body as List<APIBaseParameter>;
            if (method == APIBaseMethod.GET && vBody != null)
            {

                foreach (var param in vBody)
                {
                    tmp += "&" + param.Key + "=" + (string.IsNullOrEmpty(param.StringValue) ? (param.Value + "") : param.StringValue);
                }
            }

            if (string.IsNullOrEmpty(tmp)) return ret;

            if (hasParam)
            {
                if (hasParamVal)
                {
                    ret += tmp;
                }
                else
                {
                    ret += tmp.Substring(1);
                }
            }

            return ret;
        }

        #endregion

    }

    

    

    
}