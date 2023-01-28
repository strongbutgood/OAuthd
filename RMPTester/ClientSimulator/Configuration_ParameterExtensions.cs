using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	class ParameterExtensionsOverview
	{
		public NonEntitySet<ParameterGroup> ParameterGroups { get; set; }
		public NonEntitySet<ParameterExtDef> ParameterExtensionDefinitions { get; set; }

		#region ActionBar Buttons

		public ViewCommandResult Add(IRestAPI restAPI)
		{
			throw new NotImplementedException();
		}

		public void ExportClicked()
		{
			throw new NotImplementedException();
		}

		public void ImportClicked()
		{
			throw new NotImplementedException();
		}

		#endregion ActionBar Buttons

		#region ListView Buttons

		public ViewCommandResult Edit(IRestAPI restAPI, ParameterExtDef parameterExtension)
		{
			throw new NotImplementedException();
		}

		public ViewCommandResult Delete(IRestAPI restAPI, ParameterExtDef parameterExtension)
		{
			throw new NotImplementedException();
		}

		#endregion ListView Buttons
	}
}