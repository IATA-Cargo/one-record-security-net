using System;
using System.Collections;
using System.Collections.Generic;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                                                @"C:\Users\Administrator\Downloads\companyX.crt");
            var bc = (new Org.BouncyCastle.X509.X509CertificateParser()).ReadCertificate(cert.RawData);
            var san = bc.GetSubjectAlternativeNames();
            var uriList = new List<string>();
            foreach(var s in san)
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
            Console.WriteLine("Hello World!");
        }
    }
}
