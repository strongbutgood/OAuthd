using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class Host
	{
		private static readonly Lazy<Host> __default = new Lazy<Host>();
		public static Host Default => Host.__default.Value;

		public FileLogger FileLogger { get; } = new FileLogger();

		public HostWindow MainWindow { get; } = new HostWindow();
		public HostLocation location
		{
			get => this.MainWindow.location;
			set => this.MainWindow.location = value;
		}
		public string stsPath { get; set; } = "https://AC1VS03/ASTS/";
		public string rmpHost { get; set; } = "AC1VS03";

		public HostStorage LocalStorage { get; } = new HostStorage();

		public event Action<MessageEvent> Message;

		public Host()
		{
		}

		public HostWindow MakeIFrame(string url)
		{
			var window = new HostWindow(this.MainWindow)
			{
				location = url
			};
			return window;
		}

		public void PostMessage(MessageEvent message)
		{
			this.Message?.Invoke(message);
		}
		public void PostMessage(object data, string origin, string lastEventId = null, object source = null, object[] ports = null)
		{
			this.PostMessage(new MessageEvent(data, origin, lastEventId, source, ports));
		}
	}
	class HostLocation
	{
		public Uri Uri { get; private set; }

		public string protocol => this.Uri.Scheme + ":";
		public string hostname => this.Uri.Host;
		public string search => this.Uri.Query;
		public string hash => this.Uri.Fragment;

		public HostLocation(string location)
		{
			if (Uri.TryCreate(location, UriKind.Absolute, out var uri))
				this.Uri = uri;
			else
				this.Uri = new Uri(location, UriKind.Relative);
		}

		public static implicit operator string(HostLocation location)
		{
			return location.Uri.ToString();
		}
		public static implicit operator HostLocation(string location)
		{
			return new HostLocation(location);
		}
	}
	class HostWindow
	{
		private string _location;
		private readonly Stack<string> _history;
		private readonly Stack<string> _historyFwd;
		private readonly HostWindow _parent;
		public HostLocation location
		{
			get => this._location;
			set
			{
				if (this._location != null)
					this._history.Push(this._location);
				if (!value.Uri.IsAbsoluteUri)
				{
					var uri = new Uri(this._location);
					var baseUri = new Uri(uri.Scheme + "://" + uri.Host);
					uri = new Uri(baseUri, value.Uri);
					value = uri.AbsoluteUri;
				}
				this._location = value;
				_ = this.NavigateToAsync(value);
			}
		}

		private TaskCompletionSource<HostNavigationEventArgs> _navigationTCS;
		public Task<HostNavigationEventArgs> NavigationTask => this._navigationTCS.Task;
		public bool IsRoot => this._parent == null;
		public HostWindow Root => this._parent?.Root ?? this;
		private readonly System.Net.Http.HttpClientHandler _handler;
		internal readonly System.Net.Http.HttpClient _client;

		public event EventHandler<HostNavigationEventArgs> Loaded;

		public HostWindow(HostWindow parent = null)
		{
			this._parent = parent;
			this._location = "#";
			this._history = new Stack<string>();
			this._historyFwd = new Stack<string>();
			this._navigationTCS = new TaskCompletionSource<HostNavigationEventArgs>();
			this._navigationTCS.TrySetResult(new HostNavigationEventArgs(null, null, null, null));
			this._handler = new System.Net.Http.HttpClientHandler();
			this._handler.ServerCertificateCustomValidationCallback += (HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors) =>
			{
				return true;
			};
			this._handler.AllowAutoRedirect = false;
			this._client = new System.Net.Http.HttpClient(this._handler);
		}

		public async Task<HostNavigationEventArgs> BackAsync()
		{
			if (this._history.Count > 0)
			{
				var popped = this._history.Pop();
				this._historyFwd.Push(this._location);
				this._location = popped;
				return await this.NavigateToAsync(popped);
			}
			return await this._navigationTCS.Task;
		}
		public async Task<HostNavigationEventArgs> ForwardAsync()
		{
			if (this._historyFwd.Count > 0)
			{
				var popped = this._historyFwd.Pop();
				this._history.Push(this._location);
				this._location = popped;
				return await this.NavigateToAsync(popped);
			}
			return await this._navigationTCS.Task;
		}

		private Task<HostNavigationEventArgs> NavigateToAsync(string url)
		{
			this._navigationTCS.TrySetCanceled();
			this._navigationTCS = new TaskCompletionSource<HostNavigationEventArgs>();
			this._navigationTCS.Task.ContinueWith(r =>
			{
				if (!r.IsFaulted && !r.IsCanceled)
					this.Loaded?.Invoke(this, r.Result);
			});
			_ = this.NavigateToCoreAsync(url, this._navigationTCS);
			return this._navigationTCS.Task;
		}

		private async Task NavigateToCoreAsync(string url, TaskCompletionSource<HostNavigationEventArgs> tcs)
		{
			Console.WriteLine("GET: {0}", url);
			var response = await this._client.GetAsync(url).ConfigureAwait(false);
			await Host.Default.FileLogger.StoreRequestResponseAsync(response, this._handler);
			if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
			{
				this._location = response.Headers.Location.ToString();
				await this.NavigateToCoreAsync(response.Headers.Location.ToString(), tcs);
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				tcs.TrySetResult(await HostNavigationEventArgs.CreateAsync(url, response));
			}
			else
			{
				try
				{
					response.EnsureSuccessStatusCode();
					tcs.TrySetException(new Exception("No Content"));
				}
				catch (Exception ex)
				{
					tcs.TrySetException(ex);
				}
			}
		}

		public async Task<HostNavigationEventArgs> PostAsync(string url, Dictionary<string, string> content)
		{
			var formContent = new System.Net.Http.FormUrlEncodedContent(content);
			return await this.PostAsync(url, formContent);
		}
		public async Task<HostNavigationEventArgs> PostAsync(string url, System.Net.Http.HttpContent content)
		{
			Console.WriteLine("POST: {0}", url);
			Console.WriteLine(await content.ReadAsStringAsync());
			var response = await this._client.PostAsync(url, content);
			await Host.Default.FileLogger.StoreRequestResponseAsync(response, this._handler);
			if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
			{
				this.location = response.Headers.Location.ToString();
				return await this.NavigationTask;
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				return await HostNavigationEventArgs.CreateAsync(url, response);
			}
			else
			{
				response.EnsureSuccessStatusCode();
				throw new Exception("No Content");
			}
		}
	}
	class HostStorage : SettingsBase
	{
		public object getItem(string key)
		{
			if (key != null && this.TryGetValue(key, out var value))
				return value;
			return null;
		}

		public void setItem(string key, object value)
		{
			this[key] = value;
		}

		public void removeItem(string key)
		{
			this.Remove(key);
		}
	}
	class HostNavigationEventArgs : EventArgs
	{
		public string Url { get; }
		public string ContentString { get; }
		public byte[] ContentBytes { get; }
		public string ContentType { get; }
		public string Raw { get; }

		public HostNavigationEventArgs(string url, string contentString, byte[] contentBytes, string contentType, string raw = null)
		{
			this.Url = url;
			this.ContentString = contentString;
			this.ContentBytes = contentBytes;
			this.ContentType = contentType;
		}
		public static async Task<HostNavigationEventArgs> CreateAsync(string url, System.Net.Http.HttpResponseMessage response)
		{
			return new HostNavigationEventArgs(
				url,
				await response.Content.ReadAsStringAsync().ConfigureAwait(false),
				await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false),
				response.Content.Headers.ContentType.MediaType,
				response.ToString());
		}
	}
	class MessageEvent
	{
		public object data { get; }
		public string origin { get; }
		public string lastEventId { get; }
		public object source { get; }
		public object[] ports { get; }

		public MessageEvent(object data, string origin, string lastEventId, object source, object[] ports)
		{
			this.data = data;
			this.origin = origin;
			this.lastEventId = lastEventId;
			this.source = source;
			this.ports = ports;
		}

	}
#pragma warning restore IDE1006 // Naming Styles
}
