using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the Execution Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class ExecutionOverviewPage : ClientSummaryPage<Equipment>
	{
		/// <summary>Opens the <see cref="ExecutionOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static ExecutionOverviewPage Open(IRestAPI restAPI)
		{
			var equipmentQuery = restAPI.ForEquipment();
			var count = equipmentQuery.GetRuntimeCount(viewFilter: StateViewTypes.Approved);
			var many = equipmentQuery.GetManyRuntime(includeCheckedOut: true, viewFilter: StateViewTypes.Approved);
			var page = new ExecutionOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="ExecutionOverviewPage"/> instance for the given <paramref name="equipment"/>.</summary>
		/// <param name="equipment">The equipment to be displayed on this page.</param>
		private ExecutionOverviewPage(IEnumerable<Equipment> equipment) : base(equipment) { }

		/// <summary>Opens the detail page for the selected <paramref name="equipment"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="equipment">The equipment to open the detail page for.</param>
		public override ClientDetailPage<Equipment> Select(IRestAPI restAPI, Equipment equipment)
		{
			return ExecutionDetailsPage.Open(restAPI, equipment);
		}
	}

	/// <summary>
	/// Represents the Execution Details page for a specific <see cref="Equipment"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class ExecutionDetailsPage : ClientDetailPage<Equipment>
	{
		/// <summary>Opens the <see cref="ExecutionDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="equipment">The <see cref="Equipment"/> to get details for.</param>
		public static ExecutionDetailsPage Open(IRestAPI restAPI, Equipment equipment)
		{
			var page = new ExecutionDetailsPage(equipment)
			{
			};
			return page;
		}

		/// <summary>Creates a new <see cref="ExecutionDetailsPage"/> instance for the given <paramref name="equipment"/>.</summary>
		/// <param name="equipment">The equipment to be displayed on this page.</param>
		private ExecutionDetailsPage(Equipment equipment) : base(equipment) { }

		// TODO:5: Jeph -> Jeph/Rohan: fill out the details of the ExecutionDetailsPage of RMP client simulation API thing

		protected override IVersionedQueryBuilder<Equipment> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForEquipment();
		}
	}
}
