using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	abstract class CapabilityOverview : ClientSummaryPage<Capability>
	{
		private CapabilityOverview(IEnumerable<Capability> capabilities) : base(capabilities) { }

		void AddClicked() { }
		void CapabilityClicked() { }
	}

	abstract class CapabilityDetails : ClientDetailPage<Capability>
	{
		public List<EquipmentParameter> Parameters { get; set; }
		public List<IControlStatusEntity> ControlStatuses { get; set; }
		public List<Equipment> Equipment { get; set; }

		private CapabilityDetails(Capability capability) : base(capability) { }

		void ParametersTabClicked() { }
		void ControlStatusTabClicked() { }
		void EquipmentTabClicked() { }

		void AckOnExit() { }
	}
}