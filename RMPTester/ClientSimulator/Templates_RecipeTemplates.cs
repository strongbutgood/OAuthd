using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the Recipe Template Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class RecipeTemplateOverviewPage : ClientSummaryPage<RecipeTemplate>
	{
		/// <summary>Opens the <see cref="RecipeTemplateOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static RecipeTemplateOverviewPage Open(IRestAPI restAPI)
		{
			var recipeQuery = restAPI.ForRecipeTemplate();
			var count = recipeQuery.GetCount();
			var many = recipeQuery.GetMany(includeCheckedOut: true);
			var page = new RecipeTemplateOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="RecipeTemplateOverviewPage"/> instance for the given <paramref name="recipeTemplates"/>.</summary>
		/// <param name="recipeTemplates">The recipe templates to be displayed on this page.</param>
		private RecipeTemplateOverviewPage(IEnumerable<RecipeTemplate> recipeTemplates) : base(recipeTemplates) { }

		#region ActionBar Buttons

		/// <summary>Adds a new recipe template to the Recipe Manager Plus system and opens the <see cref="RecipeTemplateDetailsPage"/> for the added recipe template.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <returns>A <see cref="RecipeTemplateDetailsPage"/> with the added recipe template.</returns>
		public RecipeTemplateDetailsPage Add(IRestAPI restAPI)
		{
			var recipeTemplateQuery = restAPI.ForRecipeTemplate();
			var recipe = recipeTemplateQuery.Add();
			return (RecipeTemplateDetailsPage)this.Select(restAPI, recipe);
		}

		#endregion ActionBar Buttons

		/// <summary>Opens the detail page for the selected <paramref name="recipeTemplate"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="recipeTemplate">The recipe template to open the detail page for.</param>
		public override ClientDetailPage<RecipeTemplate> Select(IRestAPI restAPI, RecipeTemplate recipeTemplate)
		{
			return RecipeTemplateDetailsPage.Open(restAPI, recipeTemplate);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public new RecipeTemplateDetailsPage Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			return (RecipeTemplateDetailsPage)base.Select(restAPI, id, anyVersion);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public new RecipeTemplateDetailsPage Select(IRestAPI restAPI, string name)
		{
			return (RecipeTemplateDetailsPage)base.Select(restAPI, name);
		}
	}

	/// <summary>
	/// Represents the Tabs of the Recipe Template Details page.
	/// </summary>
	enum RecipeTemplateDetailsTabs
	{
		/// <summary>The procedure tab.</summary>
		Procedure,
		/// <summary>The parameter map tab.</summary>
		ParameterMap,
	}

	/// <summary>
	/// Represents the Recipe Template Details page for a specific <see cref="RecipeTemplate"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class RecipeTemplateDetailsPage : ClientDetailPage<RecipeTemplate>
	{
		/// <summary>Opens the <see cref="RecipeTemplateDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="recipeTemplate">The <see cref="RecipeTemplate"/> to get details for.</param>
		/// <param name="openWithTab">The tab to select when opening the client simulator page.</param>
		public static RecipeTemplateDetailsPage Open(IRestAPI restAPI, RecipeTemplate recipeTemplate, RecipeTemplateDetailsTabs openWithTab = RecipeTemplateDetailsTabs.Procedure)
		{
			var page = new RecipeTemplateDetailsPage(recipeTemplate)
			{
				Capabilities = new EntitySet<Capability>(),
				ProcedureTokens = new NonEntitySet<ProcedureToken>(i => i.Id),
				ParameterGroups = new NonEntitySet<ParameterGroup>(i => i.Id),
				ParameterMaps = new NonEntitySet<RecipeParameterMap>(i => i.Id),
				Tab = openWithTab,
			};
			page.RefreshForCurrentTab(restAPI);
			return page;
		}

		private bool _procedureLoaded;
		private bool _parameterMapLoaded;

		/// <summary>Gets the capabilities available to the displayed recipe template. This is loaded with the <see cref="RecipeTemplateDetailsTabs.Procedure"/> tab.</summary>
		public EntitySet<Capability> Capabilities { get; private set; }
		/// <summary>Gets the procedure tokens of the displayed recipe template. This is loaded with the <see cref="RecipeTemplateDetailsTabs.Procedure"/> tab.</summary>
		public NonEntitySet<ProcedureToken> ProcedureTokens { get; private set; }
		/// <summary>Gets the parameter groups configured. This is loaded with the <see cref="RecipeTemplateDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<ParameterGroup> ParameterGroups { get; private set; }
		/// <summary>Gets the recipe parameter maps of the displayed recipe template. This is loaded with the <see cref="RecipeTemplateDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<RecipeParameterMap> ParameterMaps { get; private set; }

		/// <summary>Gets the tab currently in view.</summary>
		public RecipeTemplateDetailsTabs Tab { get; private set; }

		/// <summary>Creates a new <see cref="RecipeTemplateDetailsPage"/> instance for the given <paramref name="recipeTemplate"/>.</summary>
		/// <param name="recipeTemplate">The recipe template to be displayed on this page.</param>
		private RecipeTemplateDetailsPage(RecipeTemplate recipeTemplate) : base(recipeTemplate) { }

		#region Action Buttons

		/// <summary>Checks out the recipe template.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var result = base.CheckOut(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Undos the recipe template check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var result = base.UndoCheckOut(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Checks in the recipe template and its changes.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckIn(IRestAPI restAPI)
		{
			var result = base.CheckIn(restAPI);
			if (!this.Entity.CheckedOut)
			{
				this.RefreshForCurrentTab(restAPI);
			}
			return result;
		}

		/// <summary>Sets the approval status of the recipe template.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the recipe template.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public override ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var result = base.SetState(restAPI, newState, comment);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Upgrades the recipe template to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult Upgrade(IRestAPI restAPI)
		{
			var result = base.Upgrade(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		public ViewCommandResult Validate(IRestAPI restAPI, Func<IEnumerable<Equipment>, Equipment> selector)
		{
			var equipmentQuery = restAPI.ForEquipment();
			var equipment = selector(equipmentQuery.GetMany(includeCheckedOut: null));
			var recipeTemplateQuery = restAPI.ForRecipeTemplate();
			var result = recipeTemplateQuery.ValidateForEquipment(this.Entity.Id, equipment.Id);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}
		public ViewCommandResult Validate(IRestAPI restAPI, Equipment equipment)
		{
			return this.Validate(restAPI, es => es.FirstOrDefault(e => e.Id == equipment.Id));
		}

		public ViewCommandResult SetFormulaTemplate(IRestAPI restAPI, Func<IEnumerable<FormulaTemplate>, FormulaTemplate> selector)
		{
			throw new NotImplementedException();
		}
		public ViewCommandResult SetFormulaTemplate(IRestAPI restAPI, FormulaTemplate formulaTemplate)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Sub Page Buttons

		/// <summary>Switches to the <see cref="RecipeTemplateDetailsTabs.Procedure"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ProcedureTab(IRestAPI restAPI)
		{
			this.RefreshForProcedureTab(restAPI);
			this.Tab = RecipeTemplateDetailsTabs.Procedure;
		}
		/// <summary>Switches to the <see cref="RecipeTemplateDetailsTabs.ParameterMap"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ParameterMapTab(IRestAPI restAPI)
		{
			this.RefreshForParameterMapTab(restAPI);
			this.Tab = RecipeTemplateDetailsTabs.ParameterMap;
		}

		#endregion

		/// <summary>Provides a query builder for working with <see cref="RecipeTemplate"/> entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected override IVersionedQueryBuilder<RecipeTemplate> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForRecipeTemplate();
		}

		private void RefreshForCurrentTab(IRestAPI restAPI)
		{
			if (this.Tab == RecipeTemplateDetailsTabs.Procedure)
				this.RefreshForProcedureTab(restAPI);
			else
				this.RefreshForParameterMapTab(restAPI);
		}

		private void LoadProcedure(IRestAPI restAPI)
		{
			if (!this._procedureLoaded)
			{
				var capabilityQuery = restAPI.ForCapability();
				var procedureTokenQuery = restAPI.ForProcedureToken();
				// get capabilities
				this.Capabilities.Clear();
				this.Capabilities.AddRange(capabilityQuery.GetManyForProcedure(this.Entity.ProcedureId.GetValueOrDefault()));
				// get procedure tokens
				this.ProcedureTokens.Clear();
				this.ProcedureTokens.AddRange(procedureTokenQuery.GetManyForProcedure(this.Entity.ProcedureId.GetValueOrDefault()));
				this._procedureLoaded = true;
			}
		}

		private void LoadParameterMap(IRestAPI restAPI)
		{
			if (!this._parameterMapLoaded)
			{
				var parameterGroupsQuery = restAPI.ForParameterGroup();
				var formulaParameterQuery = restAPI.ForFormulaParameter();
				var recipeParameterMapQuery = restAPI.ForRecipeParameterMap();
				// get parameter groups
				this.ParameterGroups.Clear();
				this.ParameterGroups.AddRange(parameterGroupsQuery.GetMany(includeNull: true, includeDeleted: true));
				// get recipe parameter maps
				this.ParameterMaps.Clear();
				this.ParameterMaps.AddRange(recipeParameterMapQuery.GetManyForRecipeTemplate(this.Entity.Id));
				this._parameterMapLoaded = true;
			}
		}

		private void RefreshForProcedureTab(IRestAPI restAPI)
		{
			this.LoadProcedure(restAPI);
		}

		private void RefreshForParameterMapTab(IRestAPI restAPI)
		{
			this.LoadParameterMap(restAPI);
			this.LoadProcedure(restAPI);
			var recipeParameterMapQuery = restAPI.ForRecipeParameterMap();
			var recipeTemplateQuery = restAPI.ForRecipeTemplate();
			// get recipe parameter maps count
			var count = recipeParameterMapQuery.GetCountForRecipeTemplate(this.Entity.Id);
			// get recipe template
			this.Entity = recipeTemplateQuery.GetOne(this.Entity.Id);
		}
	}
}
