using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class CapabilityQueryExtension
	{
		public static CapabilityQuery ForCapability(this IRestAPI restAPI)
		{
			var query = new CapabilityQuery(restAPI);
			return query;
		}
	}

	class CapabilityQuery : VersionedEntityQuery<Capability>
	{
		public CapabilityQuery(IRestAPI restAPI) : base(restAPI) { this.ControllerName = "Capabilities"; }

		public int GetCount(bool? includeCheckedOut = null)
		{
			this.WithQuery<bool>("getCount", true);
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetResult<int>();
		}

		public IEnumerable<Capability> GetMany(bool? includeCheckedOut = true)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetMany();
		}

		public IEnumerable<Capability> GetManyForProcedure(int procedureId)
		{
			this.WithQuery<int>("procedureId", procedureId);
			return base.GetMany();
		}
	}
}
