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
				this._location = value;
				_ = this.NavigateToAsync(value);
			}
		}

		private TaskCompletionSource<(string contentString, byte[] contentBytes, string contentType)> _navigationTCS;
		public Task<(string contentString, byte[] contentBytes, string contentType)> NavigationTask => this._navigationTCS.Task;
		public bool IsRoot => this._parent == null;
		public HostWindow Root => this._parent?.Root ?? this;
		private readonly System.Net.Http.HttpClientHandler _handler;
		private readonly System.Net.Http.HttpClient _client;

		public event Action<string, string, byte[], string> Loaded;

		public HostWindow(HostWindow parent = null)
		{
			this._parent = parent;
			this._location = "#";
			this._history = new Stack<string>();
			this._historyFwd = new Stack<string>();
			this._navigationTCS = new TaskCompletionSource<(string contentString, byte[] contentBytes, string contentType)>();
			this._navigationTCS.TrySetResult((null, null, null));
			this._handler = new System.Net.Http.HttpClientHandler();
			this._handler.ServerCertificateCustomValidationCallback += (HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors) =>
			{
				return true;
			};
			this._handler.AllowAutoRedirect = false;
			this._client = new System.Net.Http.HttpClient(this._handler);
		}

		public async Task<(string contentString, byte[] contentBytes, string contentType)> BackAsync()
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
		public async Task<(string contentString, byte[] contentBytes, string contentType)> ForwardAsync()
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

		private Task<(string contentString, byte[] contentBytes, string contentType)> NavigateToAsync(string url)
		{
			this._navigationTCS.TrySetCanceled();
			this._navigationTCS = new TaskCompletionSource<(string contentString, byte[] contentBytes, string contentType)>();
			this._navigationTCS.Task.ContinueWith(r =>
			{
				if (!r.IsFaulted && !r.IsCanceled)
					this.Loaded?.Invoke(url, r.Result.contentString, r.Result.contentBytes, r.Result.contentType);
			});
			_ = this.NavigateToCoreAsync(url, this._navigationTCS);
			return this._navigationTCS.Task;
		}

		private async Task NavigateToCoreAsync(string url, TaskCompletionSource<(string contentString, byte[] contentBytes, string contentType)> tcs)
		{
			var response = await this._client.GetAsync(url).ConfigureAwait(false);
			if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
			{
				this._location = response.Headers.Location.ToString();
				await this.NavigateToCoreAsync(response.Headers.Location.ToString(), tcs);
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				var contentType = response.Content.Headers.ContentType.MediaType;
				var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var contentBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				tcs.TrySetResult((contentString, contentBytes, contentType));
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

		public async Task<(string, byte[], string)> PostAsync(string url, Dictionary<string, string> content)
		{
			var response = await this._client.PostAsync(url, new System.Net.Http.FormUrlEncodedContent(content));
			if (response.StatusCode == System.Net.HttpStatusCode.Redirect)
			{
				this.location = response.Headers.Location.ToString();
				return await this.NavigationTask;
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.OK)
			{
				var contentType = response.Content.Headers.ContentType.MediaType;
				var contentString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
				var contentBytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
				return (contentString, contentBytes, contentType);
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
