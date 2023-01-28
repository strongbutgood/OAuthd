using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class FormulaQueryExtension
	{
		public static FormulaQuery ForFormula(this IRestAPI restAPI, StateViewTypes? viewFilter = null)
		{
			var query = new FormulaQuery(restAPI);
			if (viewFilter.HasValue)
				query.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Latest));
			return query;
		}
	}

	class FormulaQuery : VersionedEntityQuery<Formula>
	{
		public FormulaQuery(IRestAPI restAPI) : base(restAPI) { }

		public Formula Add(int formulaTemplateId)
		{
			this.WithQuery<int>("formulaTemplateId", formulaTemplateId);
			return base.Post<Formula>();
		}

		public int GetCount()
		{
			this.WithQuery<bool>("getCount", true);
			return base.GetResult<int>();
		}

		public IEnumerable<Formula> GetMany(bool? includeCheckedOut = true, StateViewTypes? viewFilter = StateViewTypes.Latest, bool? upgradeableOnlyFilter = false, bool? checkedOutFilter = false)
		{
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			if (viewFilter.HasValue)
				this.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Latest));
			if (upgradeableOnlyFilter.HasValue)
				this.WithQuery<bool>("upgradeableOnlyFilter", upgradeableOnlyFilter.GetValueOrDefault());
			if (checkedOutFilter.HasValue)
				this.WithQuery<bool>("checkedOutFilter", checkedOutFilter.GetValueOrDefault());
			return base.GetMany();
		}

		public IEnumerable<Formula> GetManyForRecipeTemplate(int formulaTemplateId, bool? getLatest = false, bool? includeCheckedOut = true)
		{
			this.WithQuery<int>("formulaTemplateId", formulaTemplateId);
			if (getLatest.HasValue)
				this.WithQuery<bool>("getLatest", getLatest.GetValueOrDefault());
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetMany();
		}
	}
}
