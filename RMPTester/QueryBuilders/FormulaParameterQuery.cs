using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class FormulaParameterQueryExtension
	{
		public static FormulaParameterQuery ForFormulaParameter(this IRestAPI restAPI)
		{
			var query = new FormulaParameterQuery(restAPI);
			return query;
		}
	}

	class FormulaParameterQuery : QueryBuilderBase<FormulaParameter>
	{
		public FormulaParameterQuery(IRestAPI restAPI) : base(restAPI) { }
		
		public FormulaParameter Add(FormulaParameter formulaParameter)
		{
			return base.Post<FormulaParameter>(formulaParameter);
		}

		public FormulaParameter Change(FormulaParameter formulaParameter, int? equipmentId = null, string equipmentName = null)
		{
			if (equipmentId.HasValue)
				this.WithQuery<int>("equipmentId", equipmentId.GetValueOrDefault());
			if (equipmentName != null)
				this.WithQuery("equipmentName", equipmentName);
			//AmbientLogger.Current.Debug(null, "Enter - FormulaParameterQuery.Change(...) with argument value '{0}'.", new object[] { formulaParameter.ToString("(null)") });
			var result = base.Put<FormulaParameter>(formulaParameter);
			//AmbientLogger.Current.Debug(null, "Exit - FormulaParameterQuery.Change(...) with result value '{0}'.", new object[] { result.ToString("(null)") });
			return result;
		}

		public void Delete(int formulaParameterId)
		{
			this.WithIdValue(formulaParameterId);
			base.Delete();
		}

		public int GetCountForFormula(int formulaId)
		{
			this.WithQuery<int>("formulaId", formulaId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public int GetCountForFormulaTemplate(int formulaTemplateId)
		{
			this.WithQuery<int>("formulaTemplateId", formulaTemplateId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<ParameterExtDef> GetInstanceParameterExtDefs(int formulaId)
		{
			this.WithCommandType("getInstanceParameterExtDefs");
			this.WithQuery<int>("id", formulaId);
			return base.GetMany<ParameterExtDef>();
		}

		public IEnumerable<ParameterExtDef> GetTemplateParameterExtDefs(int formulaTemplateId)
		{
			this.WithCommandType("getTemplateParameterExtDefs");
			this.WithIdValue(formulaTemplateId);
			return base.GetMany<ParameterExtDef>();
		}

		public IEnumerable<FormulaParameter> GetManyForFormula(int formulaId)
		{
			this.WithQuery<int>("formulaId", formulaId);
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

		public IEnumerable<FormulaParameter> GetManyForFormulaTemplate(int formulaTemplateId)
		{
			this.WithQuery<int>("formulaTemplateId", formulaTemplateId);
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
