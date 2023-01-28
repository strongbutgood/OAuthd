using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcAuthorizeResponse : SettingsBase
	{
		private static readonly Regex _queryParameterRegex = new Regex("(?<key>[^&=]+)=(?<value>[^&]*)", RegexOptions.Compiled);

		public OidcAuthorizeResponse() : base() { }
		public OidcAuthorizeResponse(IDictionary<string, object> settings) : base(settings) { }

		public string AccessToken
		{
			get => this.GetValueOrDefault("access_token", default(string));
			set => this.SetOrRemove("access_token", value);
		}
		public string IdToken
		{
			get => this.GetValueOrDefault("id_token", default(string));
			set => this.SetOrRemove("id_token", value);
		}
		public string TokenType
		{
			get => this.GetValueOrDefault("token_type", default(string));
			set => this.SetOrRemove("token_type", value);
		}
		public string RefreshToken
		{
			get => this.GetValueOrDefault("refresh_token", default(string));
			set => this.SetOrRemove("refresh_token", value);
		}
		public long? ExpiresIn
		{
			get => this.GetValueOrDefault("expires_in", default(long?));
			set => this.SetOrRemove("expires_in", value);
		}
		public string Error
		{
			get => this.GetValueOrDefault("error", default(string));
			set => this.SetOrRemove("error", value);
		}
		public string State
		{
			get => this.GetValueOrDefault("state", default(string));
			set => this.SetOrRemove("state", value);
		}

		public static OidcAuthorizeResponse FromQueryString(string queryString)
		{
			if (queryString == null)
				return default;

			var idx = queryString.LastIndexOf("#");
			if (idx >= 0)
				queryString = queryString.Substring(idx + 1);

			var response = new OidcAuthorizeResponse();

			var counter = 0;
			var pos = 0;
			Match match;
			while ((match = _queryParameterRegex.Match(queryString, pos)).Success)
			{
				var key = Uri.UnescapeDataString(match.Groups["key"].Value);
				var value = Uri.UnescapeDataString(match.Groups["value"].Value);
				response[key] = value;
				if (counter++ > 50)
				{
					response.Clear();
					response["error"] = "Response exceeded expected number of parameters";
					return response;
				}
				pos = match.Index + match.Length;
			}
			// do special typing for non-string
			if (response.TryGetValue("expires_in", out var expObj) &&
				long.TryParse((string)expObj, out var expValue))
				response.ExpiresIn = expValue;

			if (response.Any())
				return response;
			return default;
		}
	}
}
