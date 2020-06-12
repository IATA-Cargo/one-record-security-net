namespace UraClient
{
    using System;
    using System.Security.Cryptography.X509Certificates;

    public class CMSConnect
    {
        public X509Certificate2 GetCert(string url, out string error, string path = "", string pass = "")
        {
            error = string.Empty;

            try
            {
                var _uraClientCertPath = path;
                var _uraClientCertPass = pass;
                return new X509Certificate2(_uraClientCertPath, _uraClientCertPass);
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return null;
        }
    }
}
