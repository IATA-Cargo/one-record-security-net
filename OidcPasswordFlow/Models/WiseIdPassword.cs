namespace OidcPasswordFlow.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
	using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class WiseIdPassword
    {
		private const string STRING_EMPTY = "";

		public string grant_type { get; set; } = "password";
		public string scope { get; set; } = "openid profile email";
		public string username { get; set; } = STRING_EMPTY;
		public string thumbprint { get; set; } = STRING_EMPTY;
		public string password { get; set; } = STRING_EMPTY;
		public string client_id { get; set; } = STRING_EMPTY;
		public string client_secret { get; set; } = STRING_EMPTY;

		/// <summary>
		/// Intialize class and parse base data (thumbprint, password)
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="pass"></param>
		/// <param name="appId"></param>
		/// <param name="appKey"></param>
		/// <param name="pathCrt"></param>
		public WiseIdPassword(string userId, string pass, string appId, string appKey, string pathCrt)
		{

			this.username = userId;
			this.client_id = appId;
			this.client_secret = appKey;

			var cert = getCertificate(pathCrt);
			if (cert == null) return;
			
			this.thumbprint = cert.Thumbprint;

			var passArr = Encoding.UTF8.GetBytes(pass);
			byte[] passBytes = Encrypt(cert, passArr);
			this.password = BitConverter.ToString(passBytes).Replace("-", "");
		}

		/// <summary>
		/// Build json data string from this object
		/// </summary>
		/// <returns></returns>
		public string ToJson()
		{
			try
			{
				return JsonConvert.SerializeObject(this);
			}
			catch (Exception)
			{
				return STRING_EMPTY;
			}
		}

		/// <summary>
		/// Build form data string from this object
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> BuildFormData()
		{
			var result = new Dictionary<string, string>
			{
				{ "username", username + "_" + thumbprint },
				{ "password", password },
				{ "grant_type", grant_type },
				{ "scope", scope },
				{ "client_id", client_id },
				{ "client_secret", client_secret }
			};

			return result;
		}

		/// <summary>
		/// Get certificate from file
		/// </summary>
		/// <param name="resourceFilePath"></param>
		/// <returns></returns>
		private X509Certificate2 getCertificate(string resourceFilePath)
		{
			try
			{
				return new X509Certificate2(File.ReadAllBytes(resourceFilePath));
			} catch (Exception){
				return null;
			}
		}

		/// <summary>
		/// Encrypt data with certificate ( only public key )
		/// </summary>
		/// <param name="certificate"></param>
		/// <param name="inputData"></param>
		/// <returns></returns>
		private byte[] Encrypt(X509Certificate2 certificate, byte[] inputData)
		{
			try
			{
				using RSA rsa = certificate.GetRSAPublicKey();
				return rsa.Encrypt(inputData, RSAEncryptionPadding.Pkcs1);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message, ex.StackTrace);
			}
			return null;
		}

	}
}
