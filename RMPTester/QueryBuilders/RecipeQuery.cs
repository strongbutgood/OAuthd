using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class RecipeQueryExtension
	{
		public static RecipeQuery ForRecipe(this IRestAPI restAPI)
		{
			var query = new RecipeQuery(restAPI);
			return query;
		}
	}

	class RecipeQuery : VersionedEntityQuery<Recipe>
	{
		public RecipeQuery(IRestAPI restAPI) : base(restAPI) { }

		public Recipe Add(int recipeTemplateId, int formulaId)
		{
			this.WithQuery<int>("recipeTemplateId", recipeTemplateId);
			this.WithQuery<int>("formulaId", formulaId);
			return base.Post<Recipe>();
		}

		public int GetCount()
		{
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<Recipe> GetMany(bool? includeCheckedOut = true, StateViewTypes? viewFilter = StateViewTypes.Latest, bool? upgradeableOnlyFilter = false, bool? checkedOutFilter = false)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			if (viewFilter.HasValue)
				this.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Latest));
			if (upgradeableOnlyFilter.HasValue)
				this.WithQuery<bool>("upgradeableOnlyFilter", upgradeableOnlyFilter.GetValueOrDefault());
			if (checkedOutFilter.HasValue)
				this.WithQuery<bool>("checkedOutFilter", checkedOutFilter.GetValueOrDefault());
			return base.GetMany();
		}

		public ViewCommandResult ValidateForEquipment(int recipeId, int equipmentId)
		{
			this.WithCommandType("Validate");
			this.WithIdValue(recipeId);
			this.WithQuery<int>("equipmentId", equipmentId);
			return base.Put();
		}
	}
}
