using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class ProcessMfg
	{
		public string stsPath { get; }
		public string rmpHost { get; }
		public TokenManager tokenManager { get; private set; }

		public ProcessMfg(string stsPath = "https://AC1VS03/ASTS/", string rmpHost = "AC1VS03")
		{
			this.stsPath = stsPath;
			this.rmpHost = rmpHost;
		}

		public async Task OnDocumentReadAsync()
		{
			var localStorage = Host.Default.LocalStorage;
			var location = Host.Default.location;
			if (Host.Default.location.hash.Length > 0)
				localStorage.setItem("index_hash", Host.Default.location.hash);
			else
				localStorage.removeItem("index_hash");
			this.tokenManager = new TokenManager(new Dictionary<string, object>()
			{
				["client_id"] = this.rmpHost.ToUpper() + "\\Recipe Manager Plus",
				["redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagerPlus/sts_callback.cshtml",
				["post_logout_redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagerPlus/index.cshtml",
				["scope"] = "openid profile system",
				["authority"] = this.stsPath,
				["silent_redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagerPlus/sts_frame.cshtml",
				["silent_renew"] = true,
			});
			if (this.tokenManager.access_token == null)
			{
				this.tokenManager.redirectForToken();
			}
			else
			{
				localStorage.setItem("activeLogin", true);
				var timer = new System.Timers.Timer(5000);
				void callback(object s, System.Timers.ElapsedEventArgs e)
				{
					if (!localStorage.GetValueOrDefault("activeLogin", default(bool)))
					{
						logoutUser(true);
						timer.Stop();
						timer.Dispose();
						timer = null;
					}
				}
				timer.Elapsed += callback;
				timer.AutoReset = true;
				timer.Start();
				this.tokenManager.addOnTokenObtained(async () => await OnTokenObtained(false));
				await OnTokenObtained(true);
			}
		}

		public async Task DoLoginAsync()
		{
			var (contentString, contentBytes, contentType) = await Host.Default.MainWindow.NavigationTask;
			var modelJson = this.GetModelJson(contentType, contentString);
			var content3 = await Host.Default.MainWindow.PostAsync(Host.Default.location, new Dictionary<string, string>()
			{
				["idsrv.xsrf"] = modelJson["antiForgery"]["value"].ToObject<string>(),
				["username"] = "administrator",
				["password"] = "OCdY!Adn1m",
			});
			await this.StsCallbackAsync();
		}

		public async Task StsCallbackAsync()
		{
			var config = new Dictionary<string, object>
			{
				["client_id"] = this.rmpHost.ToUpper() + "\\Recipe Manager Plus",
				["authority"] = this.stsPath,
				["load_user_profile"] = false,
			};
			this.tokenManager = new TokenManager(config);
			var hash = Host.Default.LocalStorage.getItem("index_hash");
			if(hash == null)
				hash = "";
			try
			{
				await tokenManager.processTokenCallbackAsync(null);
				Host.Default.location = new Uri(new Uri(Host.Default.location), "/recipemanagerplus/index.cshtml" + hash).ToString();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to process token callback: " + e);
				Host.Default.location = new Uri(new Uri(Host.Default.location), "/recipemanagerplus/index.cshtml" + hash).ToString();
				//Host.Default.location = "/recipemanagerplus/index.cshtml" + hash;
			}
		}

		public void logoutUser(bool removeCookie)
		{
			if (removeCookie)
			{
				this.tokenManager.redirectForLogout();
				Host.Default.LocalStorage.removeItem("activeLogin");
			}
			else
			{
				this.tokenManager.removeToken();
			}
		}

		private async Task OnTokenObtained(bool doInit = false)
		{
			var httpClient = new System.Net.Http.HttpClient();
			//IdentityModel.Client.AuthorizationHeaderExtensions.SetBearerToken(httpClient, this.tokenManager.access_token);
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.tokenManager.access_token);
			try
			{
				var response = await httpClient.PostAsync("/recipemanagerplus/api/systemmanagement/setcookie", new System.Net.Http.StringContent(""));
			}
			catch
			{
				Console.WriteLine("Failed to get authentication cookie!");
			}
			if (doInit)
			{
				//initApp();
			}
		}
		private Newtonsoft.Json.Linq.JObject GetModelJson(string contentType, string html)
		{
			if (contentType != "text/html")
				return null;
			var match = System.Text.RegularExpressions.Regex.Match(html, "<script\\s+id\\s*=\\s*(?:'modelJson'|\"modelJson\")\\s+type\\s*=\\s*(?:'application/json'|\"application/json\")\\s*>((?:.|\n|\r)*?)</script>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			if (match.Success)
			{
				var scriptText = match.Groups[1].Value;
				var unescapedText = scriptText.Trim().Replace("&quot;", "\"").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
				return Newtonsoft.Json.Linq.JObject.Parse(unescapedText);
			}
			return null;
		}
	}
#pragma warning restore IDE1006 // Naming Styles
}
