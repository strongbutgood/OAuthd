using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class FormulaEquipmentQueryExtension
	{
		public static FormulaEquipmentQuery ForFormulaEquipment(this IRestAPI restAPI)
		{
			var query = new FormulaEquipmentQuery(restAPI);
			return query;
		}
	}

	class FormulaEquipmentQuery : QueryBuilderBase<FormulaEquipment>
	{
		public FormulaEquipmentQuery(IRestAPI restAPI) : base(restAPI) { }

		public IEnumerable<FormulaEquipment> GetManyForFormula(int formulaId)
		{
			this.WithQuery<int>("id", formulaId);
			return base.GetMany();
		}

		public IEnumerable<Formula> GetManyForEquipment(int equipmentRootId)
		{
			this.WithQuery<int>("equipmentRootId", equipmentRootId);
			return base.GetMany<Formula>();
		}
	}
}
