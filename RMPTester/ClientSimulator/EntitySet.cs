using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using ProcessMfg.Model;
using MATS.Common;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator
{
	class EntitySet<TEntity> : KeyedCollection<int, TEntity>
		where TEntity : IEntity
	{
		internal static ICollection<T> TryCreateAndFillSet<T>(IEnumerable<T> items)
		{
			if (typeof(IEntity).IsAssignableFrom(typeof(T)))
			{
				var gType = typeof(EntitySet<>).MakeGenericType(new Type[] { typeof(T) });
				ICollection<T> set = (ICollection<T>)Activator.CreateInstance(gType, null);
				foreach (var item in items)
				{
					set.Add(item);
				}
				return set;
			}
			return null;
		}

		public new TEntity this[int key]
		{
			get { return base[key]; }
			set
			{
				if (!this.Contains(key))
				{
					this.Add(value);
				}
				else
				{
					var item = base[key];
					if (this.GetKeyForItem(value) != key)
						throw new InvalidOperationException("The key of the supplied value does not match the key of the item which is to be replaced.");
					var index = this.IndexOf(item);
					base.SetItem(index, value);
				}
			}
		}

		protected override int GetKeyForItem(TEntity item)
		{
			ThrowHelper.IfArgumentNull(() => item);
			return item.Id;
		}

		public void AddRange(IEnumerable<TEntity> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}
	}

	class NonEntitySet<TEntity> : KeyedCollection<long, TEntity>
	{
		internal static ICollection<T> TryCreateAndFillSet<T>(IEnumerable<T> items)
		{
			if (typeof(T).GetProperty("Id") != null)
			{
				var gType = typeof(NonEntitySet<>).MakeGenericType(new Type[] { typeof(T) });
				var entityPar = Expression.Parameter(typeof(T), "item");
				var selector = Expression.Lambda(
					typeof(Func<,>).MakeGenericType(new Type[] { typeof(T), typeof(int) }),
					Expression.Property(entityPar, typeof(T), "Id"),
					entityPar);
				ICollection<T> set = (ICollection<T>)Activator.CreateInstance(gType, new object[] { selector.Compile() });
				foreach (var item in items)
				{
					set.Add(item);
				}
				return set;
			}
			return null;
		}

		private Func<TEntity, long> _keySelector;

		public new TEntity this[int key]
		{
			get { return base[key]; }
			set
			{
				if (!this.Contains(key))
				{
					this.Add(value);
				}
				else
				{
					var item = base[key];
					if (this.GetKeyForItem(value) != key)
						throw new InvalidOperationException("The key of the supplied value does not match the key of the item which is to be replaced.");
					var index = this.IndexOf(item);
					base.SetItem(index, value);
				}
			}
		}

		public NonEntitySet(Func<TEntity, long> keySelector)
			: base()
		{
			this._keySelector = keySelector;
		}
		public NonEntitySet(Func<TEntity, long> keySelector, IEqualityComparer<long> comparer)
			: base(comparer)
		{
			this._keySelector = keySelector;
		}
		public NonEntitySet(Func<TEntity, long> keySelector, IEqualityComparer<long> comparer, int dictionaryCreationThreshold)
			: base(comparer, dictionaryCreationThreshold)
		{
			this._keySelector = keySelector;
		}

		protected override long GetKeyForItem(TEntity item)
		{
			return this._keySelector(item);
		}

		public void AddRange(IEnumerable<TEntity> items)
		{
			foreach (var item in items)
			{
				this.Add(item);
			}
		}
	}
}
