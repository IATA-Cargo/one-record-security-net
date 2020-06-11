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
    using System.Text.RegularExpressions;

    public class CertUtils
    {
        public string CertFolder { get; set; }

        public CertUtils(string certFolder)
        {
            CertFolder = certFolder;
        }

        /// <summary>
        /// Store certificate to local file
        /// </summary>
        /// <param name="cert"></param>
        public void StoreCertificate(string key, int? requestid, int? certId, string csr, string cert, string cert_ps12, string pass)
        {
            try
            {
                if (requestid == null) return;

                string currentPath = CertFolder;
                string suffix = "_" + requestid + "_" + key;
                if (!Directory.Exists(currentPath)) Directory.CreateDirectory(currentPath);

                File.WriteAllText(currentPath + "\\request_id" + suffix + ".txt", 
                                  requestid + "");

                if (certId != null)
                {
                    File.WriteAllText(currentPath + "\\cert_id" + suffix + ".txt",
                                      certId + "");
                }

                if (!string.IsNullOrEmpty(csr))
                {
                    var saveStr = "-----BEGIN CERTIFICATE REQUEST-----";
                    byte[] csrArr = Convert.FromBase64String(csr);
                    saveStr += Convert.ToBase64String(csrArr, Base64FormattingOptions.InsertLineBreaks);
                    saveStr += "-----END CERTIFICATE REQUEST-----";
                    File.WriteAllText(currentPath + "\\csr" + suffix + ".txt", saveStr);
                }

                if (!string.IsNullOrEmpty(cert))
                {
                    var saveStr = "-----BEGIN CERTIFICATE-----";
                    byte[] csrArr = Convert.FromBase64String(cert);
                    saveStr += Convert.ToBase64String(csrArr, Base64FormattingOptions.InsertLineBreaks);
                    saveStr += "-----END CERTIFICATE-----";
                    File.WriteAllText(currentPath + "\\cert" + suffix + ".cer",
                                      saveStr + "");
                }

                if (!string.IsNullOrEmpty(cert_ps12))
                {
                    File.WriteAllText(currentPath + "\\cert" + suffix + ".pfx",
                                      cert_ps12 + "");
                }

                if (!string.IsNullOrEmpty(pass))
                {
                    File.WriteAllText(currentPath + "\\passPhrase" + suffix + ".txt",
                                      pass + "");
                }
            }
            catch (Exception)
            {
            }
        }


        public string LoadRequestId(string key)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "request_id_*" + key + ".txt", SearchOption.AllDirectories)
                                     .Where(s => s.EndsWith(".txt"));

                var path = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(path))
                {
                    var content = File.ReadAllText(path);
                    return content;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LoadCertStoreWithId(string id)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "cert_" + id + "_*.pfx", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".pfx"));

                var path = files.FirstOrDefault();
                //if (!string.IsNullOrEmpty(path))
                //{
                //    var content = File.ReadAllText(path);
                //    return content;
                //}
                return path;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LoadPassphareStoreWithId(string id)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "passPhrase_" + id + "_*.txt", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".txt"));

                var path = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(path))
                {
                    var content = File.ReadAllText(path);
                    return content;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LoadCertStore(string key)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "*" + key + ".pfx", SearchOption.AllDirectories)
            .               Where(s => s.EndsWith(".pfx"));

                var path = files.FirstOrDefault();
                //if (!string.IsNullOrEmpty(path))
                //{
                //    var content = File.ReadAllText(path);
                //    return content;
                //}
                return path;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LoadCertFile(string key)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "*" + key + ".cer", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".cer"));

                var path = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(path))
                {
                    var content = File.ReadAllText(path);
                    return content;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string LoadPassphareStore(string key)
        {
            try
            {
                var files = Directory.EnumerateFiles(CertFolder, "passPhrase_*_" + key + ".txt", SearchOption.AllDirectories)
            .Where(s => s.EndsWith(".txt"));

                var path = files.FirstOrDefault();
                if (!string.IsNullOrEmpty(path))
                {
                    var content = File.ReadAllText(path);
                    return content;
                }
                return string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        public string CreateSignNature(X509Certificate2 pkcs12, string key)
        {
            var signature = string.Empty;
            try
            {
               
                byte[] signs;
                var input = Encoding.UTF8.GetBytes(key + pkcs12?.SerialNumber);
                using (RSA rsa = pkcs12.GetRSAPrivateKey())
                {
                    signs = rsa.SignData(input, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                }
                signature = Convert.ToBase64String(signs, 0, signs.Length);//SignData(key + SerialNumber, privKey);


                //byte[] temp_backToBytes = Convert.FromBase64String(signature);
                //bool v;
                //using (var rsa = pkcs12.GetRSAPublicKey())
                //{
                //    v = rsa.VerifyData(input, temp_backToBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                //}

            }
            catch (Exception)
            {
            }
            return signature;
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

        public X509Certificate2 LoadCertPfxBase64(string certPath, string pass)
        {
            try
            {
                var byt = File.ReadAllText(certPath);
                byte[] bytes = System.Convert.FromBase64String(byt);

                var cert = new X509Certificate2(bytes, pass, X509KeyStorageFlags.PersistKeySet);

                return cert;
            }
            catch (Exception)
            {
                return null;
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

        public X509Certificate2 LoadCertPfx(string key)
        {
            try
            {
                var byt = LoadCertStore(key);
                var byt1 = LoadPassphareStore(key);

                return LoadCertPfxBase64(byt, byt1);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public List<string> LoadSansCert(X509Certificate2 cert)
        {
            var result = new List<string>();

            if (cert == null) return result;

            try
            {
                var subjectAlternativeName = cert.Extensions.Cast<X509Extension>()
                .Where(n => n.Oid.Value == "2.5.29.17")
                .Select(n => new AsnEncodedData(n.Oid, n.RawData))
                .Select(n => n.Format(true))
                .FirstOrDefault();

                return string.IsNullOrWhiteSpace(subjectAlternativeName)
                    ? new List<string>()
                    : subjectAlternativeName.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(n => Regex.Match(n, @"^URL=(.+)"))
                        .Where(r => r.Success && !string.IsNullOrWhiteSpace(r.Groups[1].Value))
                        .Select(r => r.Groups[1].Value)
                        .ToList();
            }
            catch (Exception)
            {
            }

            return result;
        }
    }
}
