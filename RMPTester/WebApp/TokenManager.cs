using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class TokenManager
	{
		public TokenManagerSettings Settings { get; }

		public Token Token { get; private set; }

		public event EventHandler TokenRemoved;
		public event EventHandler TokenExpiring;
		public event EventHandler TokenExpired;
		public event EventHandler TokenObtained;
		public event EventHandler SilentTokenRenewFailed;

		private System.Timers.Timer _tokenExpiringTimer;
		private System.Timers.Timer _tokenExpiredTimer;

		public TokenManager(IDictionary<string, object> settings)
		{
			if (settings != null)
				this.Settings = new TokenManagerSettings(settings);
			else
				this.Settings = new TokenManagerSettings();
			this.Settings.Add(new Dictionary<string, object>()
			{
				["persist"] = this.Settings.ContainsKey("store"),
				["persistKey"] = "TokenManager.Token",
			});

			TokenManager.LoadToken(this);
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
			this.ConfigureTokenExpiredHandling();
			this.ConfigureAutoRenewToken();

			// delay this so consuming apps can register for callbacks first
			var startupDelayTimer = new System.Timers.Timer(0.1);
			startupDelayTimer.Elapsed += (s, e) =>
			{
				startupDelayTimer.Stop();
				startupDelayTimer.Dispose();
				startupDelayTimer = null;
				this.ConfigureTokenExpiringHandling();
			};
			startupDelayTimer.AutoReset = false;
			startupDelayTimer.Start();
		}

		internal static void LoadToken(TokenManager manager)
		{
			if (manager.Settings.Persist)
			{
				if (manager.Settings.Store.TryGetValue(manager.Settings.PersistKey, out var tokenObj) &&
					tokenObj is Token token &&
					!token.Expired)
				{
					manager.Token = token;
				}
			}
		}

		public void SaveToken(IDictionary<string, object> token)
		{
			this.Token = new Token(token);

			if (this.Settings.Persist && !this.Token.Expired)
				this.Settings.Store[this.Settings.PersistKey] = token;
			else
				this.Settings.Store.Remove(this.Settings.PersistKey);

			if (token != null)
				this.TokenObtained?.Invoke(this, EventArgs.Empty);
			else
				this.TokenRemoved?.Invoke(this, EventArgs.Empty);
		}

		public void RemoveToken()
		{
			this.SaveToken(null);
		}

		public async Task RedirectForTokenAsync(Func<string, Task<string>> navigate)
		{
			var oidc = new OidcClient(this.Settings);
			await oidc.RedirectForToken(async (url, data) =>
			{
				var redirectedUrl = await navigate(url);
				var queryString = new Uri(redirectedUrl).Fragment;
				await this.ProcessTokenCallbackAsync(data, queryString);
			});
		}

		public async Task RenewTokenSilentAsync()
		{
			if (this.Settings.SilentRedirectUri == null)
				throw new Exception("silent_redirect_uri not configured");

			var settings = new Dictionary<string, object>(this.Settings)
			{
				["redirect_uri"] = this.Settings.SilentRedirectUri,
				["prompt"] = "none"
			};

			var oidc = new OidcClient(settings);
			await oidc.CreateTokenRequestAsync(async (url, data) =>
			{
				// TODO: Jeph -> Jeph: navigate to url and get the query stringy thing
				var queryString = default(string);
				var token = await oidc.ReadResponseAsync(data, queryString);
				this.SaveToken(token);
			});
		}

		public async Task RedirectForLogoutAsync(Func<string, Task> navigate)
		{
			var oidc = new OidcClient(this.Settings);
			var id_token_jwt = this.Token.IdTokenJWT;
			this.RemoveToken();
			await oidc.RedirectForLogout(id_token_jwt, navigate);
		}

		internal async Task ProcessTokenCallbackAsync(OidcAuthorizeRequest data, string queryString)
		{
			var oidc = new OidcClient(this.Settings);
			var token = await oidc.ReadResponseAsync(data, queryString);
			this.SaveToken(token);
		}

		private void ConfigureTokenExpiredHandling()
		{
			this.ConfigureTokenExpiredTimer();

			this.TokenObtained += (s, e) => this.ConfigureTokenExpiredTimer();
			this.TokenRemoved += (s, e) => this.CancelTokenExpiredTimer();
		}
		private void ConfigureTokenExpiredTimer()
		{
			this.CancelTokenExpiredTimer();
			if (this.Token?.ExpiresIn > 0)
			{
				// register 1 second beyond expiration so we don't get into edge conditions for expiration
				var duration_sec = this.Token.ExpiresIn.GetValueOrDefault(0) + 1;
				this._tokenExpiredTimer = new System.Timers.Timer(duration_sec * 1000.0);
				this._tokenExpiredTimer.Elapsed += this.TokenExpiredTimerCallback;
				this._tokenExpiredTimer.AutoReset = false;
				this._tokenExpiredTimer.Start();
			}
		}
		private void TokenExpiredTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
		{
			this._tokenExpiredTimer.Dispose();
			this._tokenExpiredTimer = null;
			if (this.Token != null)
				this.SaveToken(null);
			this.TokenExpired?.Invoke(this, EventArgs.Empty);
		}
		private void CancelTokenExpiredTimer()
		{
			if (this._tokenExpiredTimer != null)
			{
				this._tokenExpiredTimer.Stop();
				this._tokenExpiredTimer.Elapsed -= this.TokenExpiredTimerCallback;
				this._tokenExpiredTimer.Dispose();
				this._tokenExpiredTimer = null;
			}
		}

		private void ConfigureAutoRenewToken()
		{
			if (this.Settings.SilentRedirectUri != null &&
				this.Settings.SilentRenew)
			{
				this.TokenExpiring += this.TokenExpiringSilentRenew;
			}
		}
		private async void TokenExpiringSilentRenew(object sender, EventArgs e)
		{
			try
			{
				await this.RenewTokenSilentAsync();
			}
			catch (Exception err)
			{
				this.SilentTokenRenewFailed?.Invoke(this, EventArgs.Empty);
				Console.WriteLine(err.Message ?? err.ToString());
			}
		}


		private void ConfigureTokenExpiringHandling()
		{
			this.ConfigureTokenExpiringTimer();

			this.TokenObtained += (s, e) => this.ConfigureTokenExpiringTimer();
			this.TokenRemoved += (s, e) => this.CancelTokenExpiringTimer();
		}
		private void ConfigureTokenExpiringTimer()
		{
			this.CancelTokenExpiringTimer();
			if (this.Token?.Expired == false)
			{
				var duration = this.Token.ExpiresIn;
				if (duration > 60)
				{
					var duration_sec = duration.GetValueOrDefault(0) - 60;
					this._tokenExpiringTimer = new System.Timers.Timer(duration_sec * 1000.0);
					this._tokenExpiringTimer.Elapsed += this.TokenExpiringTimerCallback;
					this._tokenExpiringTimer.AutoReset = false;
					this._tokenExpiringTimer.Start();
				}
				else
				{
					this.TokenExpiringTimerCallback(this._tokenExpiringTimer, null);
				}
			}
		}
		private void TokenExpiringTimerCallback(object sender, System.Timers.ElapsedEventArgs e)
		{
			this._tokenExpiringTimer.Dispose();
			this._tokenExpiringTimer = null;
			this.TokenExpiring?.Invoke(this, EventArgs.Empty);
		}
		private void CancelTokenExpiringTimer()
		{
			if (this._tokenExpiringTimer != null)
			{
				this._tokenExpiringTimer.Stop();
				this._tokenExpiringTimer.Elapsed -= this.TokenExpiredTimerCallback;
				this._tokenExpiringTimer.Dispose();
				this._tokenExpiringTimer = null;
			}
		}
	}
}
