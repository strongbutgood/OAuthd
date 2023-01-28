using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcClientSettings : SettingsBase
	{
		public OidcClientSettings() : base() { }
		public OidcClientSettings(IDictionary<string, object> settings) : base(settings) { }

		public string Authority
		{
			get => this.GetValueOrDefault("authority", default(string));
			set => this["authority"] = value;
		}
		public string AuthorizationEndpoint
		{
			get => this.GetValueOrDefault("authorization_endpoint", default(string));
			set => this["authorization_endpoint"] = value;
		}
		public string ClientId
		{
			get => this.GetValueOrDefault("client_id", default(string));
			set => this["client_id"] = value;
		}
		public string ResponseType
		{
			get => this.GetValueOrDefault("response_type", default(string));
			set => this["response_type"] = value;
		}
		public string PostLogoutRedirectUri
		{
			get => this.GetValueOrDefault("post_logout_redirect_uri", default(string));
			set => this["post_logout_redirect_uri"] = value;
		}
		public bool? LoadUserProfile
		{
			get => this.GetValueOrDefault("load_user_profile", default(bool?));
			set => this["load_user_profile"] = value;
		}
		public OidcMetadata Metadata
		{
			get => this.GetValueOrDefault("metadata", default(OidcMetadata));
			set => this["metadata"] = value;
		}
		public JsonWebKeySets JWKS
		{
			get => this.GetValueOrDefault("jwks", default(JsonWebKeySets));
			set => this["jwks"] = value;
		}
	}
}
