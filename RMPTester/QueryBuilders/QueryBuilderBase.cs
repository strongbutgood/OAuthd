using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	/// <summary>
	/// Builds an API path for accessing Recipe Manager Plus entities.
	/// </summary>
	/// <typeparam name="T">The type of entity to access.</typeparam>
	[System.Diagnostics.DebuggerDisplay("{UrlString}")]
	abstract class QueryBuilderBase<T>
		where T : class
	{
		/// <summary>The format of the API path: "api/{controllerName}/[{commandType}/][{id}/][?{queries}]".</summary>
		public const string APIRootFormat = "api/{0}/{1}{2}{3}";

		private const string ViewCommandResultResourceType = "ViewCommandResult";

		private IRestAPI _restAPI;

		/// <summary>Gets the controller which will provide the data.</summary>
		public virtual string ControllerName { get; protected set; }
		/// <summary>Gets or sets the command to exeucte on the controller.</summary>
		public string CommandType { get; set; }
		/// <summary>Gets or sets the id for the api path.</summary>
		public int? IdValue { get; set; }
		/// <summary>Gets the list of query parameters that are added to the api path.</summary>
		public Dictionary<string, QueryParameter> Queries { get; private set; }

		/// <summary>Gets the url string that this builder will execute.</summary>
		internal string UrlString
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				string commandType = string.Empty;
				if (!string.IsNullOrWhiteSpace(this.CommandType))
					commandType = this.CommandType.Trim() + "/";
				string idValue = string.Empty;
				if (this.IdValue.HasValue)
					idValue = this.IdValue.Value.ToString() + "/";
				string query = string.Empty;
				if (this.Queries.Any())
					query = "?" + string.Join("&", this.Queries.Select(qp => qp.Value.UrlString));
				return string.Format(QueryBuilderBase<T>.APIRootFormat, this.ControllerName, commandType, idValue, query);
			}
		}

		/// <summary>Creates a new <see cref="QueryBuilderBase{T}"/> for calling RESTful API.</summary>
		/// <param name="restAPI">The facilitator of RESTful API calls.</param>
		public QueryBuilderBase(IRestAPI restAPI)
		{
			this._restAPI = restAPI;
			this.ControllerName = typeof(T).Name + "s";
			this.CommandType = string.Empty;
			this.IdValue = null;
			this.Queries = new Dictionary<string, QueryParameter>(StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>Resets the query.</summary>
		public void Reset()
		{
			this.CommandType = string.Empty;
			this.IdValue = null;
			this.Queries.Clear();
		}

		/// <summary>Gets the resource from the API path built, and converts it to an entity.</summary>
		public TResult GetResult<TResult>()
		{
			if (this._restAPI != null)
			{
				Resource resource = this._restAPI.GetOne(this.UrlString);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.FromResource<TResult>();
			}
			return default(TResult);
		}

		/// <summary>Gets the resource from the API path built, and converts it to an entity.</summary>
		public T GetOne()
		{
			if (this._restAPI != null)
			{
				Resource resource = this._restAPI.GetOne(this.UrlString);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.FromResource<T>();
			}
			return default(T);
		}

		/// <summary>Gets the resource from the API path built, splits and converts it into a list of entities.</summary>
		public IEnumerable<T> GetMany()
		{
			if (this._restAPI != null)
			{
				Resource resource = this._restAPI.GetOne(this.UrlString);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.ToArray().Select(r => r.FromResource<T>());
			}
			return null;
		}
		/// <summary>Gets the resource from the API path built, splits and converts it into a list of entities.</summary>
		/// <typeparam name="TResult">The type of the result entity to return.</typeparam>
		public IEnumerable<TResult> GetMany<TResult>()
		{
			if (this._restAPI != null)
			{
				Resource resource = this._restAPI.GetOne(this.UrlString);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.ToArray().Select(r => r.FromResource<TResult>());
			}
			return null;
		}

		/// <summary>Posts the resource to the API path built, and returns the view command result.</summary>
		/// <param name="item">The resource to post to the API path.</param>
		public ViewCommandResult Post(T item = default(T))
		{
			if (this._restAPI != null)
			{
				Resource requestResource = item.ToResource();
				Resource resource = this._restAPI.Post(this.UrlString, requestResource);
				this.Reset();
				return resource.FromResource<ViewCommandResult>();
			}
			return null;
		}
		/// <summary>Posts the resource to the API path built, and returns the resource result.</summary>
		/// <param name="item">The resource to post to the API path.</param>
		public TReturn Post<TReturn>(T item = default(T))
			where TReturn : T
		{
			if (this._restAPI != null)
			{
				Resource requestResource = item.ToResource();
				Resource resource = this._restAPI.Post(this.UrlString, requestResource);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.FromResource<TReturn>();
			}
			return default(TReturn);
		}

		/// <summary>Puts the resource to the API path built, and returns the view command result.</summary>
		/// <param name="item">The resource to put to the API path.</param>
		public ViewCommandResult Put(T item = default(T))
		{
			if (this._restAPI != null)
			{
				Resource requestResource = item.ToResource();
				Resource resource = this._restAPI.Put(this.UrlString, requestResource);
				this.Reset();
				return resource.FromResource<ViewCommandResult>();
			}
			return null;
		}
		/// <summary>Puts the resource to the API path built, and returns the resource result.</summary>
		/// <param name="item">The resource to put to the API path.</param>
		public TReturn Put<TReturn>(T item = default(T))
			where TReturn : T
		{
			if (this._restAPI != null)
			{
				Resource requestResource = item.ToResource();
				Resource resource = this._restAPI.Put(this.UrlString, requestResource);
				this.ThrowIfViewCommandError(resource);
				this.Reset();
				return resource.FromResource<TReturn>();
			}
			return default(TReturn);
		}

		/// <summary>Deletes the resource to the API path built, and returns the view command result.</summary>
		protected void Delete()
		{
			if (this._restAPI != null && this._restAPI.GetType().Namespace.Contains("MATS"))
			{
				//Server.ServerAPI.ProcessRequest(this.UrlString, System.Net.Http.HttpMethod.Delete, null);
				this.Reset();
			}
		}

		/// <summary>Tests if the resource is an error and throws an exception if applicable.</summary>
		/// <param name="resource">The resource to check.</param>
		/// <exception cref="Exception">Thrown if the view command has a <see cref="P:ViewCommandResult.CommandError"/> with no sub errors.</exception>
		/// <exception cref="AggregateException">Thrown if the view command has a <see cref="P:ViewCommandResult.CommandError"/> and one or more sub errors. The <see cref="P:AggregateException.InnerExceptions"/> will contain the sub errors.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the view command result was not successful.</exception>
		private void ThrowIfViewCommandError(Resource resource)
		{
			if (resource == null)
				throw new ArgumentNullException(nameof(resource));
			if (resource.ResourceType == QueryBuilderBase<T>.ViewCommandResultResourceType)
			{
				var viewCommandResult = resource.FromResource<ViewCommandResult>();
				if (viewCommandResult != null)
				{
					if (viewCommandResult.CommandError != null)
					{
						if (viewCommandResult.SubErrors?.Any() == true)
						{
							throw new AggregateException(viewCommandResult.CommandError.ErrorMessage, viewCommandResult.SubErrors.Select(e => new Exception(e.ErrorMessage)));
						}
						throw new Exception(viewCommandResult.CommandError.ErrorMessage);
					}
					if (!viewCommandResult.Success)
						throw new InvalidOperationException("The request was not successful.");
				}
			}
		}

		/// <summary>Updates the builders command type with that specified.</summary>
		/// <param name="commandType">The command type to execute on the controller.</param>
		public QueryBuilderBase<T> WithCommandType(string commandType)
		{
			this.CommandType = commandType;
			return this;
		}

		/// <summary>Updates the builders id value with that specified.</summary>
		/// <param name="idValue">The id value to send to the controller.</param>
		public QueryBuilderBase<T> WithIdValue(int? idValue)
		{
			this.IdValue = idValue;
			return this;
		}

		/// <summary>Updates the builders query with the name value pair provided.</summary>
		/// <typeparam name="TValue">The type of the parameter value.</typeparam>
		/// <param name="name">The name of the query parameter to add.</param>
		/// <param name="value">The value of the query parameter.</param>
		public QueryBuilderBase<T> WithQuery<TValue>(string name, TValue value)
		{
			//name = name.ToUpperInvariant();
			QueryParameter query;
			if (this.Queries.TryGetValue(name, out query))
				query.Value = value;
			else
				this.Queries.Add(name, new QueryParameter(name, value));
			return this;
		}
	}

	static class EntityQueryExtension
	{
		public static EntityQuery<TEntity> ForEntity<TEntity>(this IRestAPI restAPI, StateViewTypes? viewFilter = null)
			where TEntity : class, ProcessMfg.Model.IEntity
		{
			var query = new EntityQuery<TEntity>(restAPI);
			if (viewFilter.HasValue)
				query.WithQuery<StateViewTypes>("viewFilter", viewFilter.GetValueOrDefault(StateViewTypes.Latest));
			return query;
		}
	}

	/// <summary>
	/// Builds an API path for accessing Recipe Manager Plus entities.
	/// </summary>
	/// <typeparam name="TEntity">The type of entity to access.</typeparam>
	class EntityQuery<TEntity> : QueryBuilderBase<TEntity>, IQueryBuilder<TEntity>
		where TEntity : class, ProcessMfg.Model.IEntity
	{
		/// <summary>Creates a new <see cref="EntityQuery{TEntity}"/> for calling RESTful API.</summary>
		/// <param name="restAPI">The facilitator of RESTful API calls.</param>
		public EntityQuery(IRestAPI restAPI) : base(restAPI) { }

		#region IQueryBuilder<TEntity> Members

		/// <summary>Modifies the entity by PUTting the specified <paramref name="entity"/>, optionally overriding checkout.</summary>
		/// <param name="entity">The entity data to PUT as a change to the server.</param>
		/// <param name="overrideCheckout">true to override existing checkout.</param>
		public virtual TEntity Change(TEntity entity, bool? overrideCheckout = null)
		{
			if (overrideCheckout.HasValue)
				this.WithQuery<bool>("overrideCheckout", overrideCheckout.GetValueOrDefault());
			return base.Put<TEntity>(entity);
		}

		/// <summary>Gets the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to get.</param>
		public virtual TEntity GetOne(int id)
		{
			this.WithQuery<int>("id", id);
			return base.GetOne();
		}

		#endregion
	}

	class VersionedEntityQuery<TEntity> : EntityQuery<TEntity>, IVersionedQueryBuilder<TEntity>
		where TEntity : class, ProcessMfg.Model.IVersionedEntity
	{
		public VersionedEntityQuery(IRestAPI restAPI) : base(restAPI) { }

		#region IVersionedQueryBuilder<TEntity> Members

		/// <summary>Checks out the entity with the specified <paramref name="name"/>.</summary>
		/// <param name="name">The name of the entity to check out.</param>
		public virtual TEntity CheckOut(string name)
		{
			this.WithCommandType("checkOut");
			this.WithQuery<string>("name", name);
			return base.Put<TEntity>();
		}

		/// <summary>Checks in the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to check in.</param>
		public virtual ViewCommandResult CheckIn(int id)
		{
			this.WithCommandType("checkIn");
			this.WithQuery<int>("id", id);
			return base.Put();
		}

		/// <summary>Undoes an existing check out for the entity with the specified <paramref name="id"/>..</summary>
		/// <param name="id">The id of the entity to undo check out.</param>
		public virtual TEntity UndoCheckOut(int id)
		{
			this.WithCommandType("undoCheckOut");
			this.WithQuery<int>("id", id);
			return base.Put<TEntity>();
		}

		/// <summary>Sets the approval state of the entity with the specified <paramref name="id"/>.</summary>
		/// <param name="id">The id of the entity to set the state of.</param>
		/// <param name="state">The new state of the entity.</param>
		/// <param name="comment">A state change comment.</param>
		public virtual TEntity SetState(int id, StateType state, string comment)
		{
			this.WithCommandType("setState");
			this.WithQuery<int>("id", id);
			this.WithQuery<StateType>("state", state);
			this.WithQuery<string>("comment", comment);
			return base.Put<TEntity>();
		}

		/// <summary>Upgrades the entity with the specified <paramref name="id"/> to match the latest changes in other entities.</summary>
		/// <param name="id">The id of the entity to upgrade.</param>
		public virtual TEntity Upgrade(int id)
		{
			this.WithCommandType("upgrade");
			this.WithQuery<int>("id", id);
			return base.Put<TEntity>();
		}

		#endregion
	}
}
