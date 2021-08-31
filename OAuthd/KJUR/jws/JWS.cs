using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd.KJUR.jws
{
#pragma warning disable IDE1006 // Naming Styles
	class JWS
	{
		public class Parsed
		{
			public string headB64U { get; set; }
			public string headS { get; set; }
			public dynamic headP { get; set; }
			public string payloadB64U { get; set; }
			public string payloadS { get; set; }
			public string sigvalB64U { get; set; }
			public string si { get; set; }
			public string sigvalH { get; set; }
			public BigInteger sigvalBI { get; set; }
		}

		public Parsed parsedJWS;

		private string t(dynamic n)
		{
			var t = n.alg;
			var i = "";
			if (t != "RS256" && t != "RS512" && t != "PS256" && t != "PS512")
				throw new Exception("JWS signature algorithm not supported: " + t);
			if (t.Substring(2) == "256")
				i = "sha256";
			if (t.Substring(2) == "512")
				i = "sha512";
			return i;
		}

		public void parseJWS(string jwt, bool t)
		{
			string f;
			BigInteger o;
			string i, s;
			if (this.parsedJWS == null || !t && this.parsedJWS.sigvalH == null)
			{
				var match = System.Text.RegularExpressions.Regex.Match(jwt, "^([^.]+).([^.]+).([^.]+)$");
				if (!match.Success)
					throw new Exception("JWS signature is not a form of 'Head.Payload.SigValue'.");
				var head = match.Groups[1].Value;
				var payload = match.Groups[2].Value;
				var sigValue = match.Groups[3].Value;
				var hp = head + "." + payload;
				this.parsedJWS = new Parsed()
				{
					headB64U = head,
					payloadB64U = payload,
					sigvalB64U = sigValue,
					si = hp,
				};
				if (!t)
				{
					f = sigValue.Base64UrlToBase64().Base64ToHexString(); // b64utohex(sigValue);
					o = BigInteger.Parse(f, System.Globalization.NumberStyles.AllowHexSpecifier); // parseBigInt(f, 16);
					this.parsedJWS.sigvalH = f;
					this.parsedJWS.sigvalBI = o;
				}
				i = head.Base64UrlToUtf8(); // b64utoutf8(head);
				s = payload.Base64UrlToUtf8(); //b64utoutf8(payload);
				this.parsedJWS.headS = i;
				this.parsedJWS.payloadS = s;
				if (!KJUR.jws.JWS.isSafeJSONString(i, this.parsedJWS, "headP"))
					throw new Exception("malformed JSON string for JWS Head: " + i);
			}
		}

		public bool verifyJWSByNE(string n, object rsapars, int rsaKeyLength)
		{
			this.parseJWS(n, default);
			//return _rsasign_verifySignatureWithArgs(this.parsedJWS.si, this.parsedJWS.sigvalBI, rsapars, rsaKeyLength);
			return default;
		}
		public bool verifyJWSByKey(string n, dynamic i)
		{
			this.parseJWS(n, false);
			var r = t(this.parsedJWS.headP);
			var u = this.parsedJWS.headP.alg.Substring(0, 2) == "PS";
			if (i.hashAndVerify)
			{
				//return i.hashAndVerify(r, new Buffer(this.parsedJWS.si, "utf8").toString("base64"), b64utob64(this.parsedJWS.sigvalB64U), "base64", u);
				return i.hashAndVerify(r, this.parsedJWS.si.Utf8ToBase64Url().Base64UrlToBase64(), this.parsedJWS.sigvalB64U.Base64UrlToBase64(), "base64", u);
			}
			else
			{
				if (u)
					return i.verifyStringPSS(this.parsedJWS.si, this.parsedJWS.sigvalH, r);
				else
					return i.verifyString(this.parsedJWS.si, this.parsedJWS.sigvalH);
			}
		}
		public dynamic verifyJWSByPemX509Cert(string jwt, byte[] certBytes)
		{
			this.parseJWS(jwt, false);
			//var i = new X509;
			var i = new System.Security.Cryptography.X509Certificates.X509Certificate2(certBytes);
			var rsa = System.Security.Cryptography.X509Certificates.RSACertificateExtensions.GetRSAPublicKey(i);
			var result = rsa.VerifyData(
				Encoding.UTF8.GetBytes(this.parsedJWS.si),
				this.parsedJWS.sigvalH.HexStringToBase64().Base64ToByteArray(),
				HashAlgorithmName.SHA256,
				RSASignaturePadding.Pkcs1);
			return result;

			//i.readCertPEM(t);
			//return i.subjectPublicKeyRSA.verifyString(this.parsedJWS.si, this.parsedJWS.sigvalH)
		}

		public static bool isSafeJSONString(string n, object target, string propName)
		{
			try
			{
				var r = Newtonsoft.Json.Linq.JObject.Parse(n);
				if (r.Type != Newtonsoft.Json.Linq.JTokenType.Object)
				{
					return false;
				}
				else
				{
					if (r.Type == Newtonsoft.Json.Linq.JTokenType.Array)
						return false;
					else
					{
						if (target != null)
						{
							var prop = target.GetType().GetProperty(propName);
							if (prop != null)
								prop.SetValue(target, r);
							else
							{
								if (target is IDictionary<string, object> dict)
								{
									dict[propName] = r;
								}
							}
							//target[propName] = r;
						}
						return true;
					}
				}
			}
			catch
			{
				return false;
			}
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
