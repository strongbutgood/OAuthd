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
	/// Represents the Recipe Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class RecipeOverviewPage : ClientSummaryPage<Recipe>
	{
		public void SetViewFilter(IRestAPI restAPI, StateViewTypes viewFilter)
		{
			var recipeQuery = restAPI.ForRecipe();
			var count = recipeQuery.GetCount();
			var many = recipeQuery.GetMany(includeCheckedOut: true, viewFilter: viewFilter, upgradeableOnlyFilter: null, checkedOutFilter: null);
			this.Entities = this.CreateEntitySet(many);
		}

		/// <summary>Opens the <see cref="RecipeOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static RecipeOverviewPage Open(IRestAPI restAPI)
		{
			var recipeQuery = restAPI.ForRecipe();
			var count = recipeQuery.GetCount();
			var many = recipeQuery.GetMany(includeCheckedOut: true);
			var page = new RecipeOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="RecipeOverviewPage"/> instance for the given <paramref name="recipes"/>.</summary>
		/// <param name="recipes">The recipes to be displayed on this page.</param>
		private RecipeOverviewPage(IEnumerable<Recipe> recipes) : base(recipes) { }

		#region ActionBar Buttons

		/// <summary>Adds a new recipe to the Recipe Manager Plus system with a recipe template and formula obtained from the <paramref name="recipeTemplateSelector"/> and <paramref name="formulaSelector"/> and opens the <see cref="RecipeDetailsPage"/> for the added recipe.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="recipeTemplateSelector">A function to select which recipe template to base the new recipe on.</param>
		/// <param name="formulaSelector">A function to select which formula to provide parameters for the new recipe.</param>
		/// <returns>A <see cref="RecipeDetailsPage"/> with the added recipe.</returns>
		public RecipeDetailsPage Add(IRestAPI restAPI, Func<IEnumerable<RecipeTemplate>, RecipeTemplate> recipeTemplateSelector, Func<IEnumerable<Formula>, Formula> formulaSelector)
		{
			var recipeTemplateQuery = restAPI.ForRecipeTemplate();
			var recipeTemplate = recipeTemplateSelector(recipeTemplateQuery.GetMany(includeCheckedOut: false));
			if (!recipeTemplate.FormulaTemplateId.HasValue)
				throw new InvalidOperationException("Recipe Template must be assigned a Formula Template in order to create a Recipe.");
			var formulaQuery = restAPI.ForFormula();
			var formula = formulaSelector(formulaQuery.GetManyForRecipeTemplate(recipeTemplate.FormulaTemplateId.Value, getLatest: false, includeCheckedOut: false));
			var recipeQuery = restAPI.ForRecipe();
			var recipe = recipeQuery.Add(recipeTemplate.Id, formula.Id);
			return (RecipeDetailsPage)this.Select(restAPI, recipe);
		}
		/// <summary>Adds a new recipe to the Recipe Manager Plus system based on the specified recipe <paramref name="template"/> and <paramref name="formula"/> and opens the <see cref="RecipeDetailsPage"/> for the added recipe.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="template">The recipe template to base the new recipe on.</param>
		/// <param name="formula">The formula to provide parameters for the new recipe.</param>
		/// <returns>A <see cref="RecipeDetailsPage"/> with the added recipe.</returns>
		public RecipeDetailsPage Add(IRestAPI restAPI, RecipeTemplate template, Formula formula)
		{
			return this.Add(restAPI, ts => ts.FirstOrDefault(t => t.Id == template.Id), fs => fs.FirstOrDefault(f => f.Id == formula.Id));
		}

		#endregion ActionBar Buttons

		/// <summary>Opens the detail page for the selected <paramref name="recipe"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="recipe">The recipe to open the detail page for.</param>
		public override ClientDetailPage<Recipe> Select(IRestAPI restAPI, Recipe recipe)
		{
			return RecipeDetailsPage.Open(restAPI, recipe);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public new RecipeDetailsPage Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			return (RecipeDetailsPage)base.Select(restAPI, id, anyVersion);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public new RecipeDetailsPage Select(IRestAPI restAPI, string name)
		{
			return (RecipeDetailsPage)base.Select(restAPI, name);
		}
	}

	/// <summary>
	/// Represents the Tabs of the Recipe Details page.
	/// </summary>
	enum RecipeDetailsTabs
	{
		/// <summary>The procedure tab.</summary>
		Procedure,
		/// <summary>The parameter map tab.</summary>
		ParameterMap,
	}

	/// <summary>
	/// Represents the Recipe Details page for a specific <see cref="Recipe"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class RecipeDetailsPage : ClientDetailPage<Recipe>
	{
		/// <summary>Opens the <see cref="RecipeDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="recipe">The <see cref="Recipe"/> to get details for.</param>
		/// <param name="openWithTab">The tab to select when opening the client simulator page.</param>
		public static RecipeDetailsPage Open(IRestAPI restAPI, Recipe recipe, RecipeDetailsTabs openWithTab = RecipeDetailsTabs.Procedure)
		{
			var page = new RecipeDetailsPage(recipe)
			{
				ProcedureTokens = new NonEntitySet<ProcedureToken>(i => i.Id),
				UnitsOfMeasure = new NonEntitySet<UnitOfMeasure>(i => i.Id),
				ParameterExtDefs = new NonEntitySet<ParameterExtDef>(i => i.Id),
				ParameterGroups = new NonEntitySet<ParameterGroup>(i => i.Id),
				ParameterMaps = new NonEntitySet<RecipeParameterMap>(i => i.Id),
				Tab = openWithTab,
			};
			page.RefreshForCurrentTab(restAPI);
			return page;
		}

		private bool _procedureLoaded;
		private bool _parameterMapLoaded;

		/// <summary>Gets the template of the displayed recipe. This is loaded with the <see cref="RecipeDetailsTabs.Procedure"/> tab.</summary>
		public RecipeTemplate RecipeTemplate { get; private set; }
		/// <summary>Gets the procedure tokens of the displayed recipe. This is loaded with the <see cref="RecipeDetailsTabs.Procedure"/> tab.</summary>
		public NonEntitySet<ProcedureToken> ProcedureTokens { get; private set; }
		/// <summary>Gets the units of measures for recipe parameters. This is loaded with the <see cref="RecipeDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<UnitOfMeasure> UnitsOfMeasure { get; private set; }
		/// <summary>Gets the parameter extension definitions of the displayed recipe. This is loaded with the <see cref="RecipeDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<ParameterExtDef> ParameterExtDefs { get; private set; }
		/// <summary>Gets the parameter groups configured. This is loaded with the <see cref="RecipeDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<ParameterGroup> ParameterGroups { get; private set; }
		/// <summary>Gets the recipe parameter maps of the displayed recipe. This is loaded with the <see cref="RecipeDetailsTabs.ParameterMap"/> tab.</summary>
		public NonEntitySet<RecipeParameterMap> ParameterMaps { get; private set; }

		/// <summary>Gets the tab currently in view.</summary>
		public RecipeDetailsTabs Tab { get; private set; }

		/// <summary>Creates a new <see cref="RecipeDetailsPage"/> instance for the given <paramref name="recipe"/>.</summary>
		/// <param name="recipe">The recipe to be displayed on this page.</param>
		private RecipeDetailsPage(Recipe recipe) : base(recipe) { }

		#region Action Buttons

		/// <summary>Checks out the recipe.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var result = base.CheckOut(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Undos the recipe check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var result = base.UndoCheckOut(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Checks in the recipe and its changes.</summary>
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

		/// <summary>Sets the approval status of the recipe.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the recipe.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public override ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var result = base.SetState(restAPI, newState, comment);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Upgrades the recipe to match other versioned entities.</summary>
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
			var recipeQuery = restAPI.ForRecipe();
			var result = recipeQuery.ValidateForEquipment(this.Entity.Id, equipment.Id);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}
		public ViewCommandResult Validate(IRestAPI restAPI, Equipment equipment)
		{
			return this.Validate(restAPI, es => es.FirstOrDefault(e => e.Id == equipment.Id));
		}

		#endregion

		#region Sub Page Buttons

		/// <summary>Switches to the <see cref="RecipeDetailsTabs.Procedure"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ProcedureTab(IRestAPI restAPI)
		{
			this.RefreshForProcedureTab(restAPI);
			this.Tab = RecipeDetailsTabs.Procedure;
		}
		/// <summary>Switches to the <see cref="RecipeDetailsTabs.ParameterMap"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ParameterMapTab(IRestAPI restAPI)
		{
			this.RefreshForParameterMapTab(restAPI);
			this.Tab = RecipeDetailsTabs.ParameterMap;
		}

		#endregion

		/// <summary>Provides a query builder for working with <see cref="Recipe"/> entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected override IVersionedQueryBuilder<Recipe> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForRecipe();
		}

		private void RefreshForCurrentTab(IRestAPI restAPI)
		{
			if (this.Tab == RecipeDetailsTabs.Procedure)
				this.RefreshForProcedureTab(restAPI);
			else
				this.RefreshForParameterMapTab(restAPI);
		}

		private void LoadProcedure(IRestAPI restAPI)
		{
			if (!this._procedureLoaded)
			{
				var recipeTemplateQuery = restAPI.ForRecipeTemplate();
				var procedureTokenQuery = restAPI.ForProcedureToken();
				// get recipe template
				this.RecipeTemplate = recipeTemplateQuery.GetOne(this.Entity.RecipeTemplateId);
				// get procedure tokens
				this.ProcedureTokens.Clear();
				this.ProcedureTokens.AddRange(procedureTokenQuery.GetManyForProcedure(this.RecipeTemplate.ProcedureId.GetValueOrDefault()));
				this._procedureLoaded = true;
			}
		}

		private void LoadParameterMap(IRestAPI restAPI)
		{
			if (!this._parameterMapLoaded)
			{
				var unitsOfMeasureQuery = restAPI.ForUnitOfMeasure();
				var parameterGroupsQuery = restAPI.ForParameterGroup();
				var formulaParameterQuery = restAPI.ForFormulaParameter();
				var recipeParameterMapQuery = restAPI.ForRecipeParameterMap();
				// get units of measures
				this.UnitsOfMeasure.Clear();
				this.UnitsOfMeasure.AddRange(unitsOfMeasureQuery.GetMany(includeNull: true, includeDeleted: true));
				// get parameter groups
				this.ParameterGroups.Clear();
				this.ParameterGroups.AddRange(parameterGroupsQuery.GetMany(includeNull: true, includeDeleted: true));
				// get parameter extension definitions
				this.ParameterExtDefs.Clear();
				this.ParameterExtDefs.AddRange(formulaParameterQuery.GetInstanceParameterExtDefs(this.Entity.Id));
				// get recipe parameter maps
				this.ParameterMaps.Clear();
				this.ParameterMaps.AddRange(recipeParameterMapQuery.GetManyForRecipe(this.Entity.Id));
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
			var recipeQuery = restAPI.ForRecipe();
			// get recipe parameter maps count
			var count = recipeParameterMapQuery.GetCountForRecipe(this.Entity.Id);
			// get recipe
			this.Entity = recipeQuery.GetOne(this.Entity.Id);
		}
	}
}
