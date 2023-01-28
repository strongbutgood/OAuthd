using OAuthd.KJUR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class OidcClient
	{
		public const string requestDataKey = "OidcClient.requestDataKey";
		private readonly Settings _settings;

		public bool isOidc => this._settings.response_type?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Any((item) => item == "id_token") ?? false;

		public bool isOAuth => this._settings.response_type?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Any((item) => item == "token") ?? false;

		public OidcClient(SettingsBase settings)
		{
			if (settings != null)
				this._settings = new Settings(settings);
			else
				this._settings = new Settings();

			this._settings.Add(new Dictionary<string, object>()
			{
				[nameof(Settings.load_user_profile)] = true,
				[nameof(Settings.response_type)] = "id_token token",
				[nameof(Settings.store)] = Host.Default.LocalStorage,
			});

			if (this._settings.authority != null && this._settings.authority.IndexOf(".well-known/openid-configuration") < 0)
			{
				if (this._settings.authority[this._settings.authority.Length - 1] != '/')
				{
					this._settings.authority += '/';
				}
				this._settings.authority += ".well-known/openid-configuration";
			}
		}

		public async void redirectForToken()
		{
			try
			{
				var request = await this.createTokenRequestAsync();
				Host.Default.location = request.url;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			}
		}

		public async void redirectForLogout(string id_token_hint)
		{
			var settings = this._settings;
			try
			{
				var metadata = await this.loadMetadataAsync();
				if (metadata.end_session_endpoint == null)
				{
					Console.WriteLine("No end_session_endpoint in metadata");
				}
				var url = metadata.end_session_endpoint;
				if (id_token_hint != null && settings.post_logout_redirect_uri != null)
				{
					url += "?post_logout_redirect_uri=" + settings.post_logout_redirect_uri;
					url += "&id_token_hint=" + id_token_hint;
				}
				Host.Default.location = url;
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
			};
		}

		public async Task<string> loadAuthorizationEndpoint()
		{
			if (this._settings.authorization_endpoint != null)
				return this._settings.authorization_endpoint;

			if (this._settings.authority == null)
				throw new Exception("No authorization_endpoint configured");

			var metadata = await this.loadMetadataAsync();
			if (metadata.authorization_endpoint == null)
			{
				throw new Exception("Metadata does not contain authorization_endpoint");
			}

			return metadata.authorization_endpoint;
		}

		private static readonly string[] __required = new string[] { "client_id", "redirect_uri", "response_type", "scope" };
		private static readonly string[] __optional = new string[] { "prompt", "display", "max_age", "ui_locales", "id_token_hint", "login_hint", "acr_values" };
		public async Task<TokenRequestDTO> createTokenRequestAsync()
		{
			var settings = this._settings;

			var authorization_endpoint = await this.loadAuthorizationEndpoint();
			var state = rand();
			string nonce = null;

			var url = authorization_endpoint + "?state=" + Uri.EscapeDataString(state);

			if (this.isOidc)
			{
				nonce = rand();
				url += "&nonce=" + Uri.EscapeDataString(nonce);
			}

			foreach (var key in OidcClient.__required)
			{
				if (settings.TryGetValue(key, out var value) && value != null)
				{
					url += "&" + key + "=" + Uri.EscapeDataString(value.ToString());
				}
			}

			foreach (var key in OidcClient.__optional)
			{
				if (settings.TryGetValue(key, out var value) && value != null)
				{
					url += "&" + key + "=" + Uri.EscapeDataString(value.ToString());
				}
			}

			//var data = new Newtonsoft.Json.Linq.JObject(new {
			//	oidc = this.isOidc,
			//	oauth = this.isOAuth,
			//	state = state
	   		//});
			var data = new TokenRequestDataDTO()
			{
				oidc = this.isOidc,
				oauth = this.isOAuth,
				state = state,
			};

			if (nonce != null)
			{
				//data["nonce"] = nonce;
				data.nonce = nonce;
			}

			settings.store.setItem(requestDataKey, data.ToString());

			//return new Newtonsoft.Json.Linq.JObject(new {
			//	data = data,
			//	url = url
			//});
			return new TokenRequestDTO()
			{
				data = data,
				url = url,
			};
		}

		public class TokenRequestDTO
		{
			public TokenRequestDataDTO data { get; set; }
			public string url { get; set; }
		}
		public class TokenRequestDataDTO
		{
			public bool oidc { get; set; }
			public bool oauth { get; set; }
			public string state { get; set; }
			public string nonce { get; set; }

			public override string ToString()
			{
				if (this.nonce != null)
					return Newtonsoft.Json.Linq.JObject.FromObject(this).ToString();
				return Newtonsoft.Json.Linq.JObject.FromObject(new { this.oidc, this.oauth, this.state }).ToString();
			}
		}

		public async Task<SettingsMetadata> loadMetadataAsync()
		{
			var settings = this._settings;

			if (settings.metadata != null)
			{
				return settings.metadata;
			}

			if (settings.authority == null)
			{
				throw new Exception("No authority configured");
			}

			try
			{
				var json = await getJson(settings.authority, token: null);
				if (!(json is Newtonsoft.Json.Linq.JObject jsonObj))
					throw new NotImplementedException();
				var metadata = new SettingsMetadata(jsonObj.Properties().ToDictionary(p => p.Name, p => (object)p.Value.ToString()));
				settings.metadata = metadata;
				return metadata;
			}
			catch (Exception err)
			{
				throw new Exception("Failed to load metadata (" + err.Message + ")", err);
			}
		}

		public async Task<Newtonsoft.Json.Linq.JToken> loadX509SigningKeyAsync()
		{
			var settings = this._settings;

			async Task<Newtonsoft.Json.Linq.JToken> getKeyAsync(Newtonsoft.Json.Linq.JObject jwks)
			{
				await Task.Yield();
				if (!(jwks["keys"] is Newtonsoft.Json.Linq.JArray keys) || keys.Count == 0)
				{
					throw new Exception("Signing keys empty");
				}

				var key = keys[0];
				if (key["kty"].ToString() != "RSA")
				{
					throw new Exception("Signing key not RSA");
				}

				if (!(key["x5c"] is Newtonsoft.Json.Linq.JArray x5c) || x5c.Count == 0)
				{
					throw new Exception("RSA keys empty");
				}

				return x5c[0];
			}

			if (settings.jwks != null)
			{
				return await getKeyAsync(settings.jwks);
			}

			var metadata = await this.loadMetadataAsync();
			if (metadata.jwks_uri == null)
			{
				throw new Exception("Metadata does not contain jwks_uri");
			}

			try
			{
				var jwks = await getJson(metadata.jwks_uri, token: null) as Newtonsoft.Json.Linq.JObject;

				settings.jwks = jwks;
				return await getKeyAsync(jwks);
			}
			catch (Exception err)
			{
				throw new Exception("Failed to load signing keys (" + err.Message + ")");
			}
		}

		public async Task<object> validateIdTokenAsync(object jwt, string nonce, string access_token)
		{
			var settings = this._settings;

			var cert = await this.loadX509SigningKeyAsync();

			
			var jws = new KJUR.jws.JWS();
			if (jws.verifyJWSByPemX509Cert(jwt.ToString(), KJUR.StringExtensions.Base64ToByteArray(cert.ToObject<string>())))
			{
				var id_token = Newtonsoft.Json.Linq.JObject.Parse(jws.parsedJWS.payloadS);

				var id_token_nonce = id_token["nonce"]?.ToObject<string>();
				if (nonce != id_token_nonce)
				{
					throw new Exception("Invalid nonce");
				}

				var metadata = await this.loadMetadataAsync();
				var id_token_iss = id_token["iss"]?.ToObject<string>();
				if (id_token_iss != metadata.issuer)
				{
					throw new Exception("Invalid issuer");
				}

				var id_token_aud = id_token["aud"]?.ToObject<string>();
				if (id_token_aud != settings.client_id)
				{
					throw new Exception("Invalid audience");
				}

				var now = Convert.ToInt64(JSBuiltIns.Date_now() / 1000);

				// accept tokens issues up to 5 mins ago
				var id_token_iat = id_token["iat"]?.ToObject<long>();
				var diff = now - id_token_iat.GetValueOrDefault(0);
				if (diff > (5 * 60))
				{
					throw new Exception("Token issued too long ago");
				}

				var id_token_exp = id_token["exp"]?.ToObject<long>();
				if (id_token_exp.GetValueOrDefault(0) < now)
				{
					throw new Exception("Token expired");
				}

				if (access_token != null && settings.load_user_profile)
				{
					// if we have an access token, then call user info endpoint
					var id_token2 = await this.loadUserProfile(access_token, id_token);
					return id_token2;
				}
				else
				{
					// no access token, so we have all our claims
					return id_token;
				}
			}
			else
			{
				throw new Exception("JWT failed to validate");
			}
		}

		public async Task validateAccessTokenAsync(object id_token, string access_token)
		{
			await Task.Yield();

			var id_token_at_hash = (id_token as Newtonsoft.Json.Linq.JObject)?["at_hash"]?.ToObject<string>();
			if (id_token_at_hash == null)
			{
				throw new Exception("No at_hash in id_token");
			}

			var sha = System.Security.Cryptography.SHA256Cng.Create();
			var has2 = sha.ComputeHash(Encoding.UTF8.GetBytes(access_token));
			var lef2a = has2.ToHexString();
			var lef2 = lef2a.Substring(0, lef2a.Length / 2);

			var hash = KJUR.crypto.Util.sha256(access_token);
			var left = hash.Substring(0, hash.Length / 2);
			var left_b64u = left.HexStringToBase64().Base64ToBase64Url(); // hextob64u(left);

			if (left_b64u != id_token_at_hash)
			{
				throw new Exception("at_hash failed to validate");
			}

		}

		public async Task<Newtonsoft.Json.Linq.JObject> loadUserProfile(string access_token, Newtonsoft.Json.Linq.JObject id_token)
		{
			var metadata = await this.loadMetadataAsync();

			if (metadata.userinfo_endpoint == null)
			{
				throw new Exception("Metadata does not contain userinfo_endpoint");
			}

			var response = await getJson(metadata.userinfo_endpoint, access_token);

			if (id_token is Newtonsoft.Json.Linq.JObject jid &&
				response is Newtonsoft.Json.Linq.JObject jresp)
			{
				foreach (var prop in jresp.Properties())
				{
					jid[prop.Name] = prop.Value;
				}
			}
			return id_token;
		}

		public async Task<object> validateIdTokenAndAccessTokenAsync(object id_token_jwt, string nonce, string access_token)
		{
			var id_token = await this.validateIdTokenAsync(id_token_jwt, nonce, access_token);
			await this.validateAccessTokenAsync(id_token, access_token);

			return id_token;
		}

		public async Task<Newtonsoft.Json.Linq.JObject> readResponseAsync(string queryString)
		{
			var settings = this._settings;

			var requestData = settings.store.getItem(requestDataKey);
			var data = Newtonsoft.Json.Linq.JObject.Parse(requestData.ToString());
			settings.store.removeItem(requestDataKey);

			if (data == null)
			{
				throw new Exception("No request state loaded");
			}

			//data = JSON.parse(data);
			if (data == null)
			{
				throw new Exception("No request state loaded");
			}

			var data_state = data["state"]?.ToObject<string>();
			if (data_state == null)
			{
				throw new Exception("No state loaded");
			}

			var result = parseOidcResult(queryString);
			if (result == null)
			{
				throw new Exception("No OIDC response");
			}

			var result_error = result["error"]?.ToObject<string>();
			if (result_error != null)
			{
				throw new Exception(result_error);
			}

			var result_state = result["state"]?.ToObject<string>();
			if (result_state != data_state)
			{
				throw new Exception("Invalid state");
			}

			var data_nonce = data["nonce"]?.ToObject<string>();
			var data_oidc = data["oidc"]?.ToObject<bool>();
			var result_id_token = result["id_token"];
			if (data_oidc == true)
			{
				if (result_id_token == null)
				{
					throw new Exception("No identity token");
				}

				if (data_nonce == null)
				{
					throw new Exception("No nonce loaded");
				}
			}

			var data_oauth = data["oauth"]?.ToObject<bool>();
			var result_access_token = result["access_token"]?.ToObject<string>();
			var result_token_type = result["token_type"]?.ToObject<string>();
			var result_expires_in = result["expires_in"]?.ToObject<long>();
			if (data_oauth == true)
			{
				if (result_access_token == null)
				{
					throw new Exception("No access token");
				}

				if (result_token_type != "Bearer")
				{
					throw new Exception("Invalid token type");
				}

				if (result_expires_in == 0)
				{
					throw new Exception("No token expiration");
				}
			}

			Task<object> promise = Task.FromResult(default(object)); //_promiseFactory.resolve();
			if (data_oidc == true && data_oauth == true)
			{
				promise = this.validateIdTokenAndAccessTokenAsync(result_id_token, data_nonce, result_access_token);
			}
			else if (data_oidc == true)
			{
				promise = this.validateIdTokenAsync(result_id_token, data_nonce, null);
			}

			var id_token = await promise;
			return Newtonsoft.Json.Linq.JObject.FromObject(new
			{
				id_token = id_token,
				id_token_jwt = result_id_token,
				access_token = result_access_token,
				expires_in = result_expires_in,
				scope = result["scope"]
			});
		}

		private static readonly Random __random = new Random();
		internal static string rand() {
			return ((JSBuiltIns.Date_now() + __random.NextDouble()) * __random.NextDouble()).ToString("0").Replace(".", "");
		}

		internal static async Task<Newtonsoft.Json.Linq.JToken> getJson(string url, string token)
		{
			var config = new Newtonsoft.Json.Linq.JObject();
			if (token != null)
			{
				config["headers"] = new Newtonsoft.Json.Linq.JObject(new { Authorization = "Bearer " + token });
			}
			return await getJson(url, config);
		}
		internal static async Task<Newtonsoft.Json.Linq.JToken> getJson(string url, Newtonsoft.Json.Linq.JObject config)
		{
			var handler = new System.Net.Http.HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback += (HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors) =>
			{
				return true;
			};
			var httpClient = new System.Net.Http.HttpClient(handler, true);
			if (config != null && config["headers"] is Newtonsoft.Json.Linq.JObject headers)
			{
				foreach (var property in headers.Properties())
				{
					httpClient.DefaultRequestHeaders.Add(property.Name, property.Value.ToString());
				}
			}

			System.Net.Http.HttpResponseMessage response;
			try
			{
				response = await httpClient.GetAsync(url);
				await Host.Default.FileLogger.StoreRequestResponseAsync(response, handler);
			}
			catch (Exception err)
			{
				throw new Exception("Network error", err);
			}
			if (response.StatusCode != System.Net.HttpStatusCode.OK)
				throw new Exception($"{response.ReasonPhrase}({(int)response.StatusCode})");
			try
			{
				var content = await response.Content.ReadAsStringAsync();
				return Newtonsoft.Json.Linq.JToken.Parse(content);
			}
			catch (Exception)
			{
				throw;
			}
		}

		internal static Newtonsoft.Json.Linq.JObject parseOidcResult(string queryString)
		{
			if (queryString == null)
				queryString = Host.Default.location.hash;

			var idx = queryString.LastIndexOf("#");
			if (idx >= 0)
			{
				queryString = queryString.Substring(idx + 1);
			}

			var parameters = new Newtonsoft.Json.Linq.JObject();
			var regex = new System.Text.RegularExpressions.Regex("([^&=]+)=([^&]*)");
			System.Text.RegularExpressions.Match match;

			var counter = 0;
			var pos = 0;
			while ((match = regex.Match(queryString, pos)).Success)
			{
				var key = Uri.UnescapeDataString(match.Groups[1].Value);
				var value = Uri.UnescapeDataString(match.Groups[2].Value);
				parameters[key] = value;
				if (counter++ > 50)
				{
					return new Newtonsoft.Json.Linq.JObject(new {
						error = "Response exceeded expected number of parameters"
					});
				}
				pos = match.Index + match.Length;
			}

			foreach (var prop in parameters.Properties()) 
			{
				return parameters;
			}
			return default;
		}

		private class Settings : SettingsBase
		{
			public Settings() : base() { }
			public Settings(IDictionary<string, object> settings) : base(settings) { }

			public bool load_user_profile => this.GetValueOrDefault(nameof(load_user_profile), default(bool));
			public string response_type => this.GetValueOrDefault(nameof(response_type), default(string));
			public HostStorage store => this.GetValueOrDefault(nameof(store), default(HostStorage));

			public string authority
			{
				get => this.GetValueOrDefault(nameof(authority), default(string));
				set => this[nameof(authority)] = value;
			}
			public string authorization_endpoint => this.GetValueOrDefault(nameof(authorization_endpoint), default(string));
			public string client_id => this.GetValueOrDefault(nameof(client_id), default(string));
			public string post_logout_redirect_uri => this.GetValueOrDefault(nameof(post_logout_redirect_uri), default(string));
			public SettingsMetadata metadata
			{
				get => this.GetValueOrDefault(nameof(metadata), default(SettingsMetadata));
				set => this[nameof(metadata)] = value;
			}
			public Newtonsoft.Json.Linq.JObject jwks
			{
				get => this.GetValueOrDefault(nameof(jwks), default(Newtonsoft.Json.Linq.JObject));
				set => this[nameof(jwks)] = value;
			}
		}
		public class SettingsMetadata : SettingsBase
		{
			public SettingsMetadata() : base() { }
			public SettingsMetadata(IDictionary<string, object> settings) : base(settings) { }

			public string authorization_endpoint => this.GetValueOrDefault(nameof(authorization_endpoint), default(string));
			public string end_session_endpoint => this.GetValueOrDefault(nameof(end_session_endpoint), default(string));
			public string userinfo_endpoint => this.GetValueOrDefault(nameof(userinfo_endpoint), default(string));
			public string jwks_uri => this.GetValueOrDefault(nameof(jwks_uri), default(string));
			public string issuer => this.GetValueOrDefault(nameof(issuer), default(string));
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
