using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	/// <summary>
	/// A JSON Web Signature JOSE Header as defined in https://www.rfc-editor.org/rfc/rfc7515#section-4
	/// </summary>
	/// <remarks>
	/// For a JWS, the members of the JSON object(s) representing the JOSE Header describe the digital signature or MAC applied to the JWS
	/// Protected Header and the JWS Payload and optionally additional properties of the JWS.The Header Parameter names within the JOSE
	/// Header MUST be unique; JWS parsers MUST either reject JWSs with duplicate Header Parameter names or use a JSON parser that returns
	/// only the lexically last duplicate member name, as specified in Section 15.12 ("The JSON Object") of ECMAScript 5.1 [ECMAScript].
	///
	/// Implementations are required to understand the specific Header Parameters defined by this specification that are designated as "MUST
	/// be understood" and process them in the manner defined in this specification.All other Header Parameters defined by this
	/// specification that are not so designated MUST be ignored when not understood.  Unless listed as a critical Header Parameter, per
	/// Section 4.1.11, all Header Parameters not defined by this specification MUST be ignored when not understood.
	///
	/// There are three classes of Header Parameter names: Registered Header Parameter names, Public Header Parameter names, and Private Header
	/// Parameter names.
	/// </remarks>
	class JsonWebSignatureHeader : JsonWebSignaturePart
	{
		public override string Raw => this._json.ToString(Newtonsoft.Json.Formatting.None).Utf8ToBase64Url();

		/// <summary>Gets or sets the "alg" Algorithm header.</summary>
		/// <remarks>
		/// The "alg" (algorithm) Header Parameter identifies the cryptographic algorithm used to secure the JWS.The JWS Signature value is not
		/// valid if the "alg" value does not represent a supported algorithm or if there is not a key for use with that algorithm associated with the
		/// party that digitally signed or MACed the content.  "alg" values should either be registered in the IANA "JSON Web Signature and
		/// Encryption Algorithms" registry established by [JWA] or be a value that contains a Collision-Resistant Name.  The "alg" value is a case-
		/// sensitive ASCII string containing a StringOrURI value.This Header Parameter MUST be present and MUST be understood and processed by implementations.
		///
		/// A list of defined "alg" values for this use can be found in the IANA "JSON Web Signature and Encryption Algorithms" registry established
		/// by[JWA]; the initial contents of this registry are the values defined in Section 3.1 of[JWA].
		/// </remarks>
		public string Algorithm
		{
			get => this._json["alg"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					throw new ArgumentNullException(nameof(value));
				this._json["alg"] = value;
			}
		}

		public string JWKSetUrl
		{
			get => this._json["jku"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("jku");
				else
					this._json["jku"] = value;
			}
		}

		public string JsonWebKey
		{
			get => this._json["jwk"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("jwk");
				else
					this._json["jwk"] = value;
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

		public string X509CertificateChain
		{
			get => this._json["x5c"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("x5c");
				else
					this._json["x5c"] = value;
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

		/// <summary>Gets or sets the 'typ' Type header.</summary>
		/// <remarks>
		/// The "typ" (type) Header Parameter is used by JWS applications to declare the media type[IANA.MediaTypes] of this complete JWS.
		/// This is intended for use by the application when more than one kind of object could be present in an application data structure that can
		/// contain a JWS; the application can use this value to disambiguate among the different kinds of objects that might be present.
		/// It will typically not be used by applications when the kind of object is already known.
		/// This parameter is ignored by JWS implementations; any processing of this parameter is performed by the JWS application.
		/// Use of this Header Parameter is OPTIONAL.
		///
		/// Per RFC 2045 [RFC2045], all media type values, subtype values, and parameter names are case insensitive.
		/// However, parameter values are case sensitive unless otherwise specified for the specific parameter.
		///
		/// To keep messages compact in common situations, it is RECOMMENDED that producers omit an "application/" prefix of a media type value in a
		/// "typ" Header Parameter when no other '/' appears in the media type value.
		/// A recipient using the media type value MUST treat it as if "application/" were prepended to any "typ" value not containing a '/'.
		/// For instance, a "typ" value of "example" SHOULD be used to represent the "application/example" media type, whereas the media type
		/// "application/example;part="1/2"" cannot be shortened to "example;part="1/2"".
		///
		/// The "typ" value "JOSE" can be used by applications to indicate that this object is a JWS or JWE using the JWS Compact Serialization or
		/// the JWE Compact Serialization.The "typ" value "JOSE+JSON" can be used by applications to indicate that this object is a JWS or JWE
		/// using the JWS JSON Serialization or the JWE JSON Serialization. Other type values can also be used by applications.
		/// </remarks>
		public string Type
		{
			get
			{
				var type = this._json["typ"]?.ToString();
				if (type == null)
					return null;
				if (type.Contains('/'))
					return type;
				return $"{ApplicationMediaTypePrefix}{type}";
			}
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("typ");
				else if (value.StartsWith(ApplicationMediaTypePrefix))
					this._json["typ"] = value.Substring(ApplicationMediaTypePrefix.Length);
				else
					this._json["typ"] = value;
			}
		}
		private const string ApplicationMediaTypePrefix = "application/";

		public string ContentType
		{
			get => this._json["cty"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("cty");
				else
					this._json["cty"] = value;
			}
		}

		public IList<string> Critical
		{
			get => this._json["crit"]?.ToArray().Select(t => t.ToString()).ToList().AsReadOnly();
			set
			{
				if (value == null || value.Count == 0)
					this._json.Remove("crit");
				else
					this._json["crit"] = new Newtonsoft.Json.Linq.JArray(value);
			}
		}

		private readonly Newtonsoft.Json.Linq.JObject _json;

		public static JsonWebSignatureHeader Parse(string base64url)
		{
			var json = base64url.Base64UrlToUtf8();
			return new JsonWebSignatureHeader(Newtonsoft.Json.Linq.JObject.Parse(json));
		}

		/// <summary>Creates a new JWS header with the specified <paramref name="algorithm"/> and <paramref name="parameters"/>.</summary>
		/// <param name="algorithm">The algorithm used to secure the JWS or 'none' if unsecured.</param>
		/// <param name="parameters"></param>
		public JsonWebSignatureHeader(string algorithm, IDictionary<string, object> parameters = null)
		{
			if (string.IsNullOrEmpty(algorithm))
				throw new ArgumentNullException(nameof(algorithm));
			this._json = new Newtonsoft.Json.Linq.JObject();
			if (parameters != null)
			{
				foreach (var kvp in parameters)
				{
					this._json[kvp.Key] = Newtonsoft.Json.Linq.JToken.FromObject(kvp.Value);
				}
			}
			this.Algorithm = algorithm;
		}
		/// <summary>Creates a new JWS header from the JSON object.</summary>
		/// <param name="json">The header object.</param>
		public JsonWebSignatureHeader(Newtonsoft.Json.Linq.JObject json)
		{
			if (json == null)
				throw new ArgumentNullException(nameof(json));
			if (!json.ContainsKey("alg"))
				throw new ArgumentException("Missing the required 'alg' algorithm parameter.", nameof(json));
			this._json = json;
		}

		public override string ToString() => this._json.ToString(Newtonsoft.Json.Formatting.None);
	}
}
