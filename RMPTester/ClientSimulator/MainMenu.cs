using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the main menu of the Recipe Manager Plus client interface and provides methods to simulate clicking main menu items.
	/// </summary>
	static class MainMenu
	{
		/// <summary>Opens the <see cref="EquipmentOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static EquipmentOverviewPage Equipments(IRestAPI restAPI)
		{
			return EquipmentOverviewPage.Open(restAPI);
		}

		/// <summary>Opens the <see cref="FormulaTemplateOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static FormulaTemplateOverviewPage FormulaTemplates(IRestAPI restAPI)
		{
			return FormulaTemplateOverviewPage.Open(restAPI);
		}

		/// <summary>Opens the <see cref="RecipeTemplateOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static RecipeTemplateOverviewPage RecipeTemplates(IRestAPI restAPI)
		{
			return RecipeTemplateOverviewPage.Open(restAPI);
		}

		/// <summary>Opens the <see cref="FormulaOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static FormulaOverviewPage Formulas(IRestAPI restAPI)
		{
			return FormulaOverviewPage.Open(restAPI);
		}

		/// <summary>Opens the <see cref="RecipeOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static RecipeOverviewPage Recipes(IRestAPI restAPI)
		{
			return RecipeOverviewPage.Open(restAPI);
		}

		public static string GetLoggedInUser(IRestAPI restAPI)
		{
			try
			{
				return restAPI.LoggedInUser;
			}
			catch { }
			return "{unknown}";
		}
	}
}
