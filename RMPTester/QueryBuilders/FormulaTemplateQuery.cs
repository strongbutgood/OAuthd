using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class FormulaTemplateQueryExtension
	{
		public static FormulaTemplateQuery ForFormulaTemplate(this IRestAPI restAPI, StateViewTypes? viewFilter = null)
		{
			var query = new FormulaTemplateQuery(restAPI);
			if (viewFilter.HasValue)
				query.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Latest));
			return query;
		}
	}

	class FormulaTemplateQuery : VersionedEntityQuery<FormulaTemplate>
	{
		public FormulaTemplateQuery(IRestAPI restAPI) : base(restAPI) { }

		public FormulaTemplate Add()
		{
			return base.Post<FormulaTemplate>();
		}

		public int GetCount()
		{
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<FormulaTemplate> GetMany(bool? includeCheckedOut = true)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetMany();
		}
	}
}
