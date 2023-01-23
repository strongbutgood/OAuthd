using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OAuthd
{
	class Program
	{

		static async Task Main(string[] args)
		{
			//Host.Default.MainWindow.Loaded += MainWindow_Loaded;
			try
			{
				Host.Default.location = "https://ac1vs03/RecipeManagement";
				var waiter = new WindowLoadWaiter();
				await waiter.WaitAsync();
				var processMfg = new ProcessMfg();
				await processMfg.OnDocumentReadAsync();
				await waiter.WaitAsync();
				await processMfg.DoLoginAsync();
				await waiter.WaitAsync();
				await processMfg.OnDocumentReadAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex);
			}
			

			//var options = new IdentityModel.OidcClient.OidcClientOptions()
			//{
			//	ClientId = rmpHost + "\\\\RecipeManagerPlus",
			//	RedirectUri = "https://AC1VS03/RecipeManagerPlus/sts_callback.cshtml",
			//	PostLogoutRedirectUri = "https://AC1VS03/RecipeManagerPlus/index.cshtml",
			//	Scope = "openid profile system",
			//	Authority = stsPath,
			//};

			Console.WriteLine("Press any key to finish...");
			Console.ReadKey();
		}

		private static void MainWindow_Loaded(object sender, HostNavigationEventArgs args)
		{
			Console.WriteLine("TRACE BEGIN");
			Console.WriteLine("TRACE: Finished loading url {0}", args.Url);
			Console.WriteLine("TRACE: Content Type = {0}", args.ContentType);
			Console.WriteLine("TRACE: ");
			Console.WriteLine("TRACE: {0}", args.ContentString);
			Console.WriteLine("TRACE: ");
			if (args.Raw != null)
			{
				Console.WriteLine("TRACE: ");
				Console.WriteLine("TRACE: Raw Response:");
				Console.WriteLine("TRACE: {0}", args.Raw);
				Console.WriteLine("TRACE: ");
				Console.WriteLine("TRACE END");
			}
		}
	}
}
