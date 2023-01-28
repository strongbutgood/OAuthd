using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MATS.Module.RecipeManagerPlus.QueryBuilders
{
	/// <summary>
	/// Provides the query portion of an API path.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("{UrlString}")]
	class QueryParameter
	{
		/// <summary>The format of the API query: "{name}={value}".</summary>
		public const string APIQueryFormat = "{0}={1}";

		/// <summary>Gets or sets the name of the query parameter.</summary>
		public string Name { get; set; }

		/// <summary>Gets or sets the value of the query parameter.</summary>
		public object Value { get; set; }

		/// <summary>Gets the string representation of the query parameter.</summary>
		internal string UrlString
		{
			get
			{
				string value = (this.Value ?? "").ToString();
				if (this.Value != null && this.Value.GetType().IsEnum)
				{
					value = Convert.ToInt64(this.Value).ToString();
				}
				return string.Format(QueryParameter.APIQueryFormat, this.Name, value);
			}
		}

		/// <summary>Creates a new <see cref="QueryParameter"/>.</summary>
		/// <param name="name">The name of the query parameter.</param>
		/// <param name="value">The value of the query parameter.</param>
		public QueryParameter(string name, object value)
		{
			this.Name = name;
			this.Value = value;
		}

		///// <summary>Returns the string representation of the query parameter.</summary>
		//public override string ToString()
		//{
		//	return string.Format(QueryParameter.APIQueryFormat, this.Name, this.Value);
		//}
	}
}
