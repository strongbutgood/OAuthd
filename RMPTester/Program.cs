using MATS.Module.RecipeManagerPlus.ClientSimulator;
using MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using Nito.OptionParsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMPTester
{
	class Program
	{
		class Options : CommandLineOptionsBase
		{
			[Nito.OptionParsing.Option("rmp-host", 'h', OptionArgument.Optional)]
			public string RMPHost { get; set; }

			[Nito.OptionParsing.Option("user", 'u', OptionArgument.Optional)]
			public string UserName { get; set; }

		}

		static async Task Main(string[] args)
		{
			var opts = CommandLineOptionsParser.Parse<Options>(args);
			if (opts.RMPHost == null)
			{
				while (string.IsNullOrWhiteSpace(opts.RMPHost))
				{
					Console.WriteLine("Enter the host name of the Recipe Management server:");
					opts.RMPHost = Console.ReadLine();
				}
			}
			else
			{
				Console.WriteLine("Continuing with the host name '{0}' from command line args...", opts.RMPHost);
			}
			if (opts.UserName == null)
			{
				Console.WriteLine("Enter the user name for authentication with Recipe Management:");
				opts.UserName = Console.ReadLine();
			}
			else
			{
				Console.WriteLine("Continuing with the user name '{0}' from command line args...", opts.UserName);
			}
			while (true)
			{
				Console.WriteLine("Disconnected. Press 'C' to connect, 'S' to change settings, or 'Q' or 'ESC' to quit...");
				var key = Console.ReadKey(intercept: true);
				if (key.Key == ConsoleKey.C)
				{
					try
					{
						// do connect
						await ConnectRMP(opts.RMPHost, opts.UserName);
					}
					catch (Exception ex)
					{
						Console.WriteLine("Critical error with connection:\r\n{0}", ex);
					}
				}
				else if (key.Key == ConsoleKey.S)
				{
					// change settings
					Console.WriteLine("Enter the host name of the Recipe Management server (currently '{0}'):", opts.RMPHost);
					opts.RMPHost = ReadConsoleLineOrDefault(opts.RMPHost);
					Console.WriteLine("Enter the user name for authentication with Recipe Management (currently '{0}'):", opts.UserName);
					opts.UserName = ReadConsoleLineOrDefault(opts.UserName);
				}
				else if (key.Key == ConsoleKey.Q ||
					key.Key == ConsoleKey.Escape)
				{
					// quit
					Console.WriteLine("You're welcome!");
					await Task.Delay(5000);
					break;
				}
				else
					continue;
			}
		}

		private static string ReadConsoleLineOrDefault(string ifEmpty, Func<string, bool> isEmpty = null)
		{
			var line = Console.ReadLine();
			if (isEmpty == null)
				isEmpty = string.IsNullOrWhiteSpace;
			if (isEmpty(line))
				return ifEmpty;
			return line;
		}

		private static async Task ConnectRMP(string rmpHost, string userName)
		{
			using (var restApi = await RestAPI.Create(rmpHost, () =>
			{
				Console.WriteLine("Enter user name (or enter confirm for '{0}'):", userName);
				var userName2 = Console.ReadLine();
				if (string.IsNullOrWhiteSpace(userName2))
					return userName;
				return userName2;
			}, userName2 =>
			{
				Console.WriteLine("Enter password for '{0}':", userName2);
				var sb = new StringBuilder();
				while (true)
				{
					var key = Console.ReadKey(intercept: true);
					if (key.Key == ConsoleKey.Enter)
						break;
					sb.Append(key.KeyChar);
				}
				return sb.ToString();
			}))
			{
				while (true)
				{
					Console.WriteLine("Main Menu. Press 'E' for equipment, 'F' for formulas, 'R' for recipes, 'T' for templates, otherwise any key to disconnect...");
					var key = Console.ReadKey(intercept: true);
					if (key.Key == ConsoleKey.E)
					{
						try
						{
							// do equipment
							EquipmentPage(restApi);
						}
						catch (Exception ex)
						{
							Console.WriteLine("Error in equipment:\r\n{0}", ex);
						}
					}
					else if (key.Key == ConsoleKey.F)
					{
						// do formulas
					}
					else if (key.Key == ConsoleKey.R)
					{
						// do recipes
					}
					else if (key.Key == ConsoleKey.T)
					{
						// do templates
						while (true)
						{
							Console.WriteLine("Press 'F' for formula templates, 'R' for recipe templates, otherwise any key to return to the main menu...");
							key = Console.ReadKey();
							if (key.Key == ConsoleKey.F)
							{
								// do formula templates
							}
							else if (key.Key == ConsoleKey.R)
							{
								try
								{
									// do recipe templates
									RecipeTemplatePage(restApi);
								}
								catch (Exception ex)
								{
									Console.WriteLine("Error in recipe templates:\r\n{0}", ex);
								}
							}
							else
								break;
						}
					}
					else
					{
						break;
					}
				}
			}
		}

		private static void EquipmentPage(RestAPI restApi)
		{
			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("Equipment Overview:");
				var ov = MainMenu.Equipments(restApi);
				Console.WriteLine("\t" + string.Join("\r\n\t", ov.Entities.Select(e => $"{e.Id}: '{e.Name}'")));
				Console.WriteLine();
				Console.WriteLine("Press 'F' to filter, 'D' to get detail, otherwise any key to return to the main menu...");
				var key = Console.ReadKey(intercept: true);
				if (key.Key == ConsoleKey.D)
				{
					// get detail
					Console.WriteLine("Enter equipment name or id:");
					var nameOrId = Console.ReadLine();
					if (int.TryParse(nameOrId, out var id))
					{
						var eq = ov.Entities.FirstOrDefault(e => e.Id == id);
						if (eq != null)
							Console.WriteLine("Equipment Detail for id {0}:\r\n{1}", id, Newtonsoft.Json.Linq.JObject.FromObject(eq).ToString(Newtonsoft.Json.Formatting.Indented));
						else
						{
							Console.WriteLine("Equipment with id {0} not found.", id);
							continue;
						}
					}
					else
					{
						var eq = ov.Entities.FirstOrDefault(e => e.Name == nameOrId);
						if (eq != null)
							Console.WriteLine("Equipment Detail for name '{0}':\r\n{1}", nameOrId, Newtonsoft.Json.Linq.JObject.FromObject(eq).ToString(Newtonsoft.Json.Formatting.Indented));
						else
						{
							Console.WriteLine("Equipment with name '{0}' not found.", nameOrId);
							continue;
						}
					}
					Console.WriteLine();
				}
				else if (key.Key == ConsoleKey.F)
				{

				}
				else
				{
					break;
				}
			}
		}

		private static void RecipeTemplatePage(RestAPI restApi)
		{
			while (true)
			{
				Console.WriteLine();
				Console.WriteLine("Recipe Template Overview:");
				var ov = MainMenu.RecipeTemplates(restApi);
				Console.WriteLine("\t" + string.Join("\r\n\t", ov.Entities.Select(e => $"{e.Id}: '{e.Name}'")));
				Console.WriteLine();
				Console.WriteLine("Press 'F' to filter, 'D' to get detail, otherwise any key to return to the main menu...");
				var key = Console.ReadKey(intercept: true);
				if (key.Key == ConsoleKey.D)
				{
					// get detail
					Console.WriteLine("Enter recipe template name or id:");
					var nameOrId = Console.ReadLine();
					if (int.TryParse(nameOrId, out var id))
					{
						var eq = ov.Entities.FirstOrDefault(e => e.Id == id);
						if (eq != null)
							Console.WriteLine("Recipe Template Detail for id {0}:\r\n{1}", id, Newtonsoft.Json.Linq.JObject.FromObject(eq).ToString(Newtonsoft.Json.Formatting.Indented));
						else
						{
							Console.WriteLine("Recipe Template with id {0} not found.", id);
							continue;
						}
					}
					else
					{
						var eq = ov.Entities.FirstOrDefault(e => e.Name == nameOrId);
						if (eq != null)
							Console.WriteLine("Recipe Template Detail for name '{0}':\r\n{1}", nameOrId, Newtonsoft.Json.Linq.JObject.FromObject(eq).ToString(Newtonsoft.Json.Formatting.Indented));
						else
						{
							Console.WriteLine("Recipe Template with name '{0}' not found.", nameOrId);
							continue;
						}
					}
					Console.WriteLine();
				}
				else if (key.Key == ConsoleKey.F)
				{

				}
				else
				{
					break;
				}
			}
		}
	}
}
