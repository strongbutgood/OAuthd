using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	static class ProcedureTokenQueryExtension
	{
		public static ProcedureTokenQuery ForProcedureToken(this IRestAPI restAPI)
		{
			return new ProcedureTokenQuery(restAPI);
		}
	}

	class ProcedureTokenQuery : QueryBuilderBase<ProcedureToken>
	{
		public ProcedureTokenQuery(IRestAPI restAPI) : base(restAPI) { }

		public IEnumerable<ProcedureToken> GetManyForProcedure(int procedureId, bool? runningTokens = null)
		{
			this.WithQuery<int>("procedureId", procedureId);
			if (runningTokens.HasValue)
			{
				this.WithQuery<bool>("runningTokens", runningTokens.Value);
				return base.GetMany();
			}
			var procedureToken = base.GetOne();
			if (procedureToken != null)
			{
				return this.SelfAndChildren(procedureToken);
			}
			return new ProcedureToken[0];
		}

		private IEnumerable<ProcedureToken> SelfAndChildren(ProcedureToken token)
		{
			yield return token;
			foreach (var child in token.ChildProcedureTokens)
			{
				foreach (var item in this.SelfAndChildren(child))
				{
					yield return item;
				}
			}
		}
	}
}
