using System;
using System.Runtime.Serialization;

namespace MATS.Module.RecipeManagerPlus.ArchestrA.Web
{
	/// <summary>
	/// Represents an error that occurs with the <see cref="IRestAPI"/> interface.
	/// </summary>
	[Serializable]
	public class RestAPIException : Exception, ISerializable
	{
		/// <summary>Gets or sets the XML resource associated with the error.</summary>
		public Resource Resource { get; set; }

		/// <summary>Gets or sets the HTTP status code.</summary>
		public int HttpStatus { get; set; }

		/// <summary>Creates a new exception instance.</summary>
		public RestAPIException()
		{
		}

		/// <summary>Creates a new exception instance.</summary>
		/// <param name="errorMessage">The error message for the exception.</param>
		public RestAPIException(string errorMessage)
			: base(errorMessage)
		{
		}

		/// <summary>Creates a new exception instance.</summary>
		/// <param name="errorMessage">The error message for the exception.</param>
		/// <param name="httpStatus">The HTTP status code.</param>
		public RestAPIException(string errorMessage, int httpStatus)
			: base(errorMessage)
		{
			this.HttpStatus = httpStatus;
		}

		/// <summary>Creates a new exception instance.</summary>
		/// <param name="errorMessage">The error message for the exception.</param>
		/// <param name="exception">The exception that is the cause for this exception.</param>
		public RestAPIException(string errorMessage, Exception exception)
			: base(errorMessage, exception)
		{
		}

		/// <summary>Creates a new exception instance.</summary>
		/// <param name="errorMessage">The error message for the exception.</param>
		/// <param name="resource">The XML resource associated with the error.</param>
		public RestAPIException(string errorMessage, Resource resource)
			: base(errorMessage)
		{
			this.Resource = resource;
			this.HttpStatus = ((resource != null) ? resource.HttpStatus : 0);
		}

		/// <summary>Creates a new exception instance with serialized data.</summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		protected RestAPIException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.HttpStatus = info.GetInt32("HttpStatus");
			var resource = info.GetString("Resource");
			if (!string.IsNullOrWhiteSpace(resource))
			{
				this.Resource = new Resource(resource, this.HttpStatus);
			}
		}

		/// <summary>Sets the <see cref="System.Runtime.Serialization.SerializationInfo"/> with information about the exception.</summary>
		/// <param name="info">The <see cref="System.Runtime.Serialization.SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
		/// <param name="context">The <see cref="System.Runtime.Serialization.StreamingContext"/> that contains contextual information about the source or destination.</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("HttpStatus", this.HttpStatus);
			base.GetObjectData(info, context);
			info.AddValue("Resource", this.Resource != null ? this.Resource.ToXml() : "");
		}
	}
}
