using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class ParameterGroupExtension
	{
		public static ParameterGroupQuery ForParameterGroup(this IRestAPI restAPI)
		{
			return new ParameterGroupQuery(restAPI);
		}
	}

	class ParameterGroupQuery : QueryBuilderBase<ParameterGroup>
	{
		public ParameterGroupQuery(IRestAPI restAPI) : base(restAPI) { }

		public IEnumerable<ParameterGroup> GetMany(bool? includeNull = true, bool? includeDeleted = true)
		{
			if (includeNull.HasValue)
				this.WithQuery<bool>("includeNull", includeNull.GetValueOrDefault());
			if (includeDeleted.HasValue)
				this.WithQuery<bool>("includeDeleted", includeDeleted.GetValueOrDefault());
			return base.GetMany();
		}
	}
}
