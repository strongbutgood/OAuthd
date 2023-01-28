using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class OidcClient
	{
		private static readonly string[] __required = new string[] { "client_id", "redirect_uri", "response_type", "scope" };
		private static readonly string[] __optional = new string[] { "prompt", "display", "max_age", "ui_locales", "id_token_hint", "login_hint", "acr_values" };
		private static readonly Random __random = new Random();
		internal static string rand() => (((long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalMilliseconds + __random.NextDouble()) * __random.NextDouble()).ToString("0").Replace(".", "");

		private static OidcAuthorizeRequest _requestStore;

		public bool IsOIDC => this.Settings.ResponseType?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Any((item) => item == "id_token") ?? false;
		public bool IsOAuth => this.Settings.ResponseType?.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries).Any((item) => item == "token") ?? false;

		public OidcClientSettings Settings { get; set; }

		public OidcClient(IDictionary<string, object> settings = null)
		{
			if (settings != null)
				this.Settings = new OidcClientSettings(settings);
			else
				this.Settings = new OidcClientSettings();

			this.Settings.Add(new OidcClientSettings()
			{
				LoadUserProfile = true,
				ResponseType = "id_token token",
			});

			if (this.Settings.Authority != null && this.Settings.Authority.IndexOf(".well-known/openid-configuration") < 0)
			{
				if (this.Settings.Authority[this.Settings.Authority.Length - 1] != '/')
				{
					this.Settings.Authority += '/';
				}
				this.Settings.Authority += ".well-known/openid-configuration";
			}
		}

		private async Task<string> LoadAuthorizationEndpointAsync()
		{
			if (this.Settings.AuthorizationEndpoint != null)
				return this.Settings.AuthorizationEndpoint;

			if (this.Settings.Authority == null)
				throw new Exception("No authorization_endpoint configured");

			var metadata = await this.LoadMetadataAsync();
			if (metadata.AuthorizationEndpoint == null)
				throw new Exception("Metadata does not contain authorization_endpoint");

			return metadata.AuthorizationEndpoint;
		}

		private async Task<OidcMetadata> LoadMetadataAsync()
		{
			if (this.Settings.Metadata != null)
				return this.Settings.Metadata;

			if (this.Settings.Authority == null)
				throw new Exception("No authority configured");

			try
			{
				var metadata = await GetAsync<OidcMetadata>(this.Settings.Authority, token: null);
				this.Settings.Metadata = metadata;
				return metadata;
			}
			catch (Exception err)
			{
				throw new Exception("Failed to load metadata (" + err.Message + ")", err);
			}
		}

		public async Task CreateTokenRequestAsync(Func<string, OidcAuthorizeRequest, Task> navigate)
		{
			var authorization_endpoint = await this.LoadAuthorizationEndpointAsync();
			var state = rand();
			string nonce = null;

			var url = authorization_endpoint + "?state=" + Uri.EscapeDataString(state);

			if (this.IsOIDC)
			{
				nonce = rand();
				url += "&nonce=" + Uri.EscapeDataString(nonce);
			}

			foreach (var key in OidcClient.__required)
			{
				if (this.Settings.TryGetValue(key, out var value) && value != null)
				{
					url += "&" + key + "=" + Uri.EscapeDataString(value.ToString());
				}
				else
				{
					throw new InvalidOperationException($"Missing required '{key}' parameter.");
				}
			}

			foreach (var key in OidcClient.__optional)
			{
				if (this.Settings.TryGetValue(key, out var value) && value != null)
				{
					url += "&" + key + "=" + Uri.EscapeDataString(value.ToString());
				}
			}

			var data = new OidcAuthorizeRequest()
			{
				IsOIDC = this.IsOIDC,
				IsOAuth = this.IsOAuth,
				State = state,
			};

			if (nonce != null)
			{
				data.Nonce = nonce;
			}

			OidcClient._requestStore = data;

			await navigate(url, data);
		}

		public async Task<OidcLoginResponse> ReadResponseAsync(OidcAuthorizeRequest data, string queryString)
		{
			//var data = OidcClient._requestStore;
			//OidcClient._requestStore = null;

			if (data == null)
				throw new Exception("No request state loaded");

			var data_state = data.State;
			if (data_state == null)
				throw new Exception("No state loaded");

			var result = OidcAuthorizeResponse.FromQueryString(queryString);
			if (result == null)
				throw new Exception("No OIDC response");

			var result_error = result.Error;
			if (result_error != null)
				throw new Exception(result_error);

			if (result.State != data_state)
				throw new Exception("Invalid state");

			var result_id_token = result.IdToken;
			if (data.IsOIDC)
			{
				if (result_id_token == null)
					throw new Exception("No identity token");

				if (data.Nonce == null)
					throw new Exception("No nonce loaded");
			}

			var result_access_token = result.AccessToken;
			var result_expires_in = result.ExpiresIn;
			if (data.IsOAuth)
			{
				if (result_access_token == null)
					throw new Exception("No access token");

				if (result.TokenType != "Bearer")
					throw new Exception("Invalid token type");

				if (result_expires_in.GetValueOrDefault(0) == 0)
					throw new Exception("No token expiration");
			}

			Task<OidcIdToken> promise = Task.FromResult(default(OidcIdToken)); //_promiseFactory.resolve();
			if (data.IsOIDC && data.IsOAuth)
			{
				promise = this.ValidateIdTokenAndAccessTokenAsync(result_id_token, data.Nonce, result_access_token);
			}
			else if (data.IsOIDC)
			{
				promise = this.ValidateIdTokenAsync(result_id_token, data.Nonce, null);
			}

			var id_token = await promise;
			return new OidcLoginResponse(new Dictionary<string, object>()
			{
				["id_token"] = id_token,
				["id_token_jwt"] = result_id_token,
				["access_token"] = result_access_token,
				["expires_in"] = result_expires_in,
				["scope"] = result["scope"],
			});
		}

		public async Task RedirectForToken(Func<string, OidcAuthorizeRequest, Task> navigate)
		{
			try
			{
				await this.CreateTokenRequestAsync(navigate);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				throw;
			}
		}

		public async Task RedirectForLogout(string id_token_hint, Func<string, Task> navigate)
		{
			try
			{
				var metadata = await this.LoadMetadataAsync();
				if (metadata.EndSessionEndpoint == null)
				{
					Console.WriteLine("No end_session_endpoint in metadata");
				}
				var url = metadata.EndSessionEndpoint;
				if (id_token_hint != null && this.Settings.PostLogoutRedirectUri != null)
				{
					url += "?post_logout_redirect_uri=" + this.Settings.PostLogoutRedirectUri;
					url += "&id_token_hint=" + id_token_hint;
				}
				await navigate(url);
			}
			catch (Exception err)
			{
				Console.WriteLine(err);
				throw;
			};
		}


		public async Task<string> LoadX509SigningKeyAsync()
		{
			if (this.Settings.JWKS == null)
			{

				var metadata = await this.LoadMetadataAsync();
				if (metadata.JWKSUri == null)
					throw new Exception("Metadata does not contain jwks_uri");

				try
				{
					this.Settings.JWKS = await GetAsync(metadata.JWKSUri, token: null, deserializer: JsonWebKeySets.Parse);
				}
				catch (Exception err)
				{
					throw new Exception("Failed to load signing keys (" + err.Message + ")");
				}
			}
			try
			{
				var jwks = this.Settings.JWKS;
				if (jwks.Keys == null || jwks.Keys.Count == 0)
					throw new Exception("Signing keys empty");

				var key = jwks.Keys[0];
				if (key.KeyType != "RSA")
					throw new Exception("Signing key not RSA");

				if (key.X509CertificateChain == null || key.X509CertificateChain.Count == 0)
					throw new Exception("RSA keys empty");

				return key.X509CertificateChain[0];
			}
			catch (Exception err)
			{
				throw new Exception("Failed to load signing keys (" + err.Message + ")", err);
			}
		}

		public async Task<OidcIdToken> ValidateIdTokenAsync(string token, string nonce, string access_token)
		{
			var cert = await this.LoadX509SigningKeyAsync();

			var jws = JsonWebSignature.Parse(token);
			if (jws.Verify(new X509Certificate2(cert.Base64ToByteArray())))
			{
				var id_token = OidcIdToken.Parse(jws.Payload.PlainText);
				var metadata = await this.LoadMetadataAsync();
				id_token.Validate(nonce, metadata.Issuer, this.Settings.ClientId);

				if (access_token != null && this.Settings.LoadUserProfile == true)
				{
					// if we have an access token, then call user info endpoint
					return await this.LoadUserProfileAsync(access_token, id_token);
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

		public async Task ValidateAccessTokenAsync(OidcIdToken id_token, string access_token)
		{
			await Task.Yield();

			var id_token_at_hash = id_token.AccessTokenHash;
			if (id_token_at_hash == null)
				throw new Exception("No at_hash in id_token");

			var sha = System.Security.Cryptography.SHA256Cng.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(access_token));
			var lefta = hash.ToHexString();
			var left = lefta.Substring(0, lefta.Length / 2);

			var left_b64u = left.HexStringToBase64().Base64ToBase64Url(); // hextob64u(left);

			if (left_b64u != id_token_at_hash)
				throw new Exception("at_hash failed to validate");
		}

		public async Task<OidcIdToken> LoadUserProfileAsync(string access_token, OidcIdToken id_token)
		{
			var metadata = await this.LoadMetadataAsync();

			if (metadata.UserinfoEndpoint == null)
				throw new Exception("Metadata does not contain userinfo_endpoint");

			var response = await GetAsync(metadata.UserinfoEndpoint, access_token, deserializer: Newtonsoft.Json.Linq.JObject.Parse);

			if (response != null)
			{
				foreach (var prop in response.Properties())
				{
					id_token[prop.Name] = prop.Value;
				}
			}
			return id_token;
		}

		public async Task<OidcIdToken> ValidateIdTokenAndAccessTokenAsync(string id_token_jwt, string nonce, string access_token)
		{
			var id_token = await this.ValidateIdTokenAsync(id_token_jwt, nonce, access_token);
			await this.ValidateAccessTokenAsync(id_token, access_token);

			return id_token;
		}



		private static bool CertificateValidationCallback(HttpRequestMessage request, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			// If the certificate is a valid, signed certificate, return true.
			if (sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}
			if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors)
				return true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
			{
				if (chain != null && chain.ChainStatus != null)
				{
					foreach (X509ChainStatus status in chain.ChainStatus)
					{
						if ((certificate.Subject == certificate.Issuer) &&
						   (status.Status == X509ChainStatusFlags.UntrustedRoot))
						{
							// Self-signed certificates with an untrusted root are valid. 
							continue;
						}
						else
						{
							if (status.Status != X509ChainStatusFlags.NoError)
							{
								// If there are any other errors in the certificate chain, the certificate is invalid,
								// so the method returns false.
								return false;
							}
						}
					}
				}

				// When processing reaches this line, the only errors in the certificate chain are 
				// untrusted root errors for self-signed certificates. These certificates are valid
				// for default Exchange server installations, so return true.
				return true;
			}


			/* overcome localhost and 127.0.0.1 issue */
			if ((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
			{
				if (certificate.Subject.Contains("localhost"))
				{
					if (request.RequestUri.Host.Contains("127.0.0.1"))
					{
						return true;
					}
				}
			}

			return false;
		}


		internal static async Task<T> GetAsync<T>(string url, string token, Func<string, T> deserializer = null)
		{
			var config = token != null ?
				new Dictionary<string, object>()
				{
					["headers"] = new GetRequestHeaders()
					{
						Authorization = $"Bearer {token}",
					},
				} : null;
			return await GetAsync<T>(url, config, deserializer);
		}
		internal static async Task<T> GetAsync<T>(string url, IDictionary<string, object> config, Func<string, T> deserializer = null)
		{
			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback += CertificateValidationCallback;
			var httpClient = new HttpClient(handler, true);
			if (config != null && config.TryGetValue("headers", out var headers))
			{
				OidcClient.SetRequestHeaders(httpClient, headers);
			}

			HttpResponseMessage response;
			try
			{
				response = await httpClient.GetAsync(url);
				//await Host.Default.FileLogger.StoreRequestResponseAsync(response, handler);
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
				if (deserializer != null)
				{
					return deserializer(content);
				}
				else
				{
					var serializer = Newtonsoft.Json.JsonSerializer.Create(new Newtonsoft.Json.JsonSerializerSettings() { });
					using (var jsonReader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(content)))
					{
						return serializer.Deserialize<T>(jsonReader);
					}
				}
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				response.Dispose();
			}
		}

		private static void SetRequestHeaders(HttpClient httpClient, object headers)
		{
			if (headers is System.Net.Http.Headers.HttpHeaders httpHeaders)
			{
				foreach (var httpHeader in httpHeaders)
				{
					httpClient.DefaultRequestHeaders.Add(httpHeader.Key, string.Join(", ", httpHeader.Value));
				}
			}
			else if (headers is IDictionary<string, object> dictHeaders)
			{
				foreach (var dictHeader in dictHeaders)
				{
					var headerValue = dictHeader.Value is IEnumerable<object> enumerable ? string.Join(", ", enumerable) : dictHeader.Value.ToString();
					httpClient.DefaultRequestHeaders.Add(dictHeader.Key, string.Join(", ", headerValue));
				}
			}
		}

		private class AuthenticationHeaders : System.Net.Http.Headers.HttpHeaders
		{
			public string Authorization
			{
				get => this.TryGetValues("Authorization", out var values) ? values.FirstOrDefault() : default;
				set
				{
					this.Remove("Authorization");
					if (value != null)
						this.Add("Authorization", value);
				}
			}
		}
		private class GetRequestHeaders : SettingsBase
		{
			public string Authorization
			{
				get => this.GetValueOrDefault("Authorization", default(string));
				set => this.SetOrRemove("Authorization", value);
			}
		}

	}
}
