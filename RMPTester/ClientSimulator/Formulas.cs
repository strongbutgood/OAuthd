using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Common.ERP;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the Formula Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class FormulaOverviewPage : ClientSummaryPage<Formula>
	{
		public void SetViewFilter(IRestAPI restAPI, StateViewTypes viewFilter)
		{
			var formulaQuery = restAPI.ForFormula();
			var count = formulaQuery.GetCount();
			var many = formulaQuery.GetMany(includeCheckedOut: true, viewFilter: viewFilter, upgradeableOnlyFilter: null, checkedOutFilter: null);
			this.Entities = this.CreateEntitySet(many);
		}

		/// <summary>Opens the <see cref="FormulaOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static FormulaOverviewPage Open(IRestAPI restAPI)
		{
			var formulaQuery = restAPI.ForFormula();
			var count = formulaQuery.GetCount();
			var many = formulaQuery.GetMany(includeCheckedOut: true, viewFilter: null, upgradeableOnlyFilter: null, checkedOutFilter: null);
			var page = new FormulaOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="FormulaOverviewPage"/> instance for the given <paramref name="formulas"/>.</summary>
		/// <param name="formulas">The formulas to be displayed on this page.</param>
		private FormulaOverviewPage(IEnumerable<Formula> formulas) : base(formulas) { }

		#region ActionBar Buttons

		/// <summary>Adds a new formula to the Recipe Manager Plus system with a formula template obtained from the <paramref name="selector"/> and opens the <see cref="FormulaDetailsPage"/> for the added formula.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="selector">A function to select which formula template to base the new formula on.</param>
		/// <returns>A <see cref="FormulaDetailsPage"/> with the added formula.</returns>
		public FormulaDetailsPage Add(IRestAPI restAPI, Func<IEnumerable<FormulaTemplate>, FormulaTemplate> selector)
		{
			var formulaTemplateQuery = restAPI.ForFormulaTemplate();
			var formulaTemplate = selector(formulaTemplateQuery.GetMany(includeCheckedOut: false));
			var formulaQuery = restAPI.ForFormula();
			var formula = formulaQuery.Add(formulaTemplate.Id);
			return (FormulaDetailsPage)this.Select(restAPI, formula);
		}
		/// <summary>Adds a new formula to the Recipe Manager Plus system based on the specified formula <paramref name="template"/> and opens the <see cref="FormulaDetailsPage"/> for the added formula.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="template">The formula template to base the new formula on.</param>
		/// <returns>A <see cref="FormulaDetailsPage"/> with the added formula.</returns>
		public FormulaDetailsPage Add(IRestAPI restAPI, FormulaTemplate template)
		{
			return this.Add(restAPI, ts => ts.FirstOrDefault(t => t.Id == template.Id));
		}

		#endregion ActionBar Buttons

		/// <summary>Opens the detail page for the selected <paramref name="formula"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="formula">The formula to open the detail page for.</param>
		public override ClientDetailPage<Formula> Select(IRestAPI restAPI, Formula formula)
		{
			return FormulaDetailsPage.Open(restAPI, formula);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public new FormulaDetailsPage Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			return (FormulaDetailsPage)base.Select(restAPI, id, anyVersion);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public new FormulaDetailsPage Select(IRestAPI restAPI, string name)
		{
			return (FormulaDetailsPage)base.Select(restAPI, name);
		}
	}

	/// <summary>
	/// Represents the Tabs of the Formula Details page.
	/// </summary>
	enum FormulaDetailsTabs
	{
		/// <summary>The parameters tab.</summary>
		Parameters,
		/// <summary>The equipment tab.</summary>
		Equipment,
	}

	/// <summary>
	/// Represents the Formula Details page for a specific <see cref="Formula"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class FormulaDetailsPage : ClientDetailPage<Formula>
	{
		/// <summary>Opens the <see cref="FormulaDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="formula">The <see cref="Formula"/> to get details for.</param>
		/// <param name="openWithTab">The tab to select when opening the client simulator page.</param>
		public static FormulaDetailsPage Open(IRestAPI restAPI, Formula formula, FormulaDetailsTabs openWithTab = FormulaDetailsTabs.Parameters)
		{
			var page = new FormulaDetailsPage(formula)
			{
				UnitsOfMeasure = new NonEntitySet<UnitOfMeasure>(i => i.Id),
				ParameterExtDefs = new NonEntitySet<ParameterExtDef>(i => i.Id),
				ParameterGroups = new NonEntitySet<ParameterGroup>(i => i.Id),
				Parameters = new EntitySet<FormulaParameter>(),
				Equipments = new EntitySet<Equipment>(),
				FormulaEquipments = new List<FormulaEquipment>(),
				Tab = openWithTab,
			};
			page.RefreshForCurrentTab(restAPI);
			return page;
		}

		private bool _parametersLoaded;
		private bool _equipmentLoaded;

		/// <summary>Gets the units of measures for formula parameters. This is loaded with the <see cref="FormulaDetailsTabs.Parameters"/> tab.</summary>
		public NonEntitySet<UnitOfMeasure> UnitsOfMeasure { get; private set; }
		/// <summary>Gets the parameter extension definitions of the displayed formula. This is loaded with the <see cref="FormulaDetailsTabs.Parameters"/> tab.</summary>
		public NonEntitySet<ParameterExtDef> ParameterExtDefs { get; private set; }
		/// <summary>Gets the parameter groups configured. This is loaded with the <see cref="FormulaDetailsTabs.Parameters"/> tab.</summary>
		public NonEntitySet<ParameterGroup> ParameterGroups { get; private set; }
		/// <summary>Gets the formula parameters of the displayed formula. This is loaded with the <see cref="FormulaDetailsTabs.Parameters"/> tab.</summary>
		public EntitySet<FormulaParameter> Parameters { get; private set; }
		/// <summary>Gets the equipments configured. This is loaded with the <see cref="FormulaDetailsTabs.Equipment"/> tab.</summary>
		public EntitySet<Equipment> Equipments { get; private set; }
		/// <summary>Gets the formula equipments for the displayed formula. This is loaded with the <see cref="FormulaDetailsTabs.Equipment"/> tab.</summary>
		public List<FormulaEquipment> FormulaEquipments { get; private set; }

		/// <summary>Gets the tab currently in view.</summary>
		public FormulaDetailsTabs Tab { get; private set; }

		/// <summary>Creates a new <see cref="FormulaDetailsPage"/> instance for the given <paramref name="formula"/>.</summary>
		/// <param name="formula">The formula to be displayed on this page.</param>
		private FormulaDetailsPage(Formula formula) : base(formula) { }

		#region Action Buttons

		/// <summary>Checks out the formula.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var result = base.CheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Undos the formula check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var result = base.UndoCheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Checks in the formula and its changes.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckIn(IRestAPI restAPI)
		{
			var result = base.CheckIn(restAPI);
			if (!this.Entity.CheckedOut)
			{
				this.LoadParameters(restAPI);
				this.RefreshForCurrentTab(restAPI);
			}
			return result;
		}

		/// <summary>Sets the approval status of the formula.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the formula.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public override ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var result = base.SetState(restAPI, newState, comment);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Upgrades the formula to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult Upgrade(IRestAPI restAPI)
		{
			var result = base.Upgrade(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		public ViewCommandResult SetUse(IRestAPI restAPI)
		{
			throw new NotImplementedException();
		}

		public ViewCommandResult Validate(IRestAPI restAPI)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region GridView Buttons

		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, bool useDefault)
		{
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			parameter = this.Parameters[parameter.Id];
			parameter.IsTemplateData = useDefault;
			this.Parameters[parameter.Id] = formulaParameterQuery.Change(parameter);
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}

		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, string targetValue, string minValue = null, string maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, IDictionary<string, string> extValues = null)
		{
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			parameter = this.Parameters[parameter.Id];
			parameter.IsTemplateData = false;
			parameter.TargetValue = targetValue;
			if (minValue != null)
				parameter.MinValue = minValue;
			if (maxValue != null)
				parameter.MaxValue = maxValue;
			if (freqUsed.HasValue)
				parameter.FrequentlyUsed = freqUsed.Value;
			if (uom != null)
				parameter.UnitOfMeasureId = uom.Id;
			if (extValues != null)
			{
				foreach (var kvp in extValues)
				{
					var extDef = this.ParameterExtDefs.FirstOrDefault(e => string.Equals(e.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
					if (extDef != null)
					{
						var extValue = parameter.FormulaParameterExts.FirstOrDefault(e => e.ParameterExtDefId == extDef.Id);
						if (extValue != null)
							extValue.Value = kvp.Value;
					}
				}
			}
			this.Parameters[parameter.Id] = formulaParameterQuery.Change(parameter);
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, bool targetValue, bool? freqUsed = null, UnitOfMeasure uom = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, targetValue.ToString(), null, null, freqUsed, uom, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, long targetValue, long? minValue = null, long? maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, targetValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, double targetValue, double? minValue = null, double? maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, targetValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, FormulaParameterValue targetValue, bool? freqUsed = null, UnitOfMeasure uom = null, IDictionary<string, string> extValues = null)
		{
			if (targetValue != null)
				return this.Update(restAPI, parameter, targetValue.Value, targetValue.MinValue, targetValue.MaxValue, freqUsed, uom, extValues);
			else
				return this.Update(restAPI, parameter, null, null, null, freqUsed, uom, extValues);
		}

		#endregion GridView Buttons

		#region Sub Page Buttons

		/// <summary>Switches to the <see cref="FormulaDetailsTabs.Parameters"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void ParametersTab(IRestAPI restAPI)
		{
			this.RefreshForParametersTab(restAPI);
			this.Tab = FormulaDetailsTabs.Parameters;
		}

		/// <summary>Switches to the <see cref="FormulaDetailsTabs.Equipment"/> tab.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public void EquipmentTab(IRestAPI restAPI)
		{
			this.RefreshForEquipmentTab(restAPI);
			this.Tab = FormulaDetailsTabs.Equipment;
		}

		#endregion

		/// <summary>Provides a query builder for working with <see cref="Formula"/> entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected override IVersionedQueryBuilder<Formula> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForFormula();
		}

		private void RefreshForCurrentTab(IRestAPI restAPI)
		{
			if (this.Tab == FormulaDetailsTabs.Parameters)
				this.RefreshForParametersTab(restAPI);
			else
				this.RefreshForEquipmentTab(restAPI);
		}

		private void LoadParameters(IRestAPI restAPI)
		{
			if (!this._parametersLoaded)
			{
				var unitsOfMeasureQuery = restAPI.ForUnitOfMeasure();
				var parameterGroupsQuery = restAPI.ForParameterGroup();
				var formulaParameterQuery = restAPI.ForFormulaParameter();
				// get units of measures
				this.UnitsOfMeasure.Clear();
				this.UnitsOfMeasure.AddRange(unitsOfMeasureQuery.GetMany(includeNull: true, includeDeleted: true));
				// get parameter groups
				this.ParameterGroups.Clear();
				this.ParameterGroups.AddRange(parameterGroupsQuery.GetMany(includeNull: true, includeDeleted: true));
				// get parameter extension definitions
				this.ParameterExtDefs.Clear();
				this.ParameterExtDefs.AddRange(formulaParameterQuery.GetInstanceParameterExtDefs(this.Entity.Id));
				// get formula parameters
				this.Parameters.Clear();
				this.Parameters.AddRange(formulaParameterQuery.GetManyForFormula(this.Entity.Id));
				this._parametersLoaded = true;
			}
		}

		private void LoadEquipment(IRestAPI restAPI)
		{
			if (!this._equipmentLoaded)
			{
				var equipmentQuery = restAPI.ForEquipment();
				var formulaEquipmentsQuery = restAPI.ForFormulaEquipment();
				// get all equipments
				this.Equipments.Clear();
				this.Equipments.AddRange(equipmentQuery.GetMany(includeCheckedOut: false));
				// get all formula equipments
				this.FormulaEquipments.Clear();
				this.FormulaEquipments.AddRange(formulaEquipmentsQuery.GetManyForFormula(this.Entity.Id));
				this._equipmentLoaded = true;
			}
		}

		private void RefreshForParametersTab(IRestAPI restAPI)
		{
			this.LoadParameters(restAPI);
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);

			// relate some things??
			foreach (var item in this.ParameterExtDefs)
			{
				if (item.ParameterGroupId != null)
				{
					item.ParameterGroup = this.ParameterGroups.SingleOrDefault(pg => pg.Id == item.ParameterGroupId.Value);
				}
			}
		}

		private void RefreshForEquipmentTab(IRestAPI restAPI)
		{
			this.LoadEquipment(restAPI);
			var equipmentQuery = restAPI.ForEquipment();
			// get formula equipments count
			var count = equipmentQuery.GetCount(includeCheckedOut: false);
		}
	}
}
