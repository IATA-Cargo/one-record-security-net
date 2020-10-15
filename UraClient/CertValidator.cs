using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Ocsp;
using Org.BouncyCastle.X509;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace UraClient
{
    public class CertValidator
    {
        string _cacheFolder = null;
        public CertValidator(string cacheFolder)
        {
            _cacheFolder = cacheFolder;
        }
        public IList<string> OneRecordIDs
        {
            get;
            private set;
        }

        /// <summary>
        /// Validate a certificate.
        /// if the AIA OCSP is available, then it will be used. Otherwise
        /// the CDP will be used for validation.
        /// Unknown status will be returned if the cert does not include any 
        /// URL for validation.
        /// </summary>
        /// <param name="requestByteArray"></param>
        /// <returns></returns>
        public CertStatus Validate(byte[] requestByteArray)
        {
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(requestByteArray);
            OneRecordIDs = ParseOneRecordIDs(cert);
            var aia = ParseAIA(cert);

            //No AIA endpoints.
            if (aia == null || string.IsNullOrEmpty(aia.Ocsp) || string.IsNullOrEmpty(aia.Issuer))
            {
                return ValidateCertificateAgainstCRL(cert);
            }
            else
            {
                return Validate(cert, aia);
            }
        }

        /// <summary>
        /// Validate a certificate against its AIA OCSP.
        /// </summary>
        /// <param name="cert"></param>
        /// <param name="aia"></param>
        /// <returns></returns>
        CertStatus Validate(System.Security.Cryptography.X509Certificates.X509Certificate2 cert,
                                   AIA aia)
        {

            string hash = ComputeSHA1(System.Text.ASCIIEncoding.ASCII.GetBytes(aia.Issuer));
            string filePath = IssuerCachedFolder + hash;

            //Check if aki is cached
            if (!IsIssuerCached(aia.Issuer))
            {
                Download(aia.Issuer, filePath);
                if (!IsIssuerCached(aia.Issuer))
                {
                    return CertStatus.Unknown(CertStatus.BadIssuer);
                }
            }

            var issuerTemp = new System.Security.Cryptography.X509Certificates.X509Certificate2(filePath);
            var certParser = new Org.BouncyCastle.X509.X509CertificateParser();
            var issuer = certParser.ReadCertificate(issuerTemp.RawData);
            var cert2Validate = certParser.ReadCertificate(cert.RawData);

            var id = new Org.BouncyCastle.Ocsp.CertificateID(
                                    Org.BouncyCastle.Ocsp.CertificateID.HashSha1,
                                    issuer,
                                    cert2Validate.SerialNumber);

            byte[] reqEnc = GenerateOCSPRequest(id, cert2Validate);
            byte[] resp = GetOCSPResponse(aia.Ocsp, reqEnc);

            //Extract the response
            OcspResp ocspResponse = new OcspResp(resp);

            BasicOcspResp basicOCSPResponse =
                    (BasicOcspResp)ocspResponse.GetResponseObject();

            SingleResp singResp = basicOCSPResponse.Responses[0];

            //Validate ID
            var expectedId = singResp.GetCertID();
            if (!expectedId.SerialNumber.Equals(id.SerialNumber))
            {
                return CertStatus.Unknown(CertStatus.BadSerial);
            }

            if (!Org.BouncyCastle.Utilities.Arrays.AreEqual(expectedId.GetIssuerNameHash(), id.GetIssuerNameHash()))
            {
                return CertStatus.Unknown(CertStatus.IssuerNotMatch);
            }

            //Extract Status
            var certificateStatus = singResp.GetCertStatus();

            if (certificateStatus == null)
                return CertStatus.Good;

            if (certificateStatus is Org.BouncyCastle.Ocsp.RevokedStatus)
            {
                int revocationReason = ((Org.BouncyCastle.Ocsp.RevokedStatus)certificateStatus).RevocationReason;
                var revocationDate = ((Org.BouncyCastle.Ocsp.RevokedStatus)certificateStatus).RevocationTime;
                return CertStatus.Revoked(revocationDate.ToString("o"), revocationReason);
            }

            if (certificateStatus is Org.BouncyCastle.Ocsp.UnknownStatus)
                return CertStatus.Unknown();

            return CertStatus.Unknown();
        }

        /// <summary>
        /// Lookup and Parse AIA URLs. Normally it should include two URLs:
        /// 1. AIA OCSP
        /// 2. AIA Issuer 
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        AIA ParseAIA(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            try
            {
                var bc = (new Org.BouncyCastle.X509.X509CertificateParser()).ReadCertificate(cert.RawData);

                byte[] bytes = bc.GetExtensionValue(new DerObjectIdentifier(
                                Org.BouncyCastle.Asn1.X509.X509Extensions.AuthorityInfoAccess.Id)).GetOctets();

                if (bytes == null)
                    return null;

                Asn1InputStream aIn = new Asn1InputStream(bytes);
                Asn1Object obj = aIn.ReadObject();

                if (obj == null)
                    return null;

                Asn1Sequence s = (Asn1Sequence)obj;
                var elements = s.GetEnumerator();

                string ocspUrl = null, issuerUrl = null;
                while (elements.MoveNext())
                {
                    Asn1Sequence element = (Asn1Sequence)elements.Current;
                    DerObjectIdentifier oid = (DerObjectIdentifier)element[0];

                    if (oid.Id.Equals("1.3.6.1.5.5.7.48.1"))
                    {
                        var taggedObject = (Asn1TaggedObject)element[1];
                        ocspUrl = ExtractAIAUrl(taggedObject);
                    }
                    else if (oid.Id.Equals("1.3.6.1.5.5.7.48.2"))
                    {
                        var taggedObject = (Asn1TaggedObject)element[1];
                        issuerUrl = ExtractAIAUrl(taggedObject);
                    }
                }
                return new AIA()
                {
                    Issuer = issuerUrl,
                    Ocsp = ocspUrl
                };
            }
            catch (Exception ex)
            {
                //Log.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Lookup and validate certificate against CDP URL inside the certificate.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public CertStatus ValidateCertificateAgainstCRL(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            var urls = ParseCDPUrls(cert);
            if (urls == null || urls.Count == 0 || urls.Count > 1)
                return CertStatus.Unknown(CertStatus.NoCrl);

            var crl = LoadCrl(urls[0]);
            if (crl.NextUpdate.Value < DateTime.UtcNow)
                return CertStatus.Unknown(CertStatus.BadCrl);

            var serialNumber = new BigInteger(cert.SerialNumber, 16);

            var entry = crl.GetRevokedCertificate(serialNumber);

            if (entry == null)
                return CertStatus.Good;

            DerEnumerated reasonCode = null;
            try
            {
                reasonCode = DerEnumerated.GetInstance(entry.GetExtensionValue(X509Extensions.ReasonCode));
            }
            catch
            {
                return CertStatus.Unknown(CertStatus.BadRevocationReason);
            }

            int? revocationReason = null;
            if (reasonCode != null)
            {
                revocationReason = reasonCode.Value.SignValue;
            }
            else
            {
                revocationReason = CrlReason.Unspecified;
            }
            DateTime revocationDate = entry.RevocationDate;
            return CertStatus.Revoked(revocationDate.ToString("o"), revocationReason);
        }

        /// <summary>
        /// Lookup and parse CDP URL inside a certificate.
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        IList<string> ParseCDPUrls(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            var crls = new List<string>();
            if (cert.Extensions != null && cert.Extensions.Count > 0)
            {
                foreach (var ext in cert.Extensions)
                {
                    if (ext.Oid.Value == "2.5.29.31")
                    {
                        var o = Org.BouncyCastle.Asn1.Asn1Object.FromByteArray(ext.RawData);
                        var cdpListObj = Org.BouncyCastle.Asn1.X509.CrlDistPoint.GetInstance(o);
                        var cdpList = cdpListObj.GetDistributionPoints();
                        var generalNames = GeneralNames.GetInstance(cdpList[0].DistributionPointName.Name).GetNames();
                        for (int j = 0; j < generalNames.Length; j++)
                        {
                            if (generalNames[j].TagNo == GeneralName.UniformResourceIdentifier)
                            {
                                var url = ((DerIA5String)generalNames[j].Name).GetString();
                                crls.Add(url);
                            }
                        }
                        break;
                    }
                }
            }
            return crls;
        }

        /// <summary>
        /// Extract AIA URL. It can be AIA OCSP or AIA Issuer
        /// </summary>
        /// <param name="taggedObject"></param>
        /// <returns></returns>
        string ExtractAIAUrl(Asn1TaggedObject taggedObject)
        {
            var gn = (GeneralName)GeneralName.GetInstance(taggedObject);
            return ((DerIA5String)DerIA5String.GetInstance(gn.Name)).GetString();
        }

        /// <summary>
        /// Generate OCSP Request
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cert"></param>
        /// <returns></returns>
        byte[] GenerateOCSPRequest(Org.BouncyCastle.Ocsp.CertificateID id,
                                    Org.BouncyCastle.X509.X509Certificate cert)
        {
            byte[] nonce = new byte[16];
            Random rand = new Random();
            rand.NextBytes(nonce);

            //OCSP OID
            var asn1 = new DerOctetString(new DerOctetString(new byte[] { 1, 3, 6, 1, 5, 5, 7, 48, 1, 1 }));

            //Create OCSP Request
            var gen = new Org.BouncyCastle.Ocsp.OcspReqGenerator();
            gen.AddRequest(id);
            gen.SetRequestorName(new Org.BouncyCastle.Asn1.X509.GeneralName(
                                Org.BouncyCastle.Asn1.X509.GeneralName.DirectoryName, cert.SubjectDN));

            IList oids = new ArrayList();
            IList values = new ArrayList();

            oids.Add(Org.BouncyCastle.Asn1.Ocsp.OcspObjectIdentifiers.PkixOcspNonce);
            values.Add(new X509Extension(false,
                                        new Org.BouncyCastle.Asn1.DerOctetString(
                                                new Org.BouncyCastle.Asn1.DerOctetString(nonce))));

            oids.Add(Org.BouncyCastle.Asn1.Ocsp.OcspObjectIdentifiers.PkixOcsp);
            values.Add(new X509Extension(false, asn1));
            gen.SetRequestExtensions(new X509Extensions(oids, values));

            var req = gen.Generate();
            return req.GetEncoded();
        }

        /// <summary>
        /// Get OCSP Response
        /// </summary>
        /// <param name="ocspUrl"></param>
        /// <param name="reqEnc"></param>
        /// <returns></returns>
        byte[] GetOCSPResponse(string ocspUrl, byte[] reqEnc)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ocspUrl);
            request.Method = "POST";
            request.ContentType = "application/ocsp-request";
            request.ContentLength = reqEnc.Length;
            request.Accept = "application/ocsp-response";
            request.Timeout = 10000;
            var stream = request.GetRequestStream();
            stream.Write(reqEnc, 0, reqEnc.Length);
            stream.Close();
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            Stream respStream = response.GetResponseStream();
            byte[] resp = Stream2Bytes(respStream);
            respStream.Close();
            return resp;
        }

        /// <summary>
        /// Load CRL
        /// If the CRL does not expire, then load the cached one 
        /// otherwise download the CRL from the given URL
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        X509Crl LoadCrl(string uri)
        {
            try
            {
                string hash = ComputeSHA1(System.Text.ASCIIEncoding.ASCII.GetBytes(uri));
                string fileName = CrlCachedFolder + hash;
                if (System.IO.File.Exists(fileName))
                {
                    var crl = (new Org.BouncyCastle.X509.X509CrlParser()).ReadCrl(System.IO.File.ReadAllBytes(fileName));
                    if (crl.NextUpdate.Value > DateTime.UtcNow)
                        return crl;
                }
                Download(uri, fileName);
                return (new Org.BouncyCastle.X509.X509CrlParser()).ReadCrl(System.IO.File.ReadAllBytes(fileName));
            }
            catch
            {
                return null;
            }
        }

        bool IsIssuerCached(string url)
        {
            string hash = ComputeSHA1(System.Text.ASCIIEncoding.ASCII.GetBytes(url));
            return System.IO.File.Exists(IssuerCachedFolder + hash);
        }

        string CachedFolder
        {
            get
            {
                //return System.Configuration.ConfigurationManager.AppSettings["CachedFolder"];
                return _cacheFolder;
            }
        }

        string IssuerCachedFolder
        {
            get
            {
                string d = CachedFolder + "Issuers\\";
                if (!System.IO.Directory.Exists(d))
                {
                    System.IO.Directory.CreateDirectory(d);
                }
                return d;
            }
        }

        string CrlCachedFolder
        {
            get
            {
                string d = CachedFolder + "Crl\\";
                if (!System.IO.Directory.Exists(d))
                {
                    System.IO.Directory.CreateDirectory(d);
                }
                return d;
            }
        }

        /// <summary>
        /// Download a file from a given URL. This is to download Issuing CA Certificate.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="filePath"></param>
        static void Download(string url, string filePath)
        {
            try
            {
                using (var client = new WebClient())
                {
                    byte[] buf = client.DownloadData(url);
                    System.IO.File.WriteAllBytes(filePath, buf);

                }
            }
            catch
            {
            }
        }

        static string ComputeSHA1(byte[] data)
        {
            return BitConverter.ToString((new System.Security.Cryptography.SHA1Managed()).ComputeHash(
                                                data)).Replace("-", string.Empty).ToLower();
        }

        byte[] Stream2Bytes(Stream stream)
        {
            byte[] buffer = new byte[4096];
            MemoryStream ms = new MemoryStream();
            int read = 0;
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }
            return ms.ToArray();
        }

        /// <summary>
        /// Parse list of URIs serve as OneRecord IDs
        /// </summary>
        /// <param name="cert"></param>
        /// <returns></returns>
        public static IList<string> ParseOneRecordIDs(System.Security.Cryptography.X509Certificates.X509Certificate2 cert)
        {
            var bc = (new Org.BouncyCastle.X509.X509CertificateParser()).ReadCertificate(cert.RawData);
            var san = bc.GetSubjectAlternativeNames();
            var uriList = new List<string>();
            foreach (var s in san)
            {
                if (s is IList)
                {
                    var l = s as IList;
                    switch (l[0])
                    {
                        case Org.BouncyCastle.Asn1.X509.GeneralName.UniformResourceIdentifier:
                            uriList.Add(l[1].ToString());
                            break;
                        default:
                            break;
                    }
                }
            }
            return uriList;
        }
    }

    public class AIA
    {
        public string Issuer
        {
            get;
            set;
        }

        public string Ocsp
        {
            get;
            set;
        }
    }
}
