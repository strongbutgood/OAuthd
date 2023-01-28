using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class RecipeParameterMapQueryExtension
	{
		public static RecipeParameterMapQuery ForRecipeParameterMap(this IRestAPI restAPI)
		{
			var query = new RecipeParameterMapQuery(restAPI);
			return query;
		}
	}

	class RecipeParameterMapQuery : QueryBuilderBase<RecipeParameterMap>
	{
		public RecipeParameterMapQuery(IRestAPI restAPI) : base(restAPI) { }

		public int GetCountForRecipe(int recipeId)
		{
			this.WithQuery<int>("recipeId", recipeId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public int GetCountForRecipeTemplate(int recipeTemplateId)
		{
			this.WithQuery<int>("recipeTemplateId", recipeTemplateId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<RecipeParameterMap> GetManyForRecipe(int recipeId)
		{
			this.WithQuery<int>("recipeId", recipeId);
			this.WithQuery("nameFilter", "");
			this.WithQuery<ParameterDataViewTypes>("typeFilter", ParameterDataViewTypes.All);
			this.WithQuery<bool>("modifiedOnly", false);
			this.WithQuery("capabilityNameFilter", "");
			this.WithQuery("parameterGroupNameFilter", "");
			this.WithQuery("procedureTokenNameFilter", "");
			this.WithQuery("ioReferenceFilter", "");
			this.WithQuery<bool>("missingTargetIOReferencesOnlyFilter", false);
			this.WithQuery<IOMaskTypes>("ioReferenceTypeFilter", IOMaskTypes.None);
			this.WithQuery("startDate", "");
			this.WithQuery("endDate", "");
			this.WithQuery<bool>("unmappedParametersFilter", false);
			this.WithQuery<bool>("editRequiredFilter", false);
			this.WithQuery<bool>("frequentlyUsedParametersFilter", false);
			this.WithQuery<bool>("showAllRuntimeParametersFilter", false);
			this.WithQuery<bool>("optionalTargetFilter", false);
			return base.GetMany();
		}

		public IEnumerable<RecipeParameterMap> GetManyForRecipeTemplate(int recipeTemplateId)
		{
			this.WithQuery<int>("recipeTemplateId", recipeTemplateId);
			this.WithQuery("nameFilter", "");
			this.WithQuery<ParameterDataViewTypes>("typeFilter", ParameterDataViewTypes.All);
			this.WithQuery<bool>("modifiedOnly", false);
			this.WithQuery("capabilityNameFilter", "");
			this.WithQuery("parameterGroupNameFilter", "");
			this.WithQuery("procedureTokenNameFilter", "");
			this.WithQuery("ioReferenceFilter", "");
			this.WithQuery<bool>("missingTargetIOReferencesOnlyFilter", false);
			this.WithQuery<IOMaskTypes>("ioReferenceTypeFilter", IOMaskTypes.None);
			this.WithQuery("startDate", "");
			this.WithQuery("endDate", "");
			this.WithQuery<bool>("unmappedParametersFilter", false);
			this.WithQuery<bool>("editRequiredFilter", false);
			this.WithQuery<bool>("frequentlyUsedParametersFilter", false);
			this.WithQuery<bool>("showAllRuntimeParametersFilter", false);
			this.WithQuery<bool>("optionalTargetFilter", false);
			return base.GetMany();
		}
	}
}
