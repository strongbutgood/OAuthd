using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcIdToken : JsonWebToken
	{
		public override string Issuer
		{
			get => base.Issuer;
			set => base.Issuer = string.IsNullOrEmpty(value) ? throw new ArgumentException("Invalid issuer.", nameof(value)) : value;
		}
		public override string Subject
		{
			get => base.Subject;
			set => base.Subject = string.IsNullOrEmpty(value) ? throw new ArgumentException("Invalid subject.", nameof(value)) : value;
		}
		public override IList<string> Audience
		{
			get => base.Audience;
			set => base.Audience = (value == null || value.Count == 0) ? throw new ArgumentException("Invalid audience.", nameof(value)) : value;
		}
		public override long? ExpirationTime
		{
			get => base.ExpirationTime;
			set => base.ExpirationTime = value ?? throw new ArgumentException("Invalid expiration time.", nameof(value));
		}
		public override long? IssuedAt
		{
			get => base.IssuedAt;
			set => base.IssuedAt = value ?? throw new ArgumentException("Invalid issued at.", nameof(value));
		}

		/// <summary>Gets or sets a string value used to associate a Client session with an ID Token, and to mitigate replay attacks.</summary>
		public string Nonce
		{
			get => this._json["nonce"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("nonce");
				else
					this._json["nonce"] = value;
			}
		}

		/// <summary>Gets or sets the time when the End-User authentication occurred.</summary>
		public long? AuthenticationTime
		{
			get => this._json["auth_time"]?.ToObject<long>();
			set
			{
				if (!value.HasValue)
					this._json.Remove("auth_time");
				else
					this._json["auth_time"] = value.Value;
			}
		}
		public DateTime? AuthenticationDateTimeUtc
		{
			get => this.AuthenticationTime.FromEpoch();
			set => this.AuthenticationTime = value.ToEpoch();
		}

		/// <summary>Gets or sets a string specifying an Authentication Context Class Reference value that identifies the Authentication Context Class that the authentication performed satisfied.</summary>
		public string AuthenticationContextClassReference
		{
			get => this._json["acr"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("acr");
				else
					this._json["acr"] = value;
			}
		}

		/// <summary>Gets or sets a JSON array of strings that are identifiers for authentication methods used in the authentication.</summary>
		public IList<string> AuthenticationMethodsReferences
		{
			get
			{
				var claim = this._json["amr"];
				if (claim == null)
					return null;
				if (claim.Type == Newtonsoft.Json.Linq.JTokenType.Array)
					return claim.ToArray().Select(t => t.ToString()).ToArray();
				return new[] { claim.ToString() };
			}
			set
			{
				if (value == null || value.Count == 0)
					this._json.Remove("amr");
				else if (value.Count == 1)
					this._json["amr"] = value[0];
				else
					this._json["amr"] = new Newtonsoft.Json.Linq.JArray(value);
			}
		}

		/// <summary>Gets or sets the party to which the ID Token was issued.</summary>
		public string AuthorizedParty
		{
			get => this._json["azp"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("azp");
				else
					this._json["azp"] = value;
			}
		}

		/// <summary>Gets or sets the Access Token hash value.</summary>
		/// <remarks>
		/// Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the access_token value,
		/// where the hash algorithm used is the hash algorithm used in the alg Header Parameter of the ID Token's JOSE Header.
		/// For instance, if the alg is RS256, hash the access_token value with SHA-256, then take the left-most 128 bits and base64url encode them.
		/// The at_hash value is a case sensitive string.
		/// </remarks>
		public string AccessTokenHash
		{
			get => this._json["at_hash"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("at_hash");
				else
					this._json["at_hash"] = value;
			}
		}

		/// <summary>Gets or sets the Code hash value.</summary>
		/// <remarks>
		/// Its value is the base64url encoding of the left-most half of the hash of the octets of the ASCII representation of the code value,
		/// where the hash algorithm used is the hash algorithm used in the alg Header Parameter of the ID Token's JOSE Header.
		/// For instance, if the alg is HS512, hash the code value with SHA-512, then take the left-most 256 bits and base64url encode them.
		/// The c_hash value is a case sensitive string.
		/// If the ID Token is issued from the Authorization Endpoint with a code, which is the case for the response_type values code id_token and code id_token token,
		/// this is REQUIRED; otherwise, its inclusion is OPTIONAL.
		/// </remarks>
		public string CodeHash
		{
			get => this._json["c_hash"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("c_hash");
				else
					this._json["c_hash"] = value;
			}
		}

		/// <summary>Gets or sets the Public key used to check the signature of an ID Token issued by a Self-Issued OpenID Provider.</summary>
		public string SelfIssuedJWK
		{
			get => this._json["sub_jwk"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("sub_jwk");
				else
					this._json["sub_jwk"] = value;
			}
		}

		public static new OidcIdToken Parse(string token)
		{
			if (string.IsNullOrEmpty(token))
				throw new ArgumentNullException(nameof(token));
			return new OidcIdToken(Newtonsoft.Json.Linq.JObject.Parse(token));
		}

		public OidcIdToken(string issuer, string subject, string audience, DateTime expirationTime, DateTime issuedAt, IDictionary<string, object> parameters = null)
			: base(parameters)
		{
			this.Issuer = issuer;
			this.Subject = subject;
			this.Audience = new[] { audience };
			this.ExpirationDateTimeUtc = expirationTime;
			this.IssuedAtDateTimeUtc = issuedAt;
		}
		public OidcIdToken(Newtonsoft.Json.Linq.JObject json)
			: base(json)
		{
			if (string.IsNullOrEmpty(this.Issuer))
				throw new ArgumentException("Invalid issuer.", nameof(json));
			if (string.IsNullOrEmpty(this.Subject))
				throw new ArgumentException("Invalid subject.", nameof(json));
			if (this.Audience == null || this.Audience.Count == 0)
				throw new ArgumentException("Invalid audience.", nameof(json));
			if (!this.ExpirationTime.HasValue)
				throw new ArgumentException("Invalid expiration time.", nameof(json));
			if (!this.IssuedAt.HasValue)
				throw new ArgumentException("Invalid issued at.", nameof(json));
		}

		public void Validate(string nonce, string issuer, string audience)
		{
			if (nonce != this.Nonce)
				throw new Exception("Invalid nonce");

			if (issuer != this.Issuer)
				throw new Exception("Invalid issuer");

			if (!this.Audience.Contains(audience))
				throw new Exception("Invalid audience");

			var now = DateTime.UtcNow;

			// accept tokens issues up to 5 mins ago
			var diff = now - this.IssuedAtDateTimeUtc.GetValueOrDefault(DateTime.MinValue);
			if (diff.TotalMinutes > 5)
				throw new Exception("Token issued too long ago");

			if (this.ExpirationDateTimeUtc.GetValueOrDefault(DateTime.MinValue) < now)
				throw new Exception("Token expired");
		}
	}
}
