using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OAuthd
{
#pragma warning disable IDE1006 // Naming Styles
	class FrameLoader
	{
		private string url;

		public FrameLoader(string url)
		{
			this.url = url;
		}

		public async Task<object> loadAsync(string url = null)
		{
			if (url == null)
				url = this.url;

			if (url == null)
				throw new ArgumentException("No url provided", nameof(url));

			var tcs = new TaskCompletionSource<object>();
			var cts = new CancellationTokenSource();
			System.Timers.Timer timer = default;
			//var frameHtml = "<iframe style=\"display:none\"></iframe>";
			var frame = Host.Default.MakeIFrame("*");

			void cleanup()
			{
				Host.Default.Message -= message;
				if (timer != null)
				{
					timer.Stop();
					timer.Elapsed -= cancel;
				}
				timer = null;
				//frame.remove();
			}

			void cancel(object sender, System.Timers.ElapsedEventArgs e)
			{
				cleanup();
				tcs.TrySetCanceled();
				cts.Cancel();
			}

			void message(MessageEvent e)
			{
				if (timer != null && string.Equals(e.origin, Host.Default.location.protocol + "//ac1vs03/", StringComparison.OrdinalIgnoreCase))
				{
					cleanup();
					tcs.TrySetResult(e.data);
				}
			}

			timer = new System.Timers.Timer(5000.0);
			timer.Elapsed += cancel;
			timer.Start();
			Host.Default.Message += message;
			//frame.attr("src", url);
			var httpClient = new System.Net.Http.HttpClient();
			var response = await httpClient.GetAsync(url, cts.Token);
			await Host.Default.FileLogger.StoreRequestResponseAsync(response, null);
			var contentString = await response.Content.ReadAsStringAsync();
			// am I meant to call something?
			frame.location = url;
			await frame.NavigationTask;
			return await tcs.Task;
		}

	}
#pragma warning restore IDE1006 // Naming Styles
}
