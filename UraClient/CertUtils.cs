namespace UraClient
{
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    public class CertUtils
    {
        public string CertFolder { get; set; }
        public CertUtils(string certFolder)
        {
            CertFolder = certFolder;
        }
        public string GetHeaderWithKey(List<KeyValuePair<string, StringValues>> headers, string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (headers == null) return string.Empty;


            return headers.FirstOrDefault(i => i.Key == key).Value + "";
        }
        public bool VerifySignature(X509Certificate2 cert, string signature, string key)
        {
            try
            {
                var input = Encoding.UTF8.GetBytes(key + cert?.SerialNumber);
                
                byte[] temp_backToBytes = Convert.FromBase64String(signature);
                bool v;
                using (var rsa = cert.GetRSAPublicKey())
                {
                    v = rsa.VerifyData(input, temp_backToBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                return v;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public X509Certificate2 LoadCertPfx(string certPath, string pass)
        {
            try
            {
                var cert = new X509Certificate2(certPath, pass);
                return cert;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
