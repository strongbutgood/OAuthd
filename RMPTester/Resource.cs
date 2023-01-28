using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace MATS.Module.RecipeManagerPlus.ArchestrA.Web
{
	/// <summary>
	/// Represents a Recipe Manager Plus entity or set of entities as an XML resource.
	/// </summary>
	[System.Diagnostics.DebuggerDisplay("StatusCode: {HttpStatus}, {ResourceType}")]
	public class Resource
	{
		private XmlNode m_RootNode;

		/// <summary>Gets the type of resource contained.</summary>
		public string ResourceType { get { return this.m_RootNode.Name; } }

		/// <summary>Gets or sets the HTTP status code from a response message.</summary>
		public int HttpStatus { get; set; }

		/// <summary>Creates a new <see cref="Resource"/> containing the specified <paramref name="xml"/> string.</summary>
		/// <param name="xml">The XML string for this resource.</param>
		public Resource(string xml)
			: this(xml, 0)
		{
		}

		/// <summary>Creates a new <see cref="Resource"/> containing the specified <paramref name="xml"/> string.</summary>
		/// <param name="xml">The XML string for this resource.</param>
		/// <param name="httpStatus">The HTTP status code from a response message.</param>
		public Resource(string xml, int httpStatus)
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xml);
			this.m_RootNode = xmlDocument.FirstChild;
			this.HttpStatus = httpStatus;
		}

		/// <summary>Creates a new <see cref="Resource"/> from an <see cref="System.Xml.XmlNode"/>.</summary>
		/// <param name="xmlNode">The XML node for this resource.</param>
		private Resource(XmlNode xmlNode)
		{
			this.HttpStatus = 0;
			this.m_RootNode = xmlNode;
		}

		/// <summary>Gets a property by the specified <paramref name="propertyName"/>.</summary>
		/// <param name="propertyName">The name of the property to get.</param>
		public virtual string GetProperty(string propertyName)
		{
			XmlElement xmlElement = this.GetXmlElement(propertyName);
			return xmlElement.InnerText;
		}

		/// <summary>Gets a property by the specified <paramref name="propertyName"/> as an XML <see cref="Resource"/>.</summary>
		/// <param name="propertyName">The name of the property to get.</param>
		public Resource GetPropertyAsResource(string propertyName)
		{
			XmlElement xmlElement = this.GetXmlElement(propertyName);
			return new Resource(xmlElement);
		}

		/// <summary>Gets a property by the specified <paramref name="propertyName"/> as an array of XML <see cref="Resource"/>s.</summary>
		/// <param name="propertyName">The name of the property to get.</param>
		public Resource[] GetPropertyAsResourceArray(string propertyName)
		{
			XmlElement xmlElement = this.GetXmlElement(propertyName);
			return Resource.CreateArray(xmlElement.ChildNodes);
		}

		/// <summary>Gets a property by the specified <paramref name="propertyName"/> as an array of strings.</summary>
		/// <param name="propertyName">The name of the property to get.</param>
		public string[] GetPropertyAsStringArray(string propertyName)
		{
			List<string> list = new List<string>();
			XmlElement xmlElement = this.GetXmlElement(propertyName);
			foreach (XmlNode xmlNode in xmlElement.ChildNodes)
			{
				list.Add(xmlNode.InnerText);
			}
			return list.ToArray();
		}

		/// <summary>Sets the <paramref name="value"/> of the property specified <paramref name="propertyName"/>.</summary>
		/// <param name="propertyName">The name of the property to set.</param>
		/// <param name="value">The value to set the property to.</param>
		public virtual void SetProperty(string propertyName, string value)
		{
			if (value == null)
			{
				value = "";
			}
			XmlElement xmlElement = this.GetXmlElement(propertyName);
			xmlElement.InnerText = value;
			if (!string.IsNullOrEmpty(value))
			{
				xmlElement.RemoveAllAttributes();
			}
		}

		/// <summary>Gets the underlying <see cref="System.Xml.XmlElement"/> of a property by the specified <paramref name="propertyName"/>.</summary>
		/// <param name="propertyName">The name of the property to get.</param>
		private XmlElement GetXmlElement(string propertyName)
		{
			if (this.m_RootNode[propertyName] == null)
			{
				string errorMessage = string.Format(CultureInfo.InvariantCulture, "The property \"{0}\" does not exist.", new object[]
				{
					propertyName
				});
				throw new RestAPIException(errorMessage);
			}
			return this.m_RootNode[propertyName];
		}

		/// <summary>Converts this resource into an array of resources.</summary>
		public Resource[] ToArray()
		{
			return Resource.CreateArray(this.m_RootNode.ChildNodes);
		}

		/// <summary>Creates an array of XML resources from the list of XML nodes.</summary>
		/// <param name="xmlNodes">The XML nodes to create an array of resources from.</param>
		private static Resource[] CreateArray(XmlNodeList xmlNodes)
		{
			List<Resource> list = new List<Resource>();
			foreach (XmlNode xmlNode in xmlNodes)
			{
				Resource item = new Resource(xmlNode);
				list.Add(item);
			}
			return list.ToArray();
		}

		/// <summary>Returns the contents of the resource as a string.</summary>
		public virtual string ToXml()
		{
			return this.m_RootNode.OuterXml;
		}
	}
}
