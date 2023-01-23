using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OAuthd
{
	class FileLogger
	{
		public string FileDirPath { get; }

		private readonly DateTime _session;
		private long _id;

		public FileLogger(string path = @"D:\temp\rmp_hack\")
		{
			this._session = DateTime.Now;
			this.FileDirPath = Path.Combine(path, this._session.ToString("yy-MM-dd-hh-mm-ss"));
		}

		private void EnsureDirectory(string path)
		{
			var dir = Path.GetDirectoryName(path);
			if (Directory.Exists(dir))
				return;
			var stack = new Stack<string>();
			while (!Directory.Exists(dir))
			{
				stack.Push(dir);
				dir = Path.GetDirectoryName(dir);
			}
			while (stack.Count > 0)
			{
				Directory.CreateDirectory(stack.Pop());
			}
		}

		public async Task StoreRequestResponseAsync(System.Net.Http.HttpResponseMessage response, System.Net.Http.HttpClientHandler clientHandler)
		{
			try
			{
				var id = Interlocked.Increment(ref this._id);
				var request = response.RequestMessage;
				var fileName = Path.Combine(this.FileDirPath, $"{id:000}-{request.RequestUri.Host}{request.RequestUri.AbsolutePath}.xml".Replace('/', '_'));
				this.EnsureDirectory(fileName);
				var xdoc = new XDocument(
					new XDeclaration("1.0", "utf-8", "yes"),
					new XElement("http",
						new XElement("request", new XCData(await request.ToRawString(clientHandler))),
						new XElement("response", new XCData(await response.ToRawString(clientHandler)))
					)
				);
				xdoc.Save(fileName, SaveOptions.None);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error saving request/response: {0}", ex);
			}
		}
	}
}
