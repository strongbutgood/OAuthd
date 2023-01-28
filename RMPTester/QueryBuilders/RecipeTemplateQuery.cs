using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class RecipeTemplateQueryExtension
	{
		public static RecipeTemplateQuery ForRecipeTemplate(this IRestAPI restAPI)
		{
			var query = new RecipeTemplateQuery(restAPI);
			return query;
		}
	}

	class RecipeTemplateQuery : VersionedEntityQuery<RecipeTemplate>
	{
		public RecipeTemplateQuery(IRestAPI restAPI) : base(restAPI) { }

		public RecipeTemplate Add()
		{
			return base.Post<RecipeTemplate>();
		}

		public int GetCount()
		{
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<RecipeTemplate> GetMany(bool? includeCheckedOut = true)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetMany();
		}

		public ViewCommandResult ValidateForEquipment(int recipeTemplateId, int equipmentId)
		{
			this.WithCommandType("Validate");
			this.WithIdValue(recipeTemplateId);
			this.WithQuery<int>("equipmentId", equipmentId);
			return base.Put();
		}
	}
}
