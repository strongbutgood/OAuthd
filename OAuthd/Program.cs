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
			Host.Default.location = "http://ac1vs03/recipemanagerplus";
			var waiter = new WindowLoadWaiter();
			await waiter.WaitAsync();
			var processMfg = new ProcessMfg();
			await processMfg.OnDocumentReadAsync();
			await waiter.WaitAsync();
			await processMfg.DoLoginAsync();
			await waiter.WaitAsync();
			await processMfg.OnDocumentReadAsync();
			

			//var options = new IdentityModel.OidcClient.OidcClientOptions()
			//{
			//	ClientId = rmpHost + "\\\\RecipeManagerPlus",
			//	RedirectUri = "https://AC1VS03/RecipeManagerPlus/sts_callback.cshtml",
			//	PostLogoutRedirectUri = "https://AC1VS03/RecipeManagerPlus/index.cshtml",
			//	Scope = "openid profile system",
			//	Authority = stsPath,
			//};

			while (true)
			{

			}
		}

	}
}
