using System;

namespace MATS.Module.RecipeManagerPlus.ArchestrA.Web
{
	/// <summary>
	/// Defines the interface for accessing Recipe Manager Plus data.
	/// </summary>
	public interface IRestAPI : IDisposable
	{
		/// <summary>Gets the currently logged in user.</summary>
		string LoggedInUser { get; }

		/// <summary>Requests a resource using the GET method.</summary>
		/// <param name="url">The url identifying the resource to get.</param>
		/// <returns>The response as an XML resource.</returns>
		Resource GetOne(string url);

		/// <summary>Requests many resources using the GET method.</summary>
		/// <param name="url">The url identifying the resources to get.</param>
		/// <returns>The response as an array of XML resources.</returns>
		Resource[] GetMany(string url);

		/// <summary>Sends a POST request.</summary>
		/// <param name="url">The url to send the POST to.</param>
		/// <returns>The response as an XML resource.</returns>
		Resource Post(string url);

		/// <summary>Sends a resource as a POST request.</summary>
		/// <param name="url">The url to POST the request to.</param>
		/// <param name="resource">The resource to send.</param>
		/// <returns>The response as an XML resource.</returns>
		Resource Post(string url, Resource resource);

		/// <summary>Sends a PUT request.</summary>
		/// <param name="url">The url to send the PUT to.</param>
		/// <returns>The response as an XML resource.</returns>
		Resource Put(string url);

		/// <summary>Sends a resource as a PUT request.</summary>
		/// <param name="url">The url to PUT the request to.</param>
		/// <param name="resource">The resource to send.</param>
		/// <returns>The response as an XML resource.</returns>
		Resource Put(string url, Resource resource);
	}
}
