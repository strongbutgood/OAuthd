using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class Token
	{
		public readonly string id_token;
		public readonly string id_token_jwt;
		public readonly string access_token;
		public readonly long expires_at;
		public readonly IList<string> scopes;

		public bool expired => this.expires_at < (JSBuiltIns.Date_now() / 1000);
		public long expires_in => this.expires_at - (JSBuiltIns.Date_now() / 1000);

		public Token(string id_token, string id_token_jwt, string access_token, long expires_at, string scope)
		{
			this.id_token = id_token;
			this.id_token_jwt = id_token_jwt;
			this.access_token = access_token;
			if (access_token != null)
			{
				this.expires_at = Convert.ToInt64(expires_at);
			}
			else if (id_token != null)
			{
				this.expires_at = Newtonsoft.Json.Linq.JObject.Parse(id_token).Value<long>("exp");
			}
			else
			{
				throw new ArgumentException("Either access_token or id_token required.");
			}

			this.scopes = new ReadOnlyCollection<string>((scope ?? string.Empty).Split(new string[] { " " }, StringSplitOptions.None));
		}

		public static Token fromResponse(object response)
		{
			if (response is string strResponse)
				response = Newtonsoft.Json.Linq.JObject.Parse(strResponse);
			if (response is Newtonsoft.Json.Linq.JObject jResponse)
			{
				long expires_at = default;
				var response_access_token = jResponse["access_token"].ToObject<string>();
				if (response_access_token != null)
				{
					var now = Convert.ToInt64(JSBuiltIns.Date_now() / 1000);
					var response_expires_in = jResponse["expires_in"].ToObject<string>();
					expires_at = now + Convert.ToInt64(response_expires_in);
				}
				var response_id_token = jResponse["expires_in"].ToObject<string>();
				var response_id_token_jwt = jResponse["id_token_jwt"].ToObject<string>();
				var response_scope = jResponse["scope"].ToObject<string>();
				return new Token(response_id_token, response_id_token_jwt, response_access_token, expires_at, response_scope);
			}
			return new Token(null, null, null, 0, null);
		}

		public static Token fromJSON(string json)
		{
			if (json != null)
			{
				try
				{
					var obj = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, new { id_token = "", id_token_jwt = "0", access_token = "", expires_at = 0, scope = "" });
					return new Token(obj.id_token, obj.id_token_jwt, obj.access_token, obj.expires_at, obj.scope);
				}
				catch (Exception)
				{
				}
			}
			return new Token(null, null, null, 0, null);
		}

		public string toJSON()
		{
			return Newtonsoft.Json.JsonConvert.SerializeObject(new
			{
				id_token = this.id_token,
				id_token_jwt = this.id_token_jwt,
				access_token = this.access_token,
				expires_at = this.expires_at,
				scope = string.Join(" ", this.scopes)
			});
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
