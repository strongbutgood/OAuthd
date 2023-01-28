using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class EquipmentOperationExtension
	{
		public static EquipmentOperationQuery ForEquipmentOperation(this IRestAPI restAPI)
		{
			return new EquipmentOperationQuery(restAPI);
		}
	}

	class EquipmentOperationQuery : QueryBuilderBase<EquipmentOperation>
	{
		public EquipmentOperationQuery(IRestAPI restAPI) : base(restAPI) { }

		public IEnumerable<EquipmentOperation> GetManyForEquipment(int equipmentId)
		{
			this.WithQuery<int>("equipmentId", equipmentId);
			return base.GetMany();
		}
	}
}
