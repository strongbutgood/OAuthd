using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	class ParameterGroupsOverview
	{
		public NonEntitySet<ParameterGroup> ParameterGroups { get; set; }

		#region ActionBar Buttons

		public ViewCommandResult Add(IRestAPI restAPI)
		{
			throw new NotImplementedException();
		}

		#endregion ActionBar Buttons

		#region ListView Buttons

		public ViewCommandResult Edit(IRestAPI restAPI, ParameterGroup parameterGroup)
		{
			throw new NotImplementedException();
		}

		public ViewCommandResult Delete(IRestAPI restAPI, ParameterGroup parameterGroup)
		{
			throw new NotImplementedException();
		}

		public ViewCommandResult ViewPermissions(IRestAPI restAPI, ParameterGroup parameterGroup)
		{
			throw new NotImplementedException();
		}

		#endregion ListView Buttons
	}
}