using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcMetadata : SettingsBase
	{
		public OidcMetadata() : base() { }
		public OidcMetadata(IDictionary<string, object> settings) : base(settings) { }

		public string AuthorizationEndpoint => this.GetValueOrDefault("authorization_endpoint", default(string));
		public string EndSessionEndpoint => this.GetValueOrDefault("end_session_endpoint", default(string));
		public string UserinfoEndpoint => this.GetValueOrDefault("userinfo_endpoint", default(string));
		public string JWKSUri => this.GetValueOrDefault("jwks_uri", default(string));
		public string Issuer => this.GetValueOrDefault("issuer", default(string));
	}
}
