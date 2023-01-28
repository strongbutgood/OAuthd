using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	class ControlRecipeQuery : QueryBuilderBase<RecipeTemplate>
	{
		public override string ControllerName
		{
			get { return "ControlRecipes"; }
			protected set { }
		}

		public ControlRecipeQuery(IRestAPI restAPI)
			: base(restAPI)
		{
		}

		/// <summary>Gets the control recipe for the specified <paramref name="equipmentId"/>.</summary>
		/// <param name="equipmentId">The id of the equipment running the control recipe.</param>
		/// <param name="includeCheckedOut">true to include checked out entities. This is the default.</param>
		public RecipeTemplate GetOne(int equipmentId, bool? includeCheckedOut = true)
		{
			this.WithQuery<int>("equipmentId", equipmentId);
			if (includeCheckedOut.HasValue)
				this.WithQuery<bool>("includeCheckedOut", includeCheckedOut.GetValueOrDefault());
			return base.GetOne();
		}

		public RecipeTemplate LoadTree(int equipmentId)
		{
			this.WithCommandType("loadTree");
			this.WithQuery<int>("equipmentId", equipmentId);
			return base.GetOne();
		}
	}
}
