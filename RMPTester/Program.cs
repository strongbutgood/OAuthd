using MATS.Module.RecipeManagerPlus.ClientSimulator;
using MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMPTester
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var restApi = await RestAPI.Create("AC1VS03");
			var waiting = true;
			while (waiting)
			{
				await Task.Delay(10000);
			}
			try
			{
				Console.WriteLine("Gettings all recipe templates");
				var rt_ov = MainMenu.RecipeTemplates(restApi);
				foreach (var rt in rt_ov.Entities)
				{
					Console.WriteLine($"Recipe Template: {0},\r\n{1}\r\n", rt.Name, Newtonsoft.Json.Linq.JObject.FromObject(rt).ToString(Newtonsoft.Json.Formatting.Indented));
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine("ERROR: {0}", ex);
			}
			finally
			{
				restApi.Dispose();
			}
			Console.WriteLine("Press any key to exit...");
			Console.ReadKey();
		}
	}
}
