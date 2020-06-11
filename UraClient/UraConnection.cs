namespace UraClient
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Class to define object connect to URA service
    /// </summary>
    public class UraConnection : ApiBase
    {

        private const string HEADER_UID = "X-App-ID";

        /// <summary>
        /// Ura base uri
        /// </summary>
        public string HostBase { get; set; }

        /// <summary>
        /// Application Id in URA
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Constructor to initiliaze class
        /// </summary>
        /// <param name="hostBase">Ura base uri</param>
        /// <param name="appId">Application Id in URA</param>
        /// <param name="clientCertPath">The client certificate path</param>
        /// <param name="clientCertPassword">The client certificate password</param>
        public UraConnection(string hostBase, string appId, string clientCertPath, string clientCertPassword) : base(true)
        {
            HostBase = hostBase;
            ApplicationId = appId;

            var cert = this.LoadCertificateFromFile(clientCertPath, clientCertPassword);
            Certificate = cert;
        }

        /// <summary>
        ///  Verifies the ability to connect to and authenticate with the URA using the supplied credentials
        /// </summary>
        /// <param name="error">Out the description of error</param>
        /// <returns>True: Authentication, False: other</returns>
        public bool Authentication(out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/authentication";
                var headers = BuildHeaders();
                var result = Get<Model.ResponseBase>(url, headers, out error);
                if (string.IsNullOrEmpty(error) && result?.code == 200)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return false;
            }
        }

        /// <summary>
        /// Call api to create a certificate request
        /// </summary>
        /// <param name="body">Content of the certificate request</param>
        /// <param name="error">Out the description of error</param>
        /// <returns>CreateRequestResponse object</returns>
        public Model.CreateRequestResponse CreateRequest(Model.CreateRequestInput body, out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/request/submit";
                var headers = BuildHeaders();
                var result = Post<Model.CreateRequestResponse>(url, headers, body, out error);

                return result;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Call api to create a pfx certificate request
        /// </summary>
        /// <param name="body">Content of the certificate request</param>
        /// <param name="error">Out the description of error</param>
        /// <returns>CreateRequestResponse object</returns>
        public Model.CreateRequestResponse CreateRequestPfx(Model.CreateRequestInput body, out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/request/submitpfx";
                var headers = BuildHeaders();
                var result = Post<Model.CreateRequestResponse>(url, headers, body, out error);

                return result;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Approve a certificate request
        /// </summary>
        /// <param name="requestId">The id of the certificate request</param>
        /// <param name="error">Out the description of error</param>
        /// <returns>ApproveRequestResponse object</returns>
        public Model.ApproveRequestResponse ApproveRequest(int requestId, out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/request/approve";
                var headers = BuildHeaders();
                var body = new
                {
                    requestId = requestId
                };

                var result = Post<Model.ApproveRequestResponse>(url, headers, body, out error);
                return result;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return null;
            }
        }

        /// <summary>
        /// Retrieve Certificate from a certificate request
        /// </summary>
        /// <param name="requestId">The id of the certificate request</param>
        /// <param name="error">Out the description of error</param>
        /// <returns>RetrieveCertificateResponse object</returns>
        public Model.RetrieveCertificateResponse RetrieveCertificate(int requestId, out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/certificate/getbyrequest?requestid=" + requestId;
                var headers = BuildHeaders();

                var result = Get<Model.RetrieveCertificateResponse>(url, headers, out error);
                return result;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return null;
            }

        }

        /// <summary>
        /// Revoke a certificate
        /// </summary>
        /// <param name="body">Information to revoke certifiate</param>
        /// <param name="error">Out the description of error</param>
        /// <returns>RevokeCertificateResponse object</returns>
        public Model.RevokeCertificateResponse RevokeCertificate (Model.RevokeCertificateInput body, out string error)
        {
            error = string.Empty;
            try
            {
                string url = HostBase + "/certificate/revoke";
                var headers = BuildHeaders();

                var result = Post<Model.RevokeCertificateResponse>(url, headers, body, out error);
                return result;
            }
            catch (Exception ex)
            {
                error = GetError(ex);
                return null;
            }
        }

        #region Private functions

        /// <summary>
        /// Build headers of request
        /// </summary>
        /// <returns>List header object</returns>
        private List<Model.APIBaseParameter> BuildHeaders()
        {
            return new List<Model.APIBaseParameter>
            {
                new Model.APIBaseParameter
                {
                    Key = HEADER_UID,
                    Value = this.ApplicationId,
                    StringValue = this.ApplicationId,
                }
            };
        }

        /// <summary>
        /// Build headers of request
        /// </summary>
        /// <returns>List header object</returns>
        private string GetError(Exception ex)
        {
            if (ex == null) return string.Empty;
            var lastEx = ex;
            if (lastEx.InnerException != null) lastEx = lastEx.InnerException;
            if (lastEx.InnerException != null) lastEx = lastEx.InnerException;

            return string.Format("Error: " + lastEx.Message + "\n" + "Trace: " + lastEx.StackTrace);
        }

        #endregion

    }
}
