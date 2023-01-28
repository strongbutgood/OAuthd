using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class TokenManagerSettings : SettingsBase
	{
		public TokenManagerSettings() : base() { }
		public TokenManagerSettings(IDictionary<string, object> settings) : base(settings) { }

		public bool Persist => this.GetValueOrDefault("persist", default(bool));
		public IDictionary<string, object> Store => this.GetValueOrDefault("store", default(IDictionary<string, object>));
		public string PersistKey => this.GetValueOrDefault("persistKey", default(string));

		public string SilentRedirectUri => this.GetValueOrDefault("silent_redirect_uri", default(string));
		public bool SilentRenew => this.GetValueOrDefault("silent_renew", default(bool));
	}
}
