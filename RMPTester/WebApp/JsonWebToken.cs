using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonWebToken : JsonObjectBase<Newtonsoft.Json.Linq.JObject>
	{
		/// <summary>Gets or sets the 'iss' Issuer claim.</summary>
		/// <remarks>
		/// The "iss" (issuer) claim identifies the principal that issued the JWT.
		/// The processing of this claim is generally application specific.
		/// The "iss" value is a case-sensitive string containing a StringOrURI value.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual string Issuer
		{
			get => this._json["iss"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("iss");
				else
					this._json["iss"] = value;
			}
		}

		/// <summary>Gets or sets the 'sub' Subject claim.</summary>
		/// <remarks>
		/// The "sub" (subject) claim identifies the principal that is the subject of the JWT.
		/// The claims in a JWT are normally statements about the subject.
		/// The subject value MUST either be scoped to be locally unique in the context of the issuer or be globally unique.
		/// The processing of this claim is generally application specific.
		/// The "sub" value is a case-sensitive string containing a StringOrURI value.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual string Subject
		{
			get => this._json["sub"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("sub");
				else
					this._json["sub"] = value;
			}
		}

		/// <summary>Gets or sets the 'aud' Audience claim.</summary>
		/// <remarks>
		/// The "aud" (audience) claim identifies the recipients that the JWT is intended for.
		/// Each principal intended to process the JWT MUST identify itself with a value in the audience claim.
		/// If the principal processing the claim does not identify itself with a value in the "aud" claim when this claim is present,
		/// then the JWT MUST be rejected.
		/// In the general case, the "aud" value is an array of case-sensitive strings, each containing a StringOrURI value.
		/// In the special case when the JWT has one audience, the "aud" value MAY be a single case-sensitive string containing a StringOrURI value.
		/// The interpretation of audience values is generally application specific.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual IList<string> Audience
		{
			get
			{
				var claim = this._json["aud"];
				if (claim == null)
					return null;
				if (claim.Type == Newtonsoft.Json.Linq.JTokenType.Array)
					return claim.ToArray().Select(t => t.ToString()).ToArray();
				return new[] { claim.ToString() };
			}
			set
			{
				if (value == null || value.Count == 0)
					this._json.Remove("aud");
				else if (value.Count == 1)
					this._json["aud"] = value[0];
				else
					this._json["aud"] = new Newtonsoft.Json.Linq.JArray(value);
			}
		}

		/// <summary>Gets or sets the 'exp' Expiration Time claim.</summary>
		/// <remarks>
		/// The "exp" (expiration time) claim identifies the expiration time on or after which the JWT MUST NOT be accepted for processing.
		/// The processing of the "exp" claim requires that the current date/time MUST be before the expiration date/time listed in the "exp" claim.
		/// Implementers MAY provide for some small leeway, usually no more than a few minutes, to account for clock skew.
		/// Its value MUST be a number containing a NumericDate value.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual long? ExpirationTime
		{
			get => this._json["exp"]?.ToObject<long>();
			set
			{
				if (!value.HasValue)
					this._json.Remove("exp");
				else
					this._json["exp"] = value.Value;
			}
		}
		public DateTime? ExpirationDateTimeUtc
		{
			get => this.ExpirationTime.FromEpoch();
			set => this.ExpirationTime = value.ToEpoch();
		}

		/// <summary>Gets or sets the 'nbf' Not Before claim.</summary>
		/// <remarks>
		/// The "nbf" (not before) claim identifies the time before which the JWT MUST NOT be accepted for processing.
		/// The processing of the "nbf" claim requires that the current date/time MUST be after or equal to the not-before date/time listed in the "nbf" claim.
		/// Implementers MAY provide for some small leeway, usually no more than a few minutes, to account for clock skew.
		/// Its value MUST be a number containing a NumericDate value.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual long? NotBefore
		{
			get => this._json["nbf"]?.ToObject<long>();
			set
			{
				if (value == null)
					this._json.Remove("nbf");
				else
					this._json["nbf"] = value.Value;
			}
		}
		public DateTime? NotBeforeDateTimeUtc
		{
			get => this.NotBefore.FromEpoch();
			set => this.NotBefore = value.ToEpoch();
		}

		/// <summary>Gets or sets the 'iat' Issued At claim.</summary>
		/// <remarks>
		/// The "iat" (issued at) claim identifies the time at which the JWT was issued.
		/// This claim can be used to determine the age of the JWT.
		/// Its value MUST be a number containing a NumericDate value.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual long? IssuedAt
		{
			get => this._json["iat"]?.ToObject<long>();
			set
			{
				if (value == null)
					this._json.Remove("iat");
				else
					this._json["iat"] = value.Value;
			}
		}
		public DateTime? IssuedAtDateTimeUtc
		{
			get => this.IssuedAt.FromEpoch();
			set => this.IssuedAt = value.ToEpoch();
		}

		/// <summary>Gets or sets the 'jti' JWT ID claim.</summary>
		/// <remarks>
		/// The "jti" (JWT ID) claim provides a unique identifier for the JWT.
		/// The identifier value MUST be assigned in a manner that ensures that there is a negligible probability that
		/// the same value will be accidentally assigned to a different data object; if the application uses multiple issuers,
		/// collisions MUST be prevented among values produced by different issuers as well.
		/// The "jti" claim can be used to prevent the JWT from being replayed.
		/// The "jti" value is a case-sensitive string.
		/// Use of this claim is OPTIONAL.
		/// </remarks>
		public virtual string JwtId
		{
			get => this._json["jti"]?.ToString();
			set
			{
				if (string.IsNullOrEmpty(value))
					this._json.Remove("jti");
				else
					this._json["jti"] = value;
			}
		}

		public static JsonWebToken Parse(string token)
		{
			if (string.IsNullOrEmpty(token))
				throw new ArgumentNullException(nameof(token));
			return new JsonWebToken(Newtonsoft.Json.Linq.JObject.Parse(token));
		}

		protected JsonWebToken(IDictionary<string, object> parameters = null)
			: base(new Newtonsoft.Json.Linq.JObject())
		{
			if (parameters != null)
			{
				foreach (var kvp in parameters)
				{
					this._json[kvp.Key] = Newtonsoft.Json.Linq.JToken.FromObject(kvp.Value);
				}
			}
		}
		public JsonWebToken(Newtonsoft.Json.Linq.JObject json)
			: base(json)
		{
		}

		public override string ToString() => this._json.ToString(Newtonsoft.Json.Formatting.Indented);
	}
}
