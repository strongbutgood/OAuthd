using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	class UnitsOfMeasureOverview
	{
		public NonEntitySet<UnitOfMeasure> UnitsOfMeasure { get; set; }
		
		#region ActionBar Buttons

		public ViewCommandResult Add(IRestAPI restAPI)
		{
			throw new NotImplementedException();
		}
		
		#endregion ActionBar Buttons

		#region ListView Buttons

		public ViewCommandResult Edit(IRestAPI restAPI, UnitOfMeasure unitOfMeasure)
		{
			throw new NotImplementedException();
		}

		public ViewCommandResult Delete(IRestAPI restAPI, UnitOfMeasure unitOfMeasure)
		{
			throw new NotImplementedException();
		}

		#endregion ListView Buttons

	}
}