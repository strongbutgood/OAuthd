using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	/// <summary>
	/// Helps build API queries.
	/// </summary>
	/// <typeparam name="T">The type of entity to build a query for.</typeparam>
	public interface IQueryBuilder<T> //where T : class
	{
		/// <summary>Modifies the entity by PUTting the specified <paramref name="entity"/>, optionally overriding checkout.</summary>
		/// <param name="entity">The entity data to PUT as a change to the server.</param>
		/// <param name="overrideCheckout">true to override existing checkout.</param>
		T Change(T entity, bool? overrideCheckout = null);
		/// <summary>Gets the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to get.</param>
		T GetOne(int id);
	}

	/// <summary>
	/// Helps build API queries for versioned entities.
	/// </summary>
	/// <typeparam name="TEntity">The type of entity to build a query for.</typeparam>
	public interface IVersionedQueryBuilder<TEntity> : IQueryBuilder<TEntity>
		where TEntity : IVersionedEntity
	{
		/// <summary>Checks out the entity with the specified <paramref name="name"/>.</summary>
		/// <param name="name">The name of the entity to check out.</param>
		TEntity CheckOut(string name);
		/// <summary>Checks in the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to check in.</param>
		ViewCommandResult CheckIn(int id);
		/// <summary>Undoes an existing check out for the entity with the specified <paramref name="id"/>..</summary>
		/// <param name="id">The id of the entity to undo check out.</param>
		TEntity UndoCheckOut(int id);
		/// <summary>Sets the approval state of the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to set the state of.</param>
		/// <param name="state">The new state of the entity.</param>
		/// <param name="comment">A state change comment.</param>
		TEntity SetState(int id, StateType state, string comment);
		/// <summary>Upgrades the entity with the specified <paramref name="id"/> to match the latest changes in other entities.</summary>
		/// <param name="id">The id of the entity to upgrade.</param>
		TEntity Upgrade(int id);
	}
}
