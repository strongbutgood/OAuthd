using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class RestAPI : IRestAPI
	{
		private readonly string _rmpHost;
		private readonly string _stsPath;
		private readonly TokenManager _tokenManager;
		private readonly HttpClient _httpClient;
		private readonly IDictionary<string, object> _store;
		private readonly Func<string> _getUserName;
		private readonly Func<string, string> _getUserPassword;
		private bool _activeLogin;
		private bool _logoutActive;

		public static async Task<RestAPI> Create(string rmpHost, Func<string> getUserName, Func<string, string> getUserPassword)
		{
			var store = new Dictionary<string, object>();
			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback += CertificateValidationCallback;
			handler.AllowAutoRedirect = false;
			var httpClient = new HttpClient(handler, true);
			var stsPath = await GetSTSPath(httpClient, "https://" + rmpHost + "/RecipeManagement");
			var tokenManager = new TokenManager(new Dictionary<string, object>()
			{
				["client_id"] = rmpHost.ToUpper() + "\\Recipe Manager Plus",
				["redirect_uri"] = "https://" + rmpHost + "/RecipeManagement/sts_callback.cshtml",
				["post_logout_redirect_uri"] = "https://" + rmpHost + "/RecipeManagement/index.cshtml",
				["scope"] = "openid profile system",
				["authority"] = stsPath,
				["silent_redirect_uri"] = "https://" + rmpHost + "/RecipeManagement/sts_frame.cshtml",
				["silent_renew"] = true,
				["rememberme"] = false,
				["store"] = store,
			});
			if (tokenManager.Token?.AccessToken == null)
			{
				await tokenManager.RedirectForTokenAsync(async url => await DoLoginAsync(url, httpClient, getUserName, getUserPassword));
			}
			if (tokenManager.Token?.AccessToken != null)
			{
			}
			var restApi = new RestAPI(rmpHost, stsPath, tokenManager, httpClient, store, getUserName, getUserPassword);
			try
			{
				tokenManager.TokenObtained += async (s, e) => await restApi.OnTokenObtained(false);
				await restApi.OnTokenObtained(true);
			}
			catch
			{
				restApi.Dispose();
				throw;
			}
			return restApi;
		}

		public string LoggedInUser => throw new NotImplementedException();

		private RestAPI(string rmpHost, string stsPath, TokenManager tokenManager, HttpClient httpClient, IDictionary<string, object> store, Func<string> getUserName, Func<string, string> getUserPassword)
		{
			this._rmpHost = rmpHost;
			this._stsPath = stsPath;
			this._tokenManager = tokenManager;
			this._httpClient = httpClient;
			this._store = store;
			this._getUserName = getUserName;
			this._getUserPassword = getUserPassword;
			this._activeLogin = this._tokenManager.Token?.AccessToken != null;
			var timer = new System.Timers.Timer(5000);
			void callback(object s, System.Timers.ElapsedEventArgs e)
			{
				if (!this._activeLogin)
				{
					_ = this.LogoutUserAsync(true);
					timer.Stop();
					timer.Dispose();
					timer = null;
				}
			}
			timer.Elapsed += callback;
			timer.AutoReset = true;
			timer.Start();
		}


		public Resource[] GetMany(string url) => this.GetOne(url).ToArray();
		public Resource GetOne(string url)
		{
			var httpClient = this.CreateHttpClient();
			var content = Task.Run(async () =>
			{
				if (url.StartsWith("api/"))
					url = "/RecipeManagement/" + url;
				var response = await httpClient.GetAsync(url);
				return Tuple.Create(response, await response.Content.ReadAsStringAsync());
			}).Result;
			return RestAPI.ToResource(content.Item1, content.Item2);
		}
		public Resource Post(string url) => throw new NotImplementedException();
		public Resource Post(string url, Resource resource) => throw new NotImplementedException();
		public Resource Put(string url) => throw new NotImplementedException();
		public Resource Put(string url, Resource resource) => throw new NotImplementedException();
		public void Dispose()
		{
			Task.Run(async () => await this.LogoutUserAsync(false)).GetAwaiter().GetResult();
			this._httpClient.Dispose();
		}

		private async Task LogoutUserAsync(bool removeCookie)
		{
			if (!this._logoutActive)
			{
				this._logoutActive = true;
				var httpClient = this.CreateHttpClient();
				using (var response = await httpClient.PutAsync("/RecipeManagement/api/systemmanagement/logout?removeCookie=" + removeCookie, new StringContent("")))
				{
					//await Host.Default.FileLogger.StoreRequestResponseAsync(response, null);
					if (removeCookie)
					{
						await this._tokenManager.RedirectForLogoutAsync(async url => await this._httpClient.GetAsync(url));
						this._activeLogin = false;
					}
					else
					{
						this._tokenManager.RemoveToken();
					}
				}
			}
		}

		private static async Task<string> DoLoginAsync(string url, HttpClient httpClient, Func<string> getUserName, Func<string, string> getUserPassword)
		{
			string contentType;
			string content;
			string loginUrl;
			using (var request = new HttpRequestMessage(HttpMethod.Get, url))
			{
				using (var response = await WithRedirects(httpClient, request, 2))
				{
					loginUrl = response.Headers.Location?.AbsoluteUri;
					contentType = response.Content.Headers.ContentType.MediaType;
					content = await response.Content.ReadAsStringAsync();
				}
			}
			var inputs = GetInputs(contentType, content);
			var formValues = inputs.ToDictionary(i => i.Key, i => i.Value.TryGetValue("value", out var value) ? value : "");
			if (formValues.TryGetValue("ReturnUrl", out var returnUrl))
			{
				formValues["ReturnUrl"] = returnUrl.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "'").Replace("&amp;", "&");
			}
			string userName;
			do
			{
				userName = getUserName();
			} while (string.IsNullOrWhiteSpace(userName));
			formValues["Username"] = userName; // @"milky\svc.Aveva";
			string password;
			do
			{
				password = getUserPassword(userName);
			} while (string.IsNullOrEmpty(password));
			formValues["Password"] = password; // @"OCdY!Adn1m";
			using (var postRequest = new HttpRequestMessage(HttpMethod.Post, loginUrl) { Content = new FormUrlEncodedContent(formValues) })
			{
				using (var postResponse = await WithRedirects(httpClient, postRequest, 2))
				{
					if (postResponse.Headers.Location != null)
					{
						if (postResponse.Headers.Location != new Uri(loginUrl))
						{
							return postResponse.Headers.Location.AbsoluteUri;
						}
					}
					var postResponseContent = await postResponse.Content.ReadAsStringAsync();
					var validationErrors = GetValidationErrors(postResponse.Content.Headers.ContentType.MediaType, postResponseContent);
					if (validationErrors.Any())
					{
						Console.WriteLine("Login validation errors:\r\n\t{0}", string.Join("\r\n\t", validationErrors));
					}
					throw new Exception("Location not available, see data for content.")
					{
						Data =
						{
							["content"] = postResponseContent
						},
					};
				}
			}
		}

		private static List<string> GetValidationErrors(string contentType, string postResponseContent)
		{
			if (contentType != "text/html")
				return null;
			var regex = new Regex(@"<div[^>]*?class=""danger validation-summary-errors""[^>]*?>(?:\s|\r|\n)*<ul>(?:\s|\r|\n|(?:<li>(?<validation>[^<]*?)</li>))*");
			var match = regex.Match(postResponseContent);
			if (match.Success)
			{
				if (match.Groups["validation"].Success ||
					match.Groups["class"].Success &&
					match.Groups["class"].Captures.OfType<Capture>().Any(c => c.Value == "validation-summary-errors"))
				{
					return match.Groups["validation"].Captures.OfType<Capture>().Select(c => c.Value).ToList();
				}
			}
			return new List<string>();
		}

		private static async Task<HttpResponseMessage> WithRedirects(HttpClient httpClient, HttpRequestMessage request, int maxRedirects)
		{
			var requestStack = new Stack<HttpRequestMessage>();
			var responseStack = new Stack<HttpResponseMessage>();
			requestStack.Push(request);
			Console.WriteLine("{0}: {1}", request.Method.Method, request.RequestUri);
			if (request.Content != null)
				Console.WriteLine(await request.Content.ReadAsStringAsync());
			var response = await httpClient.SendAsync(request);
			responseStack.Push(response);
			HttpRequestMessage redirectRequest = null;
			while (response.StatusCode == System.Net.HttpStatusCode.Redirect)
			{
				redirectRequest?.Dispose();
				if (maxRedirects-- <= 0)
					throw new InvalidOperationException("Too many redirects.");
				if (response.Headers.Location != null)
				{
					var redirectUrl = response.Headers.Location.IsAbsoluteUri ? response.Headers.Location.AbsoluteUri : new Uri(requestStack.Peek().RequestUri, response.Headers.Location).AbsoluteUri;
					response.Dispose();
					redirectRequest = new HttpRequestMessage(HttpMethod.Get, redirectUrl);
					Console.WriteLine("{0}: {1}", redirectRequest.Method.Method, redirectRequest.RequestUri);
					requestStack.Push(redirectRequest);
					response = await httpClient.SendAsync(redirectRequest);
					responseStack.Push(response);
				}
				else
				{
					var responseContent = await response.Content?.ReadAsStringAsync();
					response.Dispose();
					throw new Exception("Location not available, see data for content.")
					{
						Data =
						{
							["content"] = responseContent
						},
					};
				}
			}
			redirectRequest?.Dispose();
			response.EnsureSuccessStatusCode();
			if (response.Headers.Location == null)
				response.Headers.Location = requestStack.Peek().RequestUri;
			return response;
		}

		private static Dictionary<string, Dictionary<string, string>> GetInputs(string contentType, string html)
		{
			if (contentType != "text/html")
				return null;
			try
			{
				var html2 = MakeMoreXHtml(html);
				var xhtml = System.Xml.Linq.XDocument.Parse(html2);
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
				match = regex.Match(html, match.Index + match.Length);
			}
			return res;
		}
		private static string MakeMoreXHtml(string html) =>
			System.Text.RegularExpressions.Regex.Replace(
				System.Text.RegularExpressions.Regex.Replace(html, @"<meta(?<attrs>[^>]*)>", @"<meta${attrs}/>"),
				@"(?<scr><script[^>]*>)(?<content>.*?)</script>",
				@"${scr}<![CDATA[${content}]]></script>",
				System.Text.RegularExpressions.RegexOptions.Multiline
			).Replace("&trade;", "™");


		private static async Task<string> GetSTSPath(HttpClient httpClient, string rmpUrl)
		{
			using (var response = await httpClient.GetAsync(rmpUrl))
			{
				var html = await response.Content.ReadAsStringAsync();
				var regex = new System.Text.RegularExpressions.Regex(
					@"<script[^>]*>(?:(?:.|\r|\n)*?stsPath\s*=\s*""(?<value>[^""]*)""(?:.|\r|\n)*?|(?:.|\r|\n)*?)</script>",
					System.Text.RegularExpressions.RegexOptions.IgnoreCase,
					TimeSpan.FromSeconds(2)
				);
				var stsPath = html;
				try
				{
					var match = regex.Match(html);
					while (match.Success)
					{
						stsPath = match.Groups["value"].Value;
						match = regex.Match(html, match.Index + match.Length);
					}
				}
				catch { }
				return stsPath;
			}
		}

		private async Task OnTokenObtained(bool doInit = false)
		{
			var httpClient = this.CreateHttpClient();
			var userName = this._tokenManager.Token?.IdToken?.Subject ?? "";
			if (!this._store.TryGetValue(userName, out var storedDeviceId))
				storedDeviceId = "";
			try
			{
				using (var response = await httpClient.PostAsync("/RecipeManagement/api/systemmanagement/setcookie/?deviceId=" + storedDeviceId, new StringContent("")))
				{
					//await Host.Default.FileLogger.StoreRequestResponseAsync(response, null);
					if (!response.IsSuccessStatusCode)
					{
						Console.WriteLine("Failed to get authentication cookie!");
						//configureAccessBlockAndErrorMessageConfirmWindow();
						//recipeManagerCommon.errorOnLogin = !0;
						//logFailure(n)
						throw new Exception(response.ReasonPhrase);
					}
				}
				using (var response = await httpClient.GetAsync("/RecipeManagement/api/SystemManagement/getdeviceId"))
				{
					var gettedDeviceId = await response.Content.ReadAsStringAsync();
					if (gettedDeviceId != storedDeviceId.ToString())
					{
						this._store[userName] = gettedDeviceId;
					}
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

		private HttpClient CreateHttpClient()
		{
			var handler = new HttpClientHandler();
			handler.ServerCertificateCustomValidationCallback += CertificateValidationCallback;
			var access_token = this._tokenManager.Token?.AccessToken;
			var httpClient = new HttpClient(handler, true)
			{
				BaseAddress = new Uri("https://" + this._rmpHost),
				DefaultRequestHeaders =
				{
					Accept =
					{
						new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/xml"),
					},
					Authorization = access_token != null ? new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", access_token) : default,
				},
			};
			return httpClient;
		}

		#region Type Conversion

		/// <summary>Serializes an entity from the Recipe Manager Plus model into a <see cref="Resource"/> which can be sent to the server with APIs.</summary>
		/// <param name="entity">The entity to serialize.</param>
		internal static Resource ToResource(HttpResponseMessage response, string entity)
		{
			if (entity == null)
			{
				Console.WriteLine("DEBUG: RestAPI was returned a null object. Returning null to the caller.");
				return null;
			}
			// special case
			if (entity is string)
			{
				if (response.Content == null)
				{
					Console.WriteLine("WARNING: RestAPI was returned null content. Returning error in ViewCommandResult. Response: '{0}'.", new object[] { response.ReasonPhrase });
					return new ProcessMfg.Model.ViewCommandResult()
					{
						CommandError = new ProcessMfg.Model.ViewError()
						{
							ErrorCategory = 0,
							ErrorCode = 0,
							ErrorMessage = "No content returned. " + response.ReasonPhrase
						},
						Success = false
					}.ToResource((int)response.StatusCode);
				}

				string xmlContent = response.Content.ReadAsStringAsync().Result;
				try
				{
					if (!response.IsSuccessStatusCode)
					{
						var jsonSerializer = new Newtonsoft.Json.JsonSerializer();
						var jsonReader = new Newtonsoft.Json.JsonTextReader(new System.IO.StringReader(entity));
						var viewCommandResult = jsonSerializer.Deserialize<ProcessMfg.Model.ViewCommandResult>(jsonReader);
						return viewCommandResult.ToResource((int)response.StatusCode);
					}
					//var xmlContent2 = JsonToXml(xmlContent);
					return new Resource(xmlContent, (int)response.StatusCode);
				}
				catch (XmlException ex)
				{
					Console.WriteLine("WARNING: RestAPI was returned invalid XML content. Returning error in ViewCommandResult. Response: '{0}', Raw content: '{1}'.", new object[] { response.ReasonPhrase, xmlContent });
					return new ProcessMfg.Model.ViewCommandResult()
					{
						CommandError = new ProcessMfg.Model.ViewError() { ErrorMessage = "Failed to retrieve resource. " + response.ReasonPhrase },
						SubErrors =
							{
								new ProcessMfg.Model.ViewError() { ErrorMessage = "Bad content returned: " + xmlContent },
								new ProcessMfg.Model.ViewError() { ErrorMessage = "XmlException: " + ex.Message },
							},
						Success = false
					}.ToResource((int)response.StatusCode);
				}
			}
			Console.WriteLine("DEBUG: RestAPI was returned a raw object. Serializing object to XML. Raw object: '{0}'.", new object[] { entity });
			RestAPI.FixupNonFlagEnums(entity);
			string result;
			try
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(entity.GetType(), new DataContractSerializerSettings()
				{
					SerializeReadOnlyTypes = true,
				});
				using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
				{
					XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter)
					{
						Formatting = Formatting.None
					};
					dataContractSerializer.WriteObject(xmlTextWriter, entity);
					xmlTextWriter.Flush();
					string text = stringWriter.ToString();
					result = text;
				}
			}
			catch (Exception)
			{
				result = string.Empty;
			}
			return new Resource(result);
		}

		private static string JsonToXml(string jsonString)
		{
			var json = Newtonsoft.Json.Linq.JToken.Parse(jsonString);
			var sb = new StringBuilder();
			using (var xw = XmlWriter.Create(new StringWriter(sb)))
			{
				var wrap = json.Type != Newtonsoft.Json.Linq.JTokenType.Object;
				if (wrap)
					xw.WriteStartElement("Root");
				jTokenToXml(json, xw);
				if (wrap)
					xw.WriteEndElement();
			}
			return sb.ToString();

			void jTokenToXml(Newtonsoft.Json.Linq.JToken jToken, XmlWriter xmlWriter)
			{
				switch (jToken.Type)
				{
					case Newtonsoft.Json.Linq.JTokenType.Comment:
						xmlWriter.WriteComment(jToken.ToString());
						break;
					case Newtonsoft.Json.Linq.JTokenType.Array:
						jArrayToXml((Newtonsoft.Json.Linq.JArray)jToken, xmlWriter);
						break;
					case Newtonsoft.Json.Linq.JTokenType.Object:
						jObjectToXml((Newtonsoft.Json.Linq.JObject)jToken, xmlWriter);
						break;
					case Newtonsoft.Json.Linq.JTokenType.Property:
						jPropertyToXml((Newtonsoft.Json.Linq.JProperty)jToken, xmlWriter);
						break;
					case Newtonsoft.Json.Linq.JTokenType.Integer:
					case Newtonsoft.Json.Linq.JTokenType.Float:
					case Newtonsoft.Json.Linq.JTokenType.String:
					case Newtonsoft.Json.Linq.JTokenType.Boolean:
					case Newtonsoft.Json.Linq.JTokenType.Date:
					case Newtonsoft.Json.Linq.JTokenType.Guid:
					case Newtonsoft.Json.Linq.JTokenType.Uri:
					case Newtonsoft.Json.Linq.JTokenType.TimeSpan:
						xmlWriter.WriteString(jToken.ToString());
						break;
					case Newtonsoft.Json.Linq.JTokenType.Raw:
						xmlWriter.WriteString(jToken.ToString());
						break;
					case Newtonsoft.Json.Linq.JTokenType.Bytes:
						xmlWriter.WriteString(jToken.ToString());
						break;
					case Newtonsoft.Json.Linq.JTokenType.None:
					case Newtonsoft.Json.Linq.JTokenType.Undefined:
					case Newtonsoft.Json.Linq.JTokenType.Null:
					case Newtonsoft.Json.Linq.JTokenType.Constructor:
					default:
						break;
				}
			}
			void jArrayToXml(Newtonsoft.Json.Linq.JArray jArray, XmlWriter xmlWriter)
			{
				foreach (var jToken in jArray)
				{
					xmlWriter.WriteStartElement("Item");
					jTokenToXml(jToken, xmlWriter);
					xmlWriter.WriteEndElement();
				}
			}
			void jObjectToXml(Newtonsoft.Json.Linq.JObject jObject, XmlWriter xmlWriter)
			{
				foreach (var jProperty in jObject.Properties())
				{
					jPropertyToXml(jProperty, xmlWriter);
				}
			}
			void jPropertyToXml(Newtonsoft.Json.Linq.JProperty jProperty, XmlWriter xmlWriter)
			{
				xmlWriter.WriteStartElement(jProperty.Name);
				jTokenToXml(jProperty.Value, xmlWriter);
				xmlWriter.WriteEndElement();
			}

		}

		///*
		internal static void FixupNonFlagEnums(object entity)
		{
			if (entity is IEnumerable entities)
			{
				// we may be looking at enumerables here...
				foreach (var item in entities)
				{
					RestAPI.FixupNonFlagEnums(item);
				}
			}
			if (entity is ProcessMfg.Model.RecipeTemplate recipeTemplate)
			{
				int allowedRequests = (int)recipeTemplate.AllowedRequests;
				if (allowedRequests > 0 && (allowedRequests & (allowedRequests - 1)) == 0)
				{
					// a mix of flags is not allowed, just clear them we wont use them
					recipeTemplate.AllowedRequests = ProcessMfg.Model.RecipeRuntimeRequestType.None;
				}
			}
			else if (entity != null)
			{
				var t = entity.GetType();
				var ps = t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public)
					.Where(p => p.CanRead && p.CanWrite && p.PropertyType.IsEnum && p.PropertyType.GetCustomAttribute(typeof(FlagsAttribute), true) == null);

				foreach (var prop in ps)
				{
					try
					{
						int value = (int)prop.GetValue(entity);
						if (Enum.GetValues(prop.PropertyType).Cast<int>().Count(v => v == value) != 1)
						{
							var newValue = Enum.ToObject(prop.PropertyType, Enum.GetValues(prop.PropertyType).Cast<int>().First());
							Console.WriteLine("WARNING: The value '{0}' of property '{1}' is not exactly one of the available members in enum type '{2}' on entity type '{3}'. Coercing the value to the first enum member '{4}'.", new object[] { value, prop.Name, prop.PropertyType.FullName, t.FullName, newValue });
							prop.SetValue(entity, newValue);
						}
					}
					catch (Exception ex)
					{
						Console.WriteLine("ERROR: An error occurred while checking if property '{0}' on entity type '{1}' needed coercion to prevent serialization issues.\r\n{2}", new object[] { prop.Name, t.FullName, ex });
					}
				}
			}
		}
		//*/

		#endregion Type Conversion


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

	}
}
