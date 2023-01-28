using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class UserGroupExtension
	{
		public static UserGroupQuery ForUserGroup(this IRestAPI restAPI)
		{
			return new UserGroupQuery(restAPI);
		}
	}

	class UserGroupQuery : QueryBuilderBase<UserGroup>
	{
		public UserGroupQuery(IRestAPI restAPI) : base(restAPI) { }

		public int GetCountForEquipment(int equipmentRootId)
		{
			this.WithQuery<int>("equipmentRootId", equipmentRootId);
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<UserGroup> GetManyForEquipment(int equipmentRootId)
		{
			this.WithQuery<int>("equipmentRootId", equipmentRootId);
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
