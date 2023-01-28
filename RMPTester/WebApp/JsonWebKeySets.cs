using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class JsonWebKeySets : JsonObjectBase<Newtonsoft.Json.Linq.JObject>
	{
		public IList<JsonWebKey> Keys
		{
			get => new JsonWebKeyCollection((Newtonsoft.Json.Linq.JArray)this._json["keys"]);
			set
			{
				if (value == null || value.Count == 0)
					this._json["keys"] = new Newtonsoft.Json.Linq.JArray();
				else
					this._json["keys"] = new Newtonsoft.Json.Linq.JArray(value.Select(jwk => jwk.ToJToken()));
			}
		}

		public static JsonWebKeySets Parse(string json)
		{
			if (string.IsNullOrEmpty(json))
				throw new ArgumentNullException(nameof(json));
			return new JsonWebKeySets(Newtonsoft.Json.Linq.JObject.Parse(json));
		}

		public JsonWebKeySets(params JsonWebKey[] jsonWebKeys)
			: base(new Newtonsoft.Json.Linq.JObject(new Newtonsoft.Json.Linq.JProperty("keys", new Newtonsoft.Json.Linq.JArray())))
		{
			if (!(this._json["keys"] is Newtonsoft.Json.Linq.JArray jArray))
			{
				jArray = new Newtonsoft.Json.Linq.JArray();
				this._json["keys"] = jArray;
			}
			foreach (var jwk in jsonWebKeys)
			{
				jArray.Add(jwk.JsonInternal);
			}
		}
		public JsonWebKeySets(Newtonsoft.Json.Linq.JObject json)
			: base(json)
		{
			if (!json.ContainsKey("keys"))
				throw new ArgumentException("Missing the required 'keys' parameter.", nameof(json));
		}

		private class JsonWebKeyCollection : Collection<JsonWebKey>
		{
			private readonly Newtonsoft.Json.Linq.JArray _jArray;

			public JsonWebKeyCollection(Newtonsoft.Json.Linq.JArray jArray)
			{
				this._jArray = jArray;
				foreach (var jToken in jArray)
				{
					base.InsertItem(base.Count, new JsonWebKey((Newtonsoft.Json.Linq.JObject)jToken));
				}
			}

			protected override void ClearItems()
			{
				this._jArray.Clear();
				base.ClearItems();
			}

			protected override void InsertItem(int index, JsonWebKey item)
			{
				this._jArray.Insert(index, item.JsonInternal);
				base.InsertItem(index, item);
			}

			protected override void SetItem(int index, JsonWebKey item)
			{
				this._jArray[index] = item.JsonInternal;
				base.SetItem(index, item);
			}

			protected override void RemoveItem(int index)
			{
				this._jArray.RemoveAt(index);
				base.RemoveItem(index);
			}
		}
	}
}
