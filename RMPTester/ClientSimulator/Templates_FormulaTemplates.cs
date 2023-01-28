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
	/// Represents the Formula Template Overview page in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class FormulaTemplateOverviewPage : ClientSummaryPage<FormulaTemplate>
	{
		/// <summary>Opens the <see cref="FormulaTemplateOverviewPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		public static FormulaTemplateOverviewPage Open(IRestAPI restAPI)
		{
			var formulaTemplateQuery = restAPI.ForFormulaTemplate();
			var count = formulaTemplateQuery.GetCount();
			var many = formulaTemplateQuery.GetMany(includeCheckedOut: true);
			var page = new FormulaTemplateOverviewPage(many);
			return page;
		}

		/// <summary>Creates a new <see cref="FormulaTemplateOverviewPage"/> instance for the given <paramref name="formulaTemplates"/>.</summary>
		/// <param name="formulaTemplates">The formula templates to be displayed on this page.</param>
		private FormulaTemplateOverviewPage(IEnumerable<FormulaTemplate> formulaTemplates) : base(formulaTemplates) { }

		#region ActionBar Buttons

		/// <summary>Adds a new formula to the Recipe Manager Plus system and opens the <see cref="FormulaTemplateDetailsPage"/> for the added formula template.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <returns>A <see cref="FormulaTemplateDetailsPage"/> with the added formula template.</returns>
		public FormulaTemplateDetailsPage Add(IRestAPI restAPI)
		{
			var formulaTemplateQuery = restAPI.ForFormulaTemplate();
			var formula = formulaTemplateQuery.Add();
			return (FormulaTemplateDetailsPage)this.Select(restAPI, formula);
		}

		#endregion ActionBar Buttons

		/// <summary>Opens the detail page for the selected <paramref name="formulaTemplate"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="formulaTemplate">The formula template to open the detail page for.</param>
		public override ClientDetailPage<FormulaTemplate> Select(IRestAPI restAPI, FormulaTemplate formulaTemplate)
		{
			return FormulaTemplateDetailsPage.Open(restAPI, formulaTemplate);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public new FormulaTemplateDetailsPage Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			return (FormulaTemplateDetailsPage)base.Select(restAPI, id, anyVersion);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public new FormulaTemplateDetailsPage Select(IRestAPI restAPI, string name)
		{
			return (FormulaTemplateDetailsPage)base.Select(restAPI, name);
		}
	}

	/// <summary>
	/// Represents the Formula Template Details page for a specific <see cref="FormulaTemplate"/> in Recipe Manager Plus and provides client like interactions for ease of understanding.
	/// </summary>
	class FormulaTemplateDetailsPage : ClientDetailPage<FormulaTemplate>
	{
		/// <summary>Opens the <see cref="FormulaTemplateDetailsPage"/> page using the given <paramref name="restAPI"/>.</summary>
		/// <param name="restAPI">The RESTful API for interacting with Recipe Manager Plus server.</param>
		/// <param name="formulaTemplate">The <see cref="FormulaTemplate"/> to get details for.</param>
		public static FormulaTemplateDetailsPage Open(IRestAPI restAPI, FormulaTemplate formulaTemplate)
		{
			var page = new FormulaTemplateDetailsPage(formulaTemplate)
			{
				ParameterExtDefs = new NonEntitySet<ParameterExtDef>(i => i.Id),
				ParameterGroups = new NonEntitySet<ParameterGroup>(i => i.Id),
				Parameters = new EntitySet<FormulaParameter>(),
			};
			page.RefreshForCurrentTab(restAPI);
			return page;
		}

		private bool _parametersLoaded;

		/// <summary>Gets the parameter extension definitions of the displayed formula.</summary>
		public NonEntitySet<ParameterExtDef> ParameterExtDefs { get; private set; }
		/// <summary>Gets the parameter groups configured.</summary>
		public NonEntitySet<ParameterGroup> ParameterGroups { get; private set; }
		/// <summary>Gets the formula parameters of the displayed formula.</summary>
		public EntitySet<FormulaParameter> Parameters { get; private set; }

		/// <summary>Creates a new <see cref="FormulaTemplateDetailsPage"/> instance for the given <paramref name="formulaTemplate"/>.</summary>
		/// <param name="formulaTemplate">The formula template to be displayed on this page.</param>
		private FormulaTemplateDetailsPage(FormulaTemplate formulaTemplate) : base(formulaTemplate) { }

		#region Action Buttons

		/// <summary>Checks out the formula template.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var result = base.CheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Undos the formula template check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var result = base.UndoCheckOut(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Checks in the formula template and its changes.</summary>
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

		/// <summary>Sets the approval status of the formula template.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the formula template.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public override ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var result = base.SetState(restAPI, newState, comment);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		/// <summary>Upgrades the formula template to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public override ViewCommandResult Upgrade(IRestAPI restAPI)
		{
			var result = base.Upgrade(restAPI);
			this.LoadParameters(restAPI);
			this.RefreshForCurrentTab(restAPI);
			return result;
		}

		public ViewCommandResult Validate(IRestAPI restAPI, Equipment equipment)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region GridView Buttons

		internal ViewCommandResult Add(IRestAPI restAPI, string parameterName, string defaultValue, string minValue = null, string maxValue = null, bool freqUsed = true, UnitOfMeasure uom = null, ParameterGroup group = null, ParameterDataType dataType = ParameterDataType.String, IDictionary<string, string> extValues = null)
		{
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			var parameter = new FormulaParameter();
			parameter.Name = parameterName;
			parameter.DataType = dataType;
			parameter.TargetValue = defaultValue;
			if (minValue != null)
				parameter.MinValue = minValue;
			if (maxValue != null)
				parameter.MaxValue = maxValue;
			parameter.FrequentlyUsed = freqUsed;
			if (uom != null)
				parameter.UnitOfMeasureId = uom.Id;
			if (group != null)
				parameter.ParameterGroupId = group.Id;
			if (extValues != null)
			{
				foreach (var kvp in extValues)
				{
					var extValue = parameter.FormulaParameterExts.FirstOrDefault(e => string.Equals(e.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
					if (extValue != null)
						extValue.Value = kvp.Value;
					else
					{
						var extDef = this.ParameterExtDefs.FirstOrDefault(e => string.Equals(e.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
						if (extDef != null)
						{
							parameter.FormulaParameterExts.Add(new ParameterExtensionValue()
							{
								ParameterExtDefId = extDef.Id,
								Value = kvp.Value,
							});
						}
					}
				}
			}
			this.Parameters.Add(formulaParameterQuery.Add(parameter));
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}
		public ViewCommandResult Add(IRestAPI restAPI, string parameterName, string defaultValue, bool freqUsed = true, UnitOfMeasure uom = null, ParameterGroup group = null, IDictionary<string, string> extValues = null)
		{
			return this.Add(restAPI, parameterName, defaultValue.ToString(), null, null, freqUsed, uom, group, ParameterDataType.String, extValues);
		}
		public ViewCommandResult Add(IRestAPI restAPI, string parameterName, bool defaultValue, bool freqUsed = true, UnitOfMeasure uom = null, ParameterGroup group = null, IDictionary<string, string> extValues = null)
		{
			return this.Add(restAPI, parameterName, defaultValue.ToString(), null, null, freqUsed, uom, group, ParameterDataType.Boolean, extValues);
		}
		public ViewCommandResult Add(IRestAPI restAPI, string parameterName, long defaultValue, long? minValue = null, long? maxValue = null, bool freqUsed = true, UnitOfMeasure uom = null, ParameterGroup group = null, IDictionary<string, string> extValues = null)
		{
			return this.Add(restAPI, parameterName, defaultValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, group, ParameterDataType.Integer, extValues);
		}
		public ViewCommandResult Add(IRestAPI restAPI, string parameterName, double defaultValue, double? minValue = null, double? maxValue = null, bool freqUsed = true, UnitOfMeasure uom = null, ParameterGroup group = null, IDictionary<string, string> extValues = null)
		{
			return this.Add(restAPI, parameterName, defaultValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, group, ParameterDataType.Double, extValues);
		}

		internal ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, string defaultValue, string minValue = null, string maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, ParameterGroup group = null, string parameterName = null, ParameterDataType dataType = ParameterDataType.String, IDictionary<string, string> extValues = null)
		{
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			parameter = this.Parameters[parameter.Id];
			if (parameterName != null)
				parameter.Name = parameterName;
			parameter.DataType = dataType;
			parameter.TargetValue = defaultValue;
			if (minValue != null)
				parameter.MinValue = minValue;
			if (maxValue != null)
				parameter.MaxValue = maxValue;
			if (freqUsed.HasValue)
				parameter.FrequentlyUsed = freqUsed.Value;
			if (uom != null)
				parameter.UnitOfMeasureId = uom.Id;
			if (group != null)
				parameter.ParameterGroupId = group.Id;
			if (extValues != null)
			{
				foreach (var kvp in extValues)
				{
					var extValue = parameter.FormulaParameterExts.FirstOrDefault(e => string.Equals(e.Name, kvp.Key, StringComparison.OrdinalIgnoreCase));
					if (extValue != null)
						extValue.Value = kvp.Value;
				}
			}
			this.Parameters[parameter.Id] = formulaParameterQuery.Change(parameter);
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, string defaultValue, bool? freqUsed = null, UnitOfMeasure uom = null, ParameterGroup group = null, string parameterName = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, defaultValue.ToString(), null, null, freqUsed, uom, group, parameterName, ParameterDataType.String, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, bool defaultValue, bool? freqUsed = null, UnitOfMeasure uom = null, ParameterGroup group = null, string parameterName = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, defaultValue.ToString(), null, null, freqUsed, uom, group, parameterName, ParameterDataType.Boolean, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, long defaultValue, long? minValue = null, long? maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, ParameterGroup group = null, string parameterName = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, defaultValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, group, parameterName, ParameterDataType.Integer, extValues);
		}
		public ViewCommandResult Update(IRestAPI restAPI, FormulaParameter parameter, double defaultValue, double? minValue = null, double? maxValue = null, bool? freqUsed = null, UnitOfMeasure uom = null, ParameterGroup group = null, string parameterName = null, IDictionary<string, string> extValues = null)
		{
			return this.Update(restAPI, parameter, defaultValue.ToString(), minValue.HasValue ? minValue.Value.ToString() : null, maxValue.HasValue ? maxValue.Value.ToString() : null, freqUsed, uom, group, parameterName, ParameterDataType.Double, extValues);
		}

		protected ViewCommandResult Delete(IRestAPI restAPI, FormulaParameter parameter)
		{
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			parameter = this.Parameters[parameter.Id];
			formulaParameterQuery.Delete(parameter.Id);
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormula(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}

		#endregion GridView Buttons

		/// <summary>Provides a query builder for working with <see cref="FormulaTemplate"/> entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected override IVersionedQueryBuilder<FormulaTemplate> QueryBuilderForEntity(IRestAPI restAPI)
		{
			return restAPI.ForFormulaTemplate();
		}

		private void RefreshForCurrentTab(IRestAPI restAPI)
		{
			this.LoadParameters(restAPI);
			var formulaParameterQuery = restAPI.ForFormulaParameter();
			// get formula parameters count
			var count = formulaParameterQuery.GetCountForFormulaTemplate(this.Entity.Id);

			// relate some things??
			foreach (var item in this.ParameterExtDefs)
			{
				if (item.ParameterGroupId != null)
				{
					item.ParameterGroup = this.ParameterGroups.SingleOrDefault(pg => pg.Id == item.ParameterGroupId.Value);
				}
			}
		}

		private void LoadParameters(IRestAPI restAPI)
		{
			if (!this._parametersLoaded)
			{
				var parameterGroupsQuery = restAPI.ForParameterGroup();
				var formulaParameterQuery = restAPI.ForFormulaParameter();
				// get parameter groups
				this.ParameterGroups.Clear();
				this.ParameterGroups.AddRange(parameterGroupsQuery.GetMany(includeNull: true, includeDeleted: true));
				// get parameter extension definitions
				this.ParameterExtDefs.Clear();
				this.ParameterExtDefs.AddRange(formulaParameterQuery.GetTemplateParameterExtDefs(this.Entity.Id));
				// get formula parameters
				this.Parameters.Clear();
				this.Parameters.AddRange(formulaParameterQuery.GetManyForFormulaTemplate(this.Entity.Id));
				this._parametersLoaded = true;
			}
		}
	}
}
