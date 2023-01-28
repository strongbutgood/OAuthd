using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class Token : SettingsBase
	{
		public bool Expired => this.ExpiresAt < DateTime.UtcNow.ToEpoch();
		public long? ExpiresIn => this.ExpiresAt - DateTime.UtcNow.ToEpoch();


		public Token() : base() { }
		public Token(IDictionary<string, object> settings)
			: base(settings)
		{
			if (!string.IsNullOrEmpty(this.AccessToken))
			{
				var expires_in = this.GetValueOrDefault("expires_in", default(long?));
				if (expires_in == null)
					throw new ArgumentException("access_token is missing the expires_in time.", nameof(settings));
				var now = DateTime.UtcNow;
				this.ExpiresAt = now.ToEpoch() + expires_in.Value;
			}
			else if (this.IdToken != null)
			{
				this.ExpiresAt = this.IdToken.ExpirationTime;
			}
			else
			{
				throw new ArgumentException("Either access_token or id_token required.");
			}
		}

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
		public long? ExpiresAt
		{
			get => this.GetValueOrDefault("expires_at", default(long?));
			set => this.SetOrRemove("expires_at", value);
		}
	}
}
