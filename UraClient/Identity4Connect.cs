namespace UraClient
{
    using RestSharp;
    using System;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using UraClient.Model;

    public class Identity4Connect
    {

        public string GetSKI(string url, out string error)
        {
            error = string.Empty;
            var result = string.Empty;

            try
            {
                var client = new RestClient(url);
                var request = new RestRequest(string.Empty, Method.GET);
                var queryResult = client.Execute<JwksObjectList>(request).Data;
                
                if (queryResult?.keys == null)
                {
                    error = "Error getdata";
                    return result;
                }

                var first = queryResult.keys.Where(i => i.use == "sig").LastOrDefault();
                if (first == null)
                {
                    error = "Error getdata";
                    return result;
                }

                var n = first.n;
                var e = first.e;
                if (string.IsNullOrEmpty(n))
                {
                    error = "Error getdata - n empty";
                    return result;
                }

                if (string.IsNullOrEmpty(e))
                {
                    error = "Error getdata - e empty";
                    return result;
                }

                RSACryptoServiceProvider csp = new RSACryptoServiceProvider();
                csp.ImportParameters(
                  new RSAParameters()
                  {
                      Modulus = FromBase64Url(n),
                      Exponent = FromBase64Url(e),
                  });

                var parameters = csp.ExportParameters(false);
                var m = parameters.Modulus;

                byte[] head = convertByte("3082010A0282010100");
                byte[] tail = convertByte("0203010001");

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
                    result = (rstLast + "").Replace("-", "").ToLower();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
            }

            return result;
        }

        #region Private functions

        private byte[] convertByte(string valu)
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

        #endregion

    }
}
