using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class EquipmentParameterExtension
	{
		public static EquipmentParameterQuery ForEquipmentParameter(this IRestAPI restAPI)
		{
			return new EquipmentParameterQuery(restAPI);
		}
	}

	class EquipmentParameterQuery : QueryBuilderBase<EquipmentParameter>
	{
		public EquipmentParameterQuery(IRestAPI restAPI) : base(restAPI) { }

		public int GetCountForEquipment(int equipmentId)
		{
			this.WithQuery<int>("equipmentId", equipmentId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<EquipmentParameter> GetManyForEquipment(int equipmentId)
		{
			this.WithQuery<int>("equipmentId", equipmentId);
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
