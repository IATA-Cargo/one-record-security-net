namespace UraClient
{
    using RestSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    

    public class OIDCUtil
    {
        RSAParameters _rsa;
        string _ski;
        public RSAParameters RSAParameters
        {
            get
            {
                return _rsa;
            }
        }
        public string SKI
        {
            get
            {
                return _ski;
            }
        }

        public OIDCUtil(string url)
        {
            LoadAndParse(url);
        }
        void LoadAndParse(string url)
        {
            var client      = new RestClient(url);
            var request     = new RestRequest(string.Empty, Method.GET);
            var queryResult = client.Execute<JwksObjectList>(request).Data;
            if (queryResult?.keys == null)
            {
                throw new UnauthorizedAccessException("Mailform IDToken.");
            }

            var first = queryResult.keys.Where(i => i.use == "sig").LastOrDefault();
            if (first == null)
            {
                throw new UnauthorizedAccessException("Mailform IDToken: sig not found.");
            }

            var n = first.n;
            var e = first.e;
            if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(e))
            {
                throw new UnauthorizedAccessException("Unexpected signature: Only SHA256RSA signature is valid.");
            }

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
            _rsa = new RSAParameters()
            {
                Modulus = FromBase64Url(n),
                Exponent = FromBase64Url(e),
            };

            csp.ImportParameters(_rsa);

            var parameters = csp.ExportParameters(false);
            var m = parameters.Modulus;

            byte[] head = ConvertByte("3082010A0282010100");
            byte[] tail = ConvertByte("0203010001");

            var length = head.Length + m.Length + tail.Length;
            var byteArray = new byte[length];

            Array.Copy(head, 0, byteArray, 0, head.Length);
            Array.Copy(m, 0, byteArray, head.Length, m.Length);
            Array.Copy(tail, 0, byteArray, head.Length + m.Length, tail.Length);
            //var bufferM = BitConverter.ToString(byteArray).Replace("-", " ");
                
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var sha1Result = sha1.ComputeHash(byteArray);
                var rstLast = BitConverter.ToString(sha1Result);
                _ski = (rstLast + "").Replace("-", "").ToLower();
            }
        }

        #region Private functions

        private byte[] ConvertByte(string valu)
        {
            return Enumerable.Range(0, valu.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(valu.Substring(x, 2), 16))
                     .ToArray();
        }

        private void EncodeLength(BinaryWriter stream, int length)
        {
            if (length < 0) throw new ArgumentOutOfRangeException("length", "Length must be non-negative");
            if (length < 0x80)
            {
                // Short form
                stream.Write((byte)length);
            }
            else
            {
                // Long form
                var temp = length;
                var bytesRequired = 0;
                while (temp > 0)
                {
                    temp >>= 8;
                    bytesRequired++;
                }
                stream.Write((byte)(bytesRequired | 0x80));
                for (var i = bytesRequired - 1; i >= 0; i--)
                {
                    stream.Write((byte)(length >> (8 * i) & 0xff));
                }
            }
        }

        private void EncodeIntegerBigEndian(BinaryWriter stream, byte[] value, bool forceUnsigned = true)
        {
            stream.Write((byte)0x02); // INTEGER
            var prefixZeros = 0;
            for (var i = 0; i < value.Length; i++)
            {
                if (value[i] != 0) break;
                prefixZeros++;
            }
            if (value.Length - prefixZeros == 0)
            {
                EncodeLength(stream, 1);
                stream.Write((byte)0);
            }
            else
            {
                if (forceUnsigned && value[prefixZeros] > 0x7f)
                {
                    // Add a prefix zero to force unsigned if the MSB is 1
                    EncodeLength(stream, value.Length - prefixZeros + 1);
                    stream.Write((byte)0);
                }
                else
                {
                    EncodeLength(stream, value.Length - prefixZeros);
                }
                for (var i = prefixZeros; i < value.Length; i++)
                {
                    stream.Write(value[i]);
                }
            }
        }

        private byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0 ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/").Replace("-", "+");
            return Convert.FromBase64String(base64);
        }

        internal class JwksObjectList
        {
            public List<JwksObject> keys { get; set; }
        }

        internal class JwksObject
        {
            public string kty { get; set; }
            public string use { get; set; }
            public string kid { get; set; }
            public string xSt { get; set; }
            public string e { get; set; }
            public string n { get; set; }
            public List<string> x5c { get; set; }
            public string alg { get; set; }

        }

        #endregion

    }
}
