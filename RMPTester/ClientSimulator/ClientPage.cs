using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
//using MATS.Common;
using MATS.Module.RecipeManagerPlus.QueryBuilders;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	/// <summary>
	/// Base class for all client simulator pages.
	/// </summary>
	abstract class ClientPage
	{
		/// <summary>Creates a new <see cref="ClientPage"/> instance.</summary>
		protected ClientPage()
		{
		}
	}

	/// <summary>
	/// Base class for all client simulator summary pages.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity represented by the page.</typeparam>
	abstract class ClientSummaryPage<TEntity> : ClientPage
		where TEntity : class, IVersionedEntity
	{
		/// <summary>Gets all the entities displayed on this page.</summary>
		public IEnumerable<TEntity> Entities { get; protected set; }

		/// <summary>Creates a new <see cref="ClientSummaryPage{TEntity}"/> instance for the given <paramref name="entities"/>.</summary>
		/// <param name="entities">The entities to be displayed on this page.</param>
		protected ClientSummaryPage(IEnumerable<TEntity> entities)
		{
			if (entities == null)
				throw new ArgumentNullException(nameof(entities));
			this.Entities = this.CreateEntitySet(entities);
		}

		/// <summary>Creates the entity set from supplied entities.</summary>
		/// <param name="entities">The entities to create an entity set from.</param>
		protected virtual IEnumerable<TEntity> CreateEntitySet(IEnumerable<TEntity> entities)
		{
			IEnumerable<TEntity> entitySet = null;
			try { if ((entitySet = EntitySet<IEntity>.TryCreateAndFillSet(entities)) != null) return entitySet; }
			catch { }
			try { if ((entitySet = NonEntitySet<IEntity>.TryCreateAndFillSet(entities)) != null) return entitySet; }
			catch { }
			return entities.ToList();
		}

		/// <summary>Opens the detail page for the selected <paramref name="entity"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="entity">The entity to open the detail page for.</param>
		public abstract ClientDetailPage<TEntity> Select(IRestAPI restAPI, TEntity entity);
		/// <summary>Opens the detail page for the entity with the given <paramref name="id"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="id">The id of the entity to open the detail page for.</param>
		/// <param name="anyVersion">true to match the entity of any version, or false to only use the entity with the given <paramref name="id"/>. The default is false.</param>
		public ClientDetailPage<TEntity> Select(IRestAPI restAPI, int id, bool anyVersion = false)
		{
			TEntity entity;
			if (!anyVersion)
			{
				entity = this.Entities.Single(e => e.Id == id);
			}
			else
			{
				entity = this.Entities.FirstOrDefault(e => e.Id == id);
				if (entity == null)
				{
					var query = restAPI.ForEntity<TEntity>();
					entity = query.GetOne(id);
					// walk the version tree to get the latest entity
					while (entity != null && entity.ChildId != null)
					{
						entity = query.GetOne(entity.ChildId.Value);
					}
				}
				if (entity == null)
					throw new InvalidOperationException("Could not select a single entity.");
			}
			return this.Select(restAPI, entity);
		}
		/// <summary>Opens the detail page for the entity with the given <paramref name="name"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="name">The name of the entity to open the detail page for.</param>
		public ClientDetailPage<TEntity> Select(IRestAPI restAPI, string name)
		{
			var entity = this.Entities.Single(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			return this.Select(restAPI, entity);
		}
	}

	/// <summary>
	/// Base class for all client detail summary pages.
	/// </summary>
	/// <typeparam name="TEntity">The type of the entity represented by the page.</typeparam>
	abstract class ClientDetailPage<TEntity> : ClientPage, IVersionedButtons<TEntity>
		where TEntity : class, IVersionedEntity
	{
		/// <summary>Gets the entity displayed on this page.</summary>
		public TEntity Entity { get; protected set; }

		/// <summary>Creates a new <see cref="ClientDetailPage{TEntity}"/> instance for the given <paramref name="entity"/>.</summary>
		/// <param name="entity">The entity to be displayed on this page.</param>
		protected ClientDetailPage(TEntity entity)
		{
			this.Entity = entity ?? throw new ArgumentNullException(nameof(entity));
		}
		
		#region IVersionedButtons Members

		/// <summary>Checks out the entity.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual ViewCommandResult CheckOut(IRestAPI restAPI)
		{
			var api = this.QueryBuilderForEntity(restAPI);
			this.Entity = api.CheckOut(this.Entity.Name);
			return new ViewCommandResult() { Success = true };
		}

		/// <summary>Undos the entity check out.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual ViewCommandResult UndoCheckOut(IRestAPI restAPI)
		{
			var api = this.QueryBuilderForEntity(restAPI);
			this.Entity = api.UndoCheckOut(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}

		/// <summary>Checks in the entity and its changes.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual ViewCommandResult CheckIn(IRestAPI restAPI)
		{
			var api = this.QueryBuilderForEntity(restAPI);
			this.Entity = api.Change(this.Entity);
			var result = api.CheckIn(this.Entity.Id);
			var undidCheckOut = false;
			if (result.CommandError != null)
			{
				if (result.CommandError.ErrorCode == 274)
				{
					this.UndoCheckOut(restAPI);
					undidCheckOut = true;
				}
			}
			if (!undidCheckOut)
			{
				this.Entity = api.GetOne(this.Entity.Id);
				this.Entity.CheckedOut = false;
			}
			return result;
		}

		/// <summary>Sets the approval status of the entity.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		/// <param name="newState">The approval status of the entity.</param>
		/// <param name="comment">The comment to record with the state change.</param>
		public virtual ViewCommandResult SetState(IRestAPI restAPI, StateType newState, string comment)
		{
			var api = this.QueryBuilderForEntity(restAPI);
			this.Entity = api.SetState(this.Entity.Id, newState, comment);
			return new ViewCommandResult() { Success = true };
		}

		/// <summary>Upgrades the entity to match other versioned entities.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual ViewCommandResult Upgrade(IRestAPI restAPI)
		{
			var api = this.QueryBuilderForEntity(restAPI);
			this.Entity = api.Upgrade(this.Entity.Id);
			return new ViewCommandResult() { Success = true };
		}

		/// <summary>Performs a diff report on the entity compared with the previous version.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual object ShowVersionDifferences(IRestAPI restAPI) { throw new NotImplementedException(); }

		/// <summary>Performs a diff report on the entity compared with after an upgrade has occurred.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		public virtual object ShowUpgradeDifferences(IRestAPI restAPI) { throw new NotImplementedException(); }

		#endregion

		//public virtual ViewCommandResult SetUse(IRestAPI restAPI, bool allowed) { throw new NotImplementedException(); }
		//public virtual ViewCommandResult Validate(IRestAPI restAPI) { throw new NotImplementedException(); }

		/// <summary>Implemented by derived classes to provide a query builder for pages with <see cref="IVersionedButtons{TEntity}"/>.</summary>
		/// <param name="restAPI">The RecipeManagerPlus RESTful API interface.</param>
		protected abstract QueryBuilders.IVersionedQueryBuilder<TEntity> QueryBuilderForEntity(IRestAPI restAPI);
	}
}
