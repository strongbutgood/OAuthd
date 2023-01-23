using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class ProcessMfg
	{
		public string stsPath { get; }
		public string rmpHost { get; }
		public string aimHost { get; }
		public TokenManager tokenManager { get; private set; }

		public ProcessMfg(string stsPath = "https://ac1vs11.milky.local/identitymanager/", string rmpHost = "AC1VS03", string aimHost = "ac1vs11.milky.local")
		{
			this.stsPath = stsPath;
			this.rmpHost = rmpHost;
			this.aimHost = aimHost;
		}

		public async Task OnDocumentReadAsync()
		{
			var html = (await Host.Default.MainWindow.NavigationTask).ContentString;
			var localStorage = Host.Default.LocalStorage;
			var location = Host.Default.location;
			if (Host.Default.location.hash.Length > 0)
				localStorage.setItem("index_hash", Host.Default.location.hash);
			else
				localStorage.removeItem("index_hash");
			if (Host.Default.location.search.Length > 0)
				localStorage.setItem("index_query", Host.Default.location.search);
			else
				localStorage.removeItem("index_query");
			this.tokenManager = new TokenManager(new Dictionary<string, object>()
			{
				["client_id"] = this.rmpHost.ToUpper() + "\\Recipe Manager Plus",
				["redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagement/sts_callback.cshtml",
				["post_logout_redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagement/index.cshtml",
				["scope"] = "openid profile system",
				["authority"] = this.stsPath,
				["silent_redirect_uri"] = location.protocol + "//" + location.hostname + "/RecipeManagement/sts_frame.cshtml",
				["silent_renew"] = true,
				["rememberme"] = false,
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
			var navigation = await Host.Default.MainWindow.NavigationTask;
			var inputs = this.GetInputs(navigation.ContentType, navigation.ContentString);
			var modelJson = this.GetModelJson(navigation.ContentType, navigation.ContentString);
			var formValues = inputs.ToDictionary(i => i.Key, i => i.Value.TryGetValue("value", out var value) ? value : "");
			if (formValues.TryGetValue("ReturnUrl", out var returnUrl))
			{
				formValues["ReturnUrl"] = returnUrl.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "'").Replace("&amp;", "&");
			}
			formValues["Username"] = @"milky\svc.Aveva";
			formValues["Password"] = @"OCdY!Adn1m";
			/*var content3 = await Host.Default.MainWindow.PostAsync(Host.Default.location, new Dictionary<string, string>()
			{
				["idsrv.xsrf"] = modelJson["antiForgery"]["value"].ToObject<string>(),
				["Username"] = "administrator",
				["Password"] = "OCdY!Adn1m",
			});//*/
			var content3 = await Host.Default.MainWindow.PostAsync(Host.Default.location, formValues);
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
			var query = Host.Default.LocalStorage.getItem("index_query");
			if (query == null)
				query = "";
			try
			{
				await tokenManager.processTokenCallbackAsync(null);
				Host.Default.location = new Uri(new Uri(Host.Default.location), "/RecipeManagement/index.cshtml" + query + hash).ToString();
			}
			catch (Exception e)
			{
				Console.WriteLine("Failed to process token callback: " + e);
				//Host.Default.location = new Uri(new Uri(Host.Default.location), "/RecipeManagement/index.cshtml" + query + hash).ToString();
				throw new Exception("Cannot continue");
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
			httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.tokenManager.access_token);
			var userName = this.tokenManager.id_token?.Count > 0 ? (string)this.tokenManager.id_token["sub"] : "";
			var storedDeviceId = Host.Default.LocalStorage.getItem(userName) as string ?? "";
			try
			{
				var response = await httpClient.PostAsync("/RecipeManagement/api/systemmanagement/setcookie/?deviceId=" + storedDeviceId, new System.Net.Http.StringContent(""));
				await Host.Default.FileLogger.StoreRequestResponseAsync(response, null);
				if (!response.IsSuccessStatusCode)
				{
					Console.WriteLine("Failed to get authentication cookie!");
					//configureAccessBlockAndErrorMessageConfirmWindow();
					//recipeManagerCommon.errorOnLogin = !0;
					//logFailure(n)
					throw new Exception(response.ReasonPhrase);
				}
				response = await httpClient.GetAsync("/RecipeManagement/api/SystemManagement/getdeviceId");
				var gettedDeviceId = await response.Content.ReadAsStringAsync();
				if (gettedDeviceId != storedDeviceId)
				{
					Host.Default.LocalStorage.setItem(userName, gettedDeviceId);
				}
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
		private Dictionary<string, Dictionary<string, string>> GetInputs(string contentType, string html)
		{
			if (contentType != "text/html")
				return null;
			try
			{
				var html2 = this.MakeMoreXHtml(html);
				var xhtml = XDocument.Parse(html2);
				var forms = xhtml.Root.XPathSelectElements("//form");
				foreach (var form in forms)
				{
					var inputs = form.XPathSelectElements("//input");
					return inputs.ToDictionary(
						e => e.Attribute("name").Value,
						e => e.XPathSelectElements("/@*").ToDictionary(
							a => a.Name.LocalName,
							a => a.Value
						)
					);
				}
			}
			catch
			{

			}

			var regex = new System.Text.RegularExpressions.Regex(@"<input(?<attr>\s+(?<attr_name>[A-Za-z_][A-Za-z0-9_-]*)\s*=\s*""(?<attr_value>[^""]*)"")*\s*/>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
			var res = new Dictionary<string, Dictionary<string, string>>();
			var match = regex.Match(html);
			while (match.Success)
			{
				//var attrs = new Dictionary<string, string>();
				var caps = match.Groups["attr"].Captures.Count;
				var attrs = match.Groups["attr_name"].Captures.OfType<System.Text.RegularExpressions.Capture>()
					.Zip(match.Groups["attr_value"].Captures.OfType<System.Text.RegularExpressions.Capture>(),
						(n, v) => (n, v))
					.ToDictionary(t => t.n.Value, t => t.v.Value);
				res[attrs["name"]] = attrs;
				match = regex.Match(html, match.Index + 1);
			}
			return res;
		}
		private string MakeMoreXHtml(string html) =>
			System.Text.RegularExpressions.Regex.Replace(
				System.Text.RegularExpressions.Regex.Replace(html, @"<meta(?<attrs>[^>]*)>", @"<meta${attrs}/>"),
				@"(?<scr><script[^>]*>)(?<content>.*?)</script>",
				@"${scr}<![CDATA[${content}]]></script>",
				System.Text.RegularExpressions.RegexOptions.Multiline
			).Replace("&trade;", "™");
	}
#pragma warning restore IDE1006 // Naming Styles
}
