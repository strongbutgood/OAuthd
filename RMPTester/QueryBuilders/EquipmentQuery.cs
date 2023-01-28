using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class EquipmentQueryExtension
	{
		public static EquipmentQuery ForEquipment(this IRestAPI restAPI)
		{
			var query = new EquipmentQuery(restAPI);
			return query;
		}
	}

	class EquipmentQuery : VersionedEntityQuery<Equipment>
	{
		public EquipmentQuery(IRestAPI restAPI) : base(restAPI) { }

		public Equipment Add()
		{
			return base.Post<Equipment>();
		}

		public int GetCount(bool? includeCheckedOut = null)
		{
			this.WithQuery<bool>("getCount", true);
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetResult<int>();
		}

		public int GetRuntimeCount(StateViewTypes? viewFilter = StateViewTypes.Approved)
		{
			this.WithQuery<bool>("getCount", true);
			if (viewFilter.HasValue)
				this.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Approved));
			this.WithQuery<bool>("runtimeView", true);
			return base.GetResult<int>();
		}

		public IEnumerable<Equipment> GetMany(bool? includeCheckedOut = true)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetMany();
		}

		public IEnumerable<Equipment> GetManyRuntime(bool? includeCheckedOut = true, StateViewTypes? viewFilter = StateViewTypes.Approved)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			if (viewFilter.HasValue)
				this.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Approved));
			this.WithQuery<bool>("runtimeView", true);
			return base.GetMany();
		}
	}
}
