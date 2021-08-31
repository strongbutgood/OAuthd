using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class TokenManager
	{
		private readonly Settings _settings;
		private readonly Dictionary<string, List<Action>> _callbacks;
		private Token _token;
		private object _httpRequest;

		internal Token Token => this._token;

		public string id_token => this._token?.id_token;
		public string id_token_jwt => this._token?.id_token_jwt;
		public string access_token => this._token != null && !this._token.expired ? this._token.access_token : default;
		public bool expired => this._token?.expired ?? true;
		public long expires_in => this._token?.expires_in ?? 0;
		public long expires_at => this._token?.expires_at ?? 0;
		public IList<string> scopes => this._token != null ? new List<string>(this._token.scopes) : new List<string>();

		public event Action tokenRemoved;
		public event Action tokenExpiring;
		public event Action tokenExpired;
		public event Action tokenObtained;
		public event Action silentTokenRenewFailed;

		public TokenManager(IDictionary<string, object> settings)
		{
			if (settings != null)
				this._settings = new Settings(settings);
			else
				this._settings = new Settings();
			this._settings.Add(new Dictionary<string, object>()
			{
				[nameof(Settings.persist)] = true,
				[nameof(Settings.store)] = Host.Default.LocalStorage,
				[nameof(Settings.persistKey)] = "TokenManager.Token",
			});

			this._callbacks = new Dictionary<string, List<Action>>()
			{
				["tokenRemovedCallbacks"] = new List<Action>() { () => this.tokenRemoved?.Invoke() },
				["tokenExpiringCallbacks"] = new List<Action>() { () => this.tokenExpiring?.Invoke() },
				["tokenExpiredCallbacks"] = new List<Action>() { () => this.tokenExpired?.Invoke() },
				["tokenObtainedCallbacks"] = new List<Action>() { () => this.tokenObtained?.Invoke() },
				["silentTokenRenewFailedCallbacks"] = new List<Action>() { () => this.silentTokenRenewFailed?.Invoke() },
			};

			loadToken(this);
			/*
			var mgr = this;
			window.addEventListener("storage", function (e) {
				if (e.key === mgr._settings.persistKey) {
					loadToken(mgr);
					if (mgr._token) {
						mgr._callTokenObtained();
					}
					else {
						mgr._callTokenRemoved();
					}
				}
			});
			 */
			configureTokenExpired(this);
			configureAutoRenewToken(this);

			// delay this so consuming apps can register for callbacks first
			var timer = new System.Timers.Timer(0.1);
			timer.Elapsed += (s, e) =>
			{
				timer.Stop();
				timer = null;
				configureTokenExpiring(this);
			};
			timer.AutoReset = false;
			timer.Start();
		}

		internal static void configureTokenExpired(TokenManager mgr)
		{
			System.Timers.Timer timer = default;

			void callback(object sender, System.Timers.ElapsedEventArgs e)
			{
				timer = null;
				if (mgr._token != null)
					mgr.saveToken(null);
				mgr._callTokenExpired();
			};

			void cancel()
			{
				if (timer != null)
				{
					timer.Stop();
					timer.Elapsed -= callback;
					timer = null;
				}
			}

			void setup(long duration_sec)
			{
				timer = new System.Timers.Timer(duration_sec * 1000.0);
				timer.Elapsed += callback;
				timer.AutoReset = false;
				timer.Start();
			}

			void configure()
			{
				cancel();
				if (mgr.expires_in > 0)
					// register 1 second beyond expiration so we don't get into edge conditions for expiration
					setup(mgr.expires_in + 1);
			}

			configure();

			mgr.addOnTokenObtained(configure);
			mgr.addOnTokenRemoved(cancel);
		}

		internal static void configureAutoRenewToken(TokenManager mgr)
		{
			if (mgr._settings.silent_redirect_uri != null &&
				mgr._settings.silent_renew)
			{

				mgr.addOnTokenExpiring(async () =>
				{
					try
					{
						await mgr.renewTokenSilentAsync();
					}
					catch (Exception e)
					{
						mgr._callSilentTokenRenewFailed();
						Console.WriteLine(e.Message ?? e.ToString());
					}
				});

			}
		}

		internal static void configureTokenExpiring(TokenManager mgr)
		{
			System.Timers.Timer timer = default;

			void callback(object sender, System.Timers.ElapsedEventArgs e)
			{
				timer = null;
				mgr._callTokenExpiring();
			};

			void cancel()
			{
				if (timer != null)
				{
					timer.Stop();
					timer.Elapsed -= callback;
					timer = null;
				}
			}

			void setup(long duration_sec)
			{
				timer = new System.Timers.Timer(duration_sec * 1000.0);
				timer.Elapsed += callback;
				timer.AutoReset = false;
				timer.Start();
			}

			void configure()
			{
				cancel();
				if (!mgr.expired)
				{
					var duration = mgr.expires_in;
					if (duration > 60)
						setup(duration - 60);
					else
						callback(timer, null);
				}
			}

			configure();

			mgr.addOnTokenObtained(configure);
			mgr.addOnTokenRemoved(cancel);
		}

		internal static void loadToken(TokenManager mgr)
		{
			if (mgr._settings.persist)
			{
				if (mgr._settings.store.getItem(mgr._settings.persistKey) is string tokenJson)
				{
					var token = Token.fromJSON(tokenJson);
					if (!token.expired)
						mgr._token = token;
				}
			}
		}

		public void setHttpRequest(object httpRequest)
		{
			if (httpRequest == null ||
				!(httpRequest is IDictionary<string, object> httpRequestObj) ||
				!(httpRequestObj["getJSON"] is Delegate))
				throw new ArgumentException("The provided value is not a valid http request.", nameof(httpRequest));
			this._httpRequest = httpRequest;
		}

		private void _callTokenRemoved()
		{
			this._callbacks["tokenRemovedCallbacks"]?.ForEach(a => a());
		}
		private void _callTokenExpiring()
		{
			this._callbacks["tokenExpiringCallbacks"]?.ForEach(a => a());
		}
		private void _callTokenExpired()
		{
			this._callbacks["tokenExpiredCallbacks"]?.ForEach(a => a());
		}
		private void _callTokenObtained()
		{
			this._callbacks["tokenObtainedCallbacks"]?.ForEach(a => a());
		}
		private void _callSilentTokenRenewFailed()
		{
			this._callbacks["silentTokenRenewFailedCallbacks"]?.ForEach(a => a());
		}

		public void saveToken(object tokenObj)
		{
			if (!(tokenObj is Token token))
				token = Token.fromResponse(tokenObj);

			this._token = token;

			if (this._settings.persist && !this.expired)
				this._settings.store.setItem(this._settings.persistKey, token.toJSON());
			else
				this._settings.store.removeItem(this._settings.persistKey);

			if (token != null)
				this._callTokenObtained();
			else
				this._callTokenRemoved();
		}

		public void addOnTokenRemoved(Action callback)
		{
			this._callbacks["tokenRemovedCallbacks"].Add(callback);
		}
		public void addOnTokenObtained(Action callback)
		{
			this._callbacks["tokenObtainedCallbacks"].Add(callback);
		}
		public void addOnTokenExpiring(Action callback)
		{
			this._callbacks["tokenExpiringCallbacks"].Add(callback);
		}
		public void addOnTokenExpired(Action callback)
		{
			this._callbacks["tokenExpiredCallbacks"].Add(callback);
		}
		public void addOnSilentTokenRenewFailed(Action callback)
		{
			this._callbacks["silentTokenRenewFailedCallbacks"].Add(callback);
		}

		public void removeToken()
		{
			this.saveToken(null);
		}

		public void redirectForToken()
		{
			var oidc = new OidcClient(this._settings);
			oidc.redirectForToken();
		}

		public void redirectForLogout()
		{
			var oidc = new OidcClient(this._settings);
			var id_token_jwt = this.id_token_jwt;
			this.removeToken();
			oidc.redirectForLogout(id_token_jwt);
		}

		public async Task<OidcClient.TokenRequestDTO> createTokenRequestAsync()
		{
			var oidc = new OidcClient(this._settings);
			return await oidc.createTokenRequestAsync();
		}

		public async Task processTokenCallbackAsync(string queryString)
		{
			var oidc = new OidcClient(this._settings);
			var token = await oidc.readResponseAsync(queryString);
			this.saveToken(token);
		}

		public async Task renewTokenSilentAsync()
		{
			if (this._settings.silent_redirect_uri == null)
			{
				throw new Exception("silent_redirect_uri not configured");
				//return _promiseFactory.reject("silent_redirect_uri not configured");
			}

			var settings = new Settings(this._settings);
			settings["redirect_uri"] = settings.silent_redirect_uri;
			settings["prompt"] = "none";

			var oidc = new OidcClient(settings);
			var request = await oidc.createTokenRequestAsync();
			var frame = new FrameLoader(request.url);
			var hash = await frame.loadAsync();
			var token = await oidc.readResponseAsync(hash.ToString());
			this.saveToken(token);
		}

		public void processTokenCallbackSilent(object hash)
		{
		/*
			if (Host.Default != null && window !== Host.Default)
			{
				if (hash == null)
					hash = window.location.hash;
		 */
				if (hash != null)
					Host.Default.PostMessage(hash, "https://ac1vs03/");
			//}
		}

		private class Settings : SettingsBase
		{
			public Settings() : base() { }
			public Settings(IDictionary<string, object> settings) : base(settings) { }

			public bool persist => this.GetValueOrDefault(nameof(persist), default(bool));
			public HostStorage store => this.GetValueOrDefault(nameof(store), default(HostStorage));
			public string persistKey => this.GetValueOrDefault(nameof(persistKey), default(string));
			
			public string silent_redirect_uri => this.GetValueOrDefault(nameof(silent_redirect_uri), default(string));
			public bool silent_renew => this.GetValueOrDefault(nameof(silent_renew), default(bool));
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
