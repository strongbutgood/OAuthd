#undef WINWAIT_V2
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
	class WindowLoadWaiter
	{
#if WINWAIT_V2
		private ConcurrentDictionary<string, TaskCompletionSource<string>> _queue;
#else
		private TaskCompletionSource<bool> _tcs;
#endif

		public WindowLoadWaiter()
		{
#if WINWAIT_V2
			this._queue = new ConcurrentDictionary<string, TaskCompletionSource<string>>();
#else
			this._tcs = new TaskCompletionSource<bool>();
#endif
			Host.Default.MainWindow.Loaded += (s, args) => // (url, str, bytes, typ) =>
			{
#if WINWAIT_V2
				var tcs = this._queue.GetOrAdd(args.Url, u => new TaskCompletionSource<string>());
				tcs.TrySetResult(args.Url);
#else
				this._tcs.TrySetResult(true);
#endif
			};
		}

		public async Task WaitAsync(string url = null)
		{
#if WINWAIT_V2
			var tcs = this._queue.GetOrAdd(url, u => new TaskCompletionSource<string>());
			await tcs.Task;
			await Task.Yield();
			this._queue.TryRemove(url, out _);
#else
			await this._tcs.Task;
			await Task.Yield();
			this._tcs = new TaskCompletionSource<bool>();
#endif
		}
	}
}
