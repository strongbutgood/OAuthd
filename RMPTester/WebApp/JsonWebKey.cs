using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	/// <summary>
	/// A JSON Web Key (JWK) is a JavaScript Object Notation (JSON) [RFC7159] data structure that represents a cryptographic key as defined in https://www.rfc-editor.org/rfc/rfc7517.
	/// </summary>
	class JsonWebKey : JsonObjectBase<Newtonsoft.Json.Linq.JObject>
	{
		/// <summary>Gets or sets "kty" Key Type parameter.</summary>
		/// <remarks>
		/// The "kty" (key type) parameter identifies the cryptographic algorithm family used with the key, such as "RSA" or "EC".
		/// "kty" values should either be registered in the IANA "JSON Web Key Types" registry established by[JWA] or be a value that contains a Collision-Resistant Name.
		/// The "kty" value is a case-sensitive string.
		/// This member MUST be present in a JWK.
		/// 
		/// A list of defined "kty" values can be found in the IANA "JSON Web Key Types" registry established by [JWA];
		/// the initial contents of this registry are the values defined in Section 6.1 of[JWA].
		/// 
		/// The key type definitions include specification of the members to be used for those key types.
		/// Members used with specific "kty" values can be found in the IANA "JSON Web Key Parameters" registry established by Section 8.1.
		/// </remarks>
		public string KeyType
		{
			get => this._json["kty"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException(nameof(value));
				this._json["kty"] = value;
			}
		}

		public string PublicKeyUse
		{
			get => this._json["use"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("use");
				else
					this._json["use"] = value;
			}
		}
		public JsonWebKeyPublicKeyUse? PublicKeyUseFlag
		{
			get
			{
				if (this.PublicKeyUse == "sig")
					return JsonWebKeyPublicKeyUse.Signature;
				if (this.PublicKeyUse == "enc")
					return JsonWebKeyPublicKeyUse.Encryption;
				return default;
			}
			set
			{
				if (value == null)
					this._json.Remove("use");
				if (value == JsonWebKeyPublicKeyUse.Signature)
					this.PublicKeyUse = "sig";
				if (value == JsonWebKeyPublicKeyUse.Encryption)
					this.PublicKeyUse = "enc";
			}
		}

		public string KeyOperations
		{
			get => this._json["key_ops"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("key_ops");
				else
					this._json["key_ops"] = value;
			}
		}
		public JsonWebKeyKeyOperations? KeyOperationsFlag
		{
			get => this.KeyOperations != null ? Enum.TryParse<JsonWebKeyKeyOperations>(this.KeyOperations, true, out var keyOperations) ? keyOperations : default : default;
			set
			{
				if (value == null)
					this._json.Remove("key_ops");
				else
					this.KeyOperations = value.Value.ToString();
			}
		}

		public string Algorithm
		{
			get => this._json["alg"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("alg");
				else
					this._json["alg"] = value;
			}
		}

		public string KeyId
		{
			get => this._json["kid"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("kid");
				else
					this._json["kid"] = value;
			}
		}

		public string X509Url
		{
			get => this._json["x5u"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("x5u");
				else
					this._json["x5u"] = value;
			}
		}

		public IList<string> X509CertificateChain
		{
			get => this._json["x5c"]?.ToArray().Select(t => t.ToString()).ToArray();
			set
			{
				if (value == null || value.Count == 0)
					this._json.Remove("x5c");
				else
					this._json["x5c"] = new Newtonsoft.Json.Linq.JArray(value);
			}
		}

		public string X509CertificateSHA1Thumbprint
		{
			get => this._json["x5t"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("x5t");
				else
					this._json["x5t"] = value;
			}
		}

		public string X509CertificateSHA256Thumbprint
		{
			get => this._json["x5t#256"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("x5t#256");
				else
					this._json["x5t#256"] = value;
			}
		}


		public static JsonWebKey Parse(string json)
		{
			if (string.IsNullOrEmpty(json))
				throw new ArgumentNullException(nameof(json));
			return new JsonWebKey(Newtonsoft.Json.Linq.JObject.Parse(json));
		}

		protected JsonWebKey(string keyType, IDictionary<string, object> parameters = null)
			: base(new Newtonsoft.Json.Linq.JObject())
		{
			if (string.IsNullOrEmpty(keyType))
				throw new ArgumentNullException(nameof(keyType));
			if (parameters != null)
			{
				foreach (var kvp in parameters)
				{
					this._json[kvp.Key] = Newtonsoft.Json.Linq.JToken.FromObject(kvp.Value);
				}
			}
			this.KeyType = keyType;
		}
		public JsonWebKey(Newtonsoft.Json.Linq.JObject json)
			: base(json)
		{
			if (!json.ContainsKey("kty"))
				throw new ArgumentException("Missing the required 'kty' key type parameter.", nameof(json));
		}

		public override string ToString() => this._json.ToString(Newtonsoft.Json.Formatting.Indented);
	}

	public enum JsonWebKeyPublicKeyUse
	{
		Signature,
		Encryption
	}
	public enum JsonWebKeyKeyOperations
	{
		Sign,
		Verify,
		Encrypt,
		Decrypt,
		WrapKey,
		UnwrapKey,
		DeriveKey,
		DeriveBits,
	}
}
