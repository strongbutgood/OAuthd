using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class UnitOfMeasureQueryExtension
	{
		public static UnitOfMeasureQuery ForUnitOfMeasure(this IRestAPI restAPI)
		{
			return new UnitOfMeasureQuery(restAPI);
		}
	}

	class UnitOfMeasureQuery : QueryBuilderBase<UnitOfMeasure>
	{
		public UnitOfMeasureQuery(IRestAPI restAPI) : base(restAPI) { }

		public IEnumerable<UnitOfMeasure> GetMany(bool? includeNull = true, bool? includeDeleted = true)
		{
			if (includeNull.HasValue)
				this.WithQuery<bool>("includeNull", includeNull.GetValueOrDefault());
			if (includeDeleted.HasValue)
				this.WithQuery<bool>("includeDeleted", includeDeleted.GetValueOrDefault());
			return base.GetMany();
		}
	}
}
