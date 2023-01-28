using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ProcessMfg.Model;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using MATS.Module.RecipeManagerPlus.QueryBuilders;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the Equipment Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class EquipmentOverviewPage : ClientSummaryPage<Equipment>
	{
		/// <summary>Opens the <see cref="EquipmentOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static EquipmentOverviewPage Open(IRestAPI restAPI)
		{
			var equipmentQuery = restAPI.ForEquipment();
			var count = equipmentQuery.GetCount();
			var many = equipmentQuery.GetMany(includeCheckedOut: true);
			var page = new EquipmentOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="EquipmentOverviewPage"/> instance for the given <paramref name="equipment"/>.</summary>
		/// <param name="equipment">The equipment to be displayed on this page.</param>
		private EquipmentOverviewPage(IEnumerable<Equipment> equipment) : base(equipment) { }

		#region ActionBar Buttons

		/// <summary>Adds a new equipment to the Recipe Manager Plus system and opens the <see cref="EquipmentDetailsPage"/> for the added equipment.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <returns>A <see cref="EquipmentDetailsPage"/> with the added equipment.</returns>
		public EquipmentDetailsPage Add(IRestAPI restAPI)
		{
			var equipmentQuery = restAPI.ForEquipment();
			var formula = equipmentQuery.Add();
			return (EquipmentDetailsPage)this.Select(restAPI, formula);
		}

		#endregion ActionBar Buttons

		/// <summary>Opens the detail page for the selected <paramref name="equipment"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="equipment">The equipment to open the detail page for.</param>
		public override ClientDetailPage<Equipment> Select(IRestAPI restAPI, Equipment equipment)
		{
			return EquipmentDetailsPage.Open(restAPI, equipment);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public new EquipmentDetailsPage Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			return (EquipmentDetailsPage)base.Select(restAPI, id, anyVersion);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public new EquipmentDetailsPage Select(IRestAPI restAPI, string name)
		{
			return (EquipmentDetailsPage)base.Select(restAPI, name);
		}
	}

	/// <summary>
	/// Represents the Tabs of the Equipment Details page.
	/// </summary>
	enum EquipmentDetailsTabs
	{
		/// <summary>The capabilities tab.</summary>
		Capabilities,
		/// <summary>The parameters tab.</summary>
		Parameters,
		/// <summary>The variables tab.</summary>
		Variables,
		/// <summary>The users/groups tab.</summary>
		UsersGroups,
	}

	/// <summary>
	/// Represents the Equipment Details page for a specific <see cref="Equipment"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class EquipmentDetailsPage : ClientDetailPage<Equipment>
	{
		/// <summary>Opens the <see cref="EquipmentDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="equipment">The <see cref="Equipment"/> to get details for.</param>
		/// <param name="openWithTab">The tab to select when opening the client simulator page.</param>
		public static EquipmentDetailsPage Open(IRestAPI restAPI, Equipment equipment, EquipmentDetailsTabs openWithTab = EquipmentDetailsTabs.Capabilities)
		{
			var page = new EquipmentDetailsPage(equipment)
			{
				Operations = new EntitySet<EquipmentOperation>(),
				ParameterGroups = new NonEntitySet<ParameterGroup>(i => i.Id),
				Parameters = new EntitySet<EquipmentParameter>(),
				Variables = new EntitySet<EquipmentVariable>(),
				UserGroups = new NonEntitySet<UserGroup>(i => i.Id),
				Tab = openWithTab,
			};
			page.RefreshForCurrentTab(restAPI);
			return page;
		}

		private bool _capabilitiesLoaded;
		private bool _parametersLoaded;
		private bool _variablesLoaded;
		private bool _usersGroupsLoaded;

		/// <summary>Gets the capabilities associated with the displayed equipment.</summary>
		public EntitySet<EquipmentOperation> Operations { get; set; }
		/// <summary>Gets the parameter groups configured.</summary>
		public NonEntitySet<ParameterGroup> ParameterGroups { get; private set; }
		/// <summary>Gets the equipment parameters of the displayed equipment.</summary>
		public EntitySet<EquipmentParameter> Parameters { get; set; }
		/// <summary>Gets the equipment variables of the displayed equipment.</summary>
		public EntitySet<EquipmentVariable> Variables { get; set; }
		/// <summary>Gets the users and groups that can access the displayed equipment.</summary>
		public NonEntitySet<UserGroup> UserGroups { get; set; }

		/// <summary>Gets the tab currently in view.</summary>
		public EquipmentDetailsTabs Tab { get; private set; }

		/// <summary>Creates a new <see cref="EquipmentDetailsPage"/> instance for the given <paramref name="equipment"/>.</summary>
		/// <param name="equipment">The equipment to be displayed on this page.</param>
		private EquipmentDetailsPage(Equipment equipment) : base(equipment) { }

		#region Action Buttons

		/// <summary>Checks out the equipment.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var result = base.CheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.LoadVariables(restAPI);
			this.LoadUsersGroups(restAPI);
			this.LoadCapabilities(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Undos the equipment check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var result = base.UndoCheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.LoadVariables(restAPI);
			this.LoadUsersGroups(restAPI);
			this.LoadCapabilities(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Checks in the equipment and its changes.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckIn(IRestAPI restAPI)
		{
			var result = base.CheckIn(restAPI);
			if (!this.Entity.CheckedOut)
			{
				this.LoadParameters(restAPI);
				this.LoadVariables(restAPI);
				this.LoadUsersGroups(restAPI);
				this.LoadCapabilities(restAPI);
				this.RefreshForCurrentTab(restAPI);
			}
			return result;
		}

		/// <summary>Sets the approval status of the equipment.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the equipment.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public override ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var result = base.SetState(restAPI, newState, comment);
			this.LoadParameters(restAPI);
			this.LoadVariables(restAPI);
			this.LoadUsersGroups(restAPI);
			this.LoadCapabilities(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Upgrades the equipment to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult Upgrade(IRestAPI restAPI)
		{
			var result = base.Upgrade(restAPI);
			this.LoadParameters(restAPI);
			this.LoadVariables(restAPI);
			this.LoadUsersGroups(restAPI);
			this.LoadCapabilities(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		public ViewCommandResult PropogateStateClicked(IRestAPI restAPI, bool propagate)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Sub Page Buttons

		/// <summary>Switches to the <see cref="EquipmentDetailsTabs.Capabilities"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void CapabilitiesTab(IRestAPI restAPI)
		{
			this.RefreshForCapabilitiesTab(restAPI);
			this.Tab = EquipmentDetailsTabs.Capabilities;
		}

		/// <summary>Switches to the <see cref="EquipmentDetailsTabs.Parameters"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ParametersTab(IRestAPI restAPI)
		{
			this.RefreshForParametersTab(restAPI);
			this.Tab = EquipmentDetailsTabs.Parameters;
		}

		/// <summary>Switches to the <see cref="EquipmentDetailsTabs.Variables"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void VariablesTab(IRestAPI restAPI)
		{
			this.RefreshForVariablesTab(restAPI);
			this.Tab = EquipmentDetailsTabs.Variables;
		}

		/// <summary>Switches to the <see cref="EquipmentDetailsTabs.UsersGroups"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void UsersGroupsTab(IRestAPI restAPI)
		{
			this.RefreshForUsersGroupsTab(restAPI);
			this.Tab = EquipmentDetailsTabs.UsersGroups;
		}

		#endregion

		/// <summary>Provides a query builder for working with <see cref="Equipment"/> entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected override IVersionedQueryBuilder<Equipment> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForEquipment();
		}

		private void RefreshForCurrentTab(IRestAPI restAPI)
		{
			switch (this.Tab)
			{
				case EquipmentDetailsTabs.Capabilities:
					this.RefreshForParametersTab(restAPI);
					break;
				case EquipmentDetailsTabs.Parameters:
					this.RefreshForParametersTab(restAPI);
					break;
				case EquipmentDetailsTabs.Variables:
					this.RefreshForVariablesTab(restAPI);
					break;
				case EquipmentDetailsTabs.UsersGroups:
					this.RefreshForParametersTab(restAPI);
					break;
				default:
					break;
			}
		}

		private void LoadCapabilities(IRestAPI restAPI)
		{
			if (!this._capabilitiesLoaded)
			{
				var equipmentOperationsQuery = restAPI.ForEquipmentOperation();
				// get all equipment operations
				this.Operations.Clear();
				this.Operations.AddRange(equipmentOperationsQuery.GetManyForEquipment(this.Entity.Id));
				this._capabilitiesLoaded = true;
			}
		}

		private void LoadParameters(IRestAPI restAPI)
		{
			if (!this._parametersLoaded)
			{
				var parameterGroupsQuery = restAPI.ForParameterGroup();
				var equipmentParameterQuery = restAPI.ForEquipmentParameter();
				// get parameter groups
				this.ParameterGroups.Clear();
				this.ParameterGroups.AddRange(parameterGroupsQuery.GetMany(includeNull: true, includeDeleted: true));
				// get equipment parameters
				this.Parameters.Clear();
				this.Parameters.AddRange(equipmentParameterQuery.GetManyForEquipment(this.Entity.Id));
				this._parametersLoaded = true;
			}
		}

		private void LoadVariables(IRestAPI restAPI)
		{
			if (!this._variablesLoaded)
			{
				var equipmentVariablesQuery = restAPI.ForEquipmentVariable();
				// get all equipment variables
				this.Variables.Clear();
				this.Variables.AddRange(equipmentVariablesQuery.GetManyForEquipment(this.Entity.Id));
				this._variablesLoaded = true;
			}
		}

		private void LoadUsersGroups(IRestAPI restAPI)
		{
			if (!this._usersGroupsLoaded)
			{
				var userGroupsQuery = restAPI.ForUserGroup();
				// get all user groups
				this.UserGroups.Clear();
				this.UserGroups.AddRange(userGroupsQuery.GetManyForEquipment(this.Entity.RootId));
				this._usersGroupsLoaded = true;
			}
		}

		private void RefreshForCapabilitiesTab(IRestAPI restAPI)
		{
			this.LoadCapabilities(restAPI);
		}

		private void RefreshForParametersTab(IRestAPI restAPI)
		{
			this.LoadParameters(restAPI);
			var equipmentParameterQuery = restAPI.ForEquipmentParameter();
			// get equipment parameters count
			var count = equipmentParameterQuery.GetCountForEquipment(this.Entity.Id);
		}

		private void RefreshForVariablesTab(IRestAPI restAPI)
		{
			this.LoadVariables(restAPI);
			var equipmentVariablesQuery = restAPI.ForEquipmentVariable();
			// get equipment variables count
			var count = equipmentVariablesQuery.GetCountForEquipment(this.Entity.Id);
		}

		private void RefreshForUsersGroupsTab(IRestAPI restAPI)
		{
			this.LoadUsersGroups(restAPI);
			var userGroupsQuery = restAPI.ForUserGroup();
			// get users/groups count
			var count = userGroupsQuery.GetCountForEquipment(this.Entity.RootId);
		}
	}
}