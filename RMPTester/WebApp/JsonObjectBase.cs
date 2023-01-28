using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonObjectBase<T> where T : Newtonsoft.Json.Linq.JToken
	{
		internal T JsonInternal => this._json;

		public Newtonsoft.Json.Linq.JToken this[string propertyName]
		{
			get => this._json[propertyName];
			set => this._json[propertyName] = value;
		}

		protected readonly T _json;

		protected JsonObjectBase(T json)
		{
			if (json == null)
				throw new ArgumentNullException(nameof(json));
			this._json = json;
		}

		public T ToJToken() => (T)this._json.DeepClone();
	}
}
