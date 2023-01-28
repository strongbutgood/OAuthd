using System;
using System.Collections.Generic;

namespace MATS.Module.RecipeManagerPlus.ClientSimulator.WebApp
{
	class SettingsBase : Dictionary<string, object>
	{
		public new object this[string key]
		{
			get => base[key];
			set => base[key] = value;
		}

		public SettingsBase() : base() { }
		public SettingsBase(IDictionary<string, object> settings) : base(settings) { }

		public void Add(IDictionary<string, object> settings)
		{
			foreach (var kvp in settings)
			{
				if (!this.ContainsKey(kvp.Key))
					this.Add(kvp.Key, kvp.Value);
			}
		}

		public void AddOrUpdate(IDictionary<string, object> settings)
		{
			foreach (var kvp in settings)
			{
				if (!this.ContainsKey(kvp.Key))
					this.Add(kvp.Key, kvp.Value);
				else
					this[kvp.Key] = kvp.Value;
			}
		}

		public T GetValueOrDefault<T>(string key, T defaultValue = default(T))
		{
			if (this.TryGetValue(key, out var value) && value is T typedValue)
				return typedValue;
			return defaultValue;
		}

		public void SetOrRemove(string key, object value)
		{
			if (value == null)
				this.Remove(key);
			else
				this[key] = value;
		}
	}
}
