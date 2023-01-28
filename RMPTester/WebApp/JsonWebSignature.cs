using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	/// <summary>
	/// A JSON Web Signature as defined in https://www.rfc-editor.org/rfc/rfc7515
	/// </summary>
	class JsonWebSignature
	{
		private static readonly Regex _regex = new Regex("^(?<head>[^.]+).(?<payload>[^.]+).(?<signature>[^.]+)$", RegexOptions.Compiled);

		/// <summary>Gets or sets the header.</summary>
		public JsonWebSignatureHeader Header { get; set; }

		/// <summary>Gets or sets the payload.</summary>
		public JsonWebSignaturePayload Payload { get; set; }

		/// <summary>Gets or sets the signature.</summary>
		public JsonWebSignatureSignature Signature { get; set; }

		public static JsonWebSignature Parse(string token)
		{
			if (string.IsNullOrEmpty(token))
				throw new ArgumentNullException(nameof(token));
			var match = JsonWebSignature._regex.Match(token);
			if (!match.Success)
				throw new ArgumentException("Not a valid token format, expecting 'xxxx.yyyy.zzzz'.", nameof(token));
			return new JsonWebSignature(match.Groups["head"].Value, match.Groups["payload"].Value, match.Groups["signature"].Value);
		}

		public JsonWebSignature(string headerBase64Url, string payloadBase64Url, string signatureBase64Url)
			: this(JsonWebSignatureHeader.Parse(headerBase64Url), new JsonWebSignaturePayload(payloadBase64Url), new JsonWebSignatureSignature(signatureBase64Url))
		{
		}
		public JsonWebSignature(JsonWebSignatureHeader header, JsonWebSignaturePayload payload, JsonWebSignatureSignature signature)
		{
			this.Header = header;
			this.Payload = payload;
			this.Signature = signature;
		}

		public bool Verify(X509Certificate2 x509)
		{
			var rsa = RSACertificateExtensions.GetRSAPublicKey(x509);
			return rsa.VerifyData(
				Encoding.UTF8.GetBytes(this.GetSignedContent()),
				this.Signature.Bytes,
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pkcs1);
		}

		private string GetSignedContent() => $"{this.Header.Raw}.{this.Payload.Raw}";

		public override string ToString() => $"{this.Header.Raw}.{this.Payload.Raw}.{this.Signature.Raw}";
	}
}
