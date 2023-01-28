using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using MATS.Module.RecipeManagerPlus.ArchestrA.Web;
using ProcessMfg.Model;

namespace MATS.Module.RecipeManagerPlus
{
	static class RestAPIExtensions
	{
		#region System Management

		public static bool SetCookie(this IRestAPI restAPI)
		{
			var result = restAPI.Post("api/SystemManagement/SetCookie");
			if (result.HttpStatus != 200)
				return false;
			return true;
		}

		public static bool LogOut(this IRestAPI restAPI, bool? removeCookie = null)
		{
			var query = removeCookie.HasValue ? "?removeCookie=" + removeCookie.Value.ToString() : string.Empty;
			var result = restAPI.Put("api/SystemManagement/LogOut" + query);
			if (result.HttpStatus != 200)
				return false;
			return true;
		}

		#endregion System Management

		#region Type Conversion

		/// <summary>Deserializes an entity from the Recipe Manager Plus model using data from a <see cref="Resource"/> obtained from the server with APIs.</summary>
		/// <typeparam name="T">The type of the entity to deserialize.</typeparam>
		/// <param name="resource">The resource to deserialize.</param>
		internal static T FromResource<T>(this Resource resource)
			//where T : IEntity
		{
			if (resource == null || string.IsNullOrEmpty(resource.ToXml()))
			{
				return default(T);
			}
			if (typeof(T).Equals(typeof(Equipment)))
			{
				// special case, equipment has its own deserialization method
				return (T)(object)Equipment.FromXml(resource.ToXml());
			}
			T result;
			try
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
				using (StringReader stringReader = new StringReader(resource.ToXml()))
				{
					XmlTextReader reader = new XmlTextReader(stringReader);
					T obj = (T)dataContractSerializer.ReadObject(reader);
					result = obj;
				}
			}
			catch (Exception)
			{
				result = default(T);
			}
			return result;
		}
		
		/// <summary>Serializes an entity from the Recipe Manager Plus model into a <see cref="Resource"/> which can be sent to the server with APIs.</summary>
		/// <typeparam name="T">The type of the entity to serialize.</typeparam>
		/// <param name="entity">The entity to serialize.</param>
		/// <param name="httpStatus">The HTTP status code from a response message.</param>
		internal static Resource ToResource<T>(this T entity, int httpStatus = 0)
			//where T : IEntity
		{
			if (entity == null)
			{
				return null;
			}
			string result;
			try
			{
				DataContractSerializer dataContractSerializer = new DataContractSerializer(typeof(T));
				using (StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture))
				{
					XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
					xmlTextWriter.Formatting = Formatting.None;
					dataContractSerializer.WriteObject(xmlTextWriter, entity);
					xmlTextWriter.Flush();
					string text = stringWriter.ToString();
					result = text;
				}
			}
			catch (Exception)
			{
				result = string.Empty;
			}
			return new Resource(result, httpStatus);
		}

		#endregion Type Conversion

		public static string ToString(this FormulaParameter parameter, string defaultValue = "(null)")
		{
			if (parameter == null)
				return defaultValue;
			var sb = new System.Text.StringBuilder();
			sb.AppendFormat("{0} ({1}) = ", parameter.Name, parameter.Id);
			if (parameter.MinValue != null)
				sb.AppendFormat("{0} <= ", parameter.MinValue);
			sb.AppendFormat("{0}", parameter.TargetValue);
			if (parameter.MaxValue != null)
				sb.AppendFormat(" <= {0}", parameter.MaxValue);
			return sb.ToString();
		}
	}
}
