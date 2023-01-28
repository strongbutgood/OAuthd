using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcLoginResponse : SettingsBase
	{
		public OidcLoginResponse() : base() { }
		public OidcLoginResponse(IDictionary<string, object> settings) : base(settings) { }

		public string Scope
		{
			get => this.GetValueOrDefault("scope", default(string));
			set => this.SetOrRemove("scope", value);
		}
		public string AccessToken
		{
			get => this.GetValueOrDefault("access_token", default(string));
			set => this.SetOrRemove("access_token", value);
		}
		public string IdTokenJWT
		{
			get => this.GetValueOrDefault("id_token_jwt", default(string));
			set => this.SetOrRemove("id_token_jwt", value);
		}
		public OidcIdToken IdToken
		{
			get => this.GetValueOrDefault("id_token", default(OidcIdToken));
			set => this.SetOrRemove("id_token", value);
		}
		public long? ExpiresIn
		{
			get => this.GetValueOrDefault("expires_in", default(long?));
			set => this.SetOrRemove("expires_in", value);
		}
	}
}
