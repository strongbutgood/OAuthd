using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Represents the actions that can be taken on <see cref="IVersionedEntity"/> instances.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity that these actions can be taken on.</typeparam>
	interface IVersionedButtons<TEntity>
		where TEntity : IVersionedEntity
	{
		/// <summary>Checks out the entity.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		ViewCommandResult CheckOut(IRestAPI restAPI);

		/// <summary>Undos the entity check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		ViewCommandResult UndoCheckOut(IRestAPI restAPI);

		/// <summary>Checks in the entity and its changes.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		ViewCommandResult CheckIn(IRestAPI restAPI);

		/// <summary>Sets the approval status of the entity.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the entity.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment);

		/// <summary>Upgrades the entity to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		ViewCommandResult Upgrade(IRestAPI restAPI);

		/// <summary>Performs a diff report on the entity compared with the previous version.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		object ShowVersionDifferences(IRestAPI restAPI);

		/// <summary>Performs a diff report on the entity compared with after an upgrade has occurred.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		object ShowUpgradeDifferences(IRestAPI restAPI);
	}
		//ViewCommandResult SetUse(IRestAPI restAPI, bool allowed);
		//ViewCommandResult Validate(IRestAPI restAPI);
}
