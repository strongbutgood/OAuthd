using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcAuthorizeRequest : SettingsBase
	{
		public OidcAuthorizeRequest() : base() { }
		public OidcAuthorizeRequest(IDictionary<string, object> settings) : base(settings) { }

		public bool IsOIDC
		{
			get => this.GetValueOrDefault("oidc", default(bool));
			set => this["oidc"] = value;
		}
		public bool IsOAuth
		{
			get => this.GetValueOrDefault("oauth", default(bool));
			set => this["oauth"] = value;
		}
		public string State
		{
			get => this.GetValueOrDefault("state", default(string));
			set => this["state"] = value;
		}
		public string Nonce
		{
			get => this.GetValueOrDefault("nonce", default(string));
			set => this["nonce"] = value;
		}

		public string ToJson()
		{
			var sb = new StringBuilder();
			using (var jsonWriter = new Newtonsoft.Json.JsonTextWriter(new System.IO.StringWriter(sb)))
			{
				jsonWriter.WriteStartObject();
				foreach (var kvp in this)
				{
					jsonWriter.WritePropertyName(kvp.Key);
					jsonWriter.WriteValue(kvp.Value);
				}
				jsonWriter.WriteEndObject();
				jsonWriter.Flush();
			}
			return sb.ToString();
		}

		public override string ToString() => base.ToString();
	}
}
