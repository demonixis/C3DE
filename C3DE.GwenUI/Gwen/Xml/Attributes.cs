using System;

namespace Gwen.Xml
{
	/// <summary>
	/// Attribute to indicate that a property is usable from XML.
	/// </summary>
	public class XmlPropertyAttribute : Attribute
	{
	}

	/// <summary>
	/// Attribute to indicate that a event is usable from XML.
	/// </summary>
	public class XmlEventAttribute : Attribute
	{
	}

	/// <summary>
	/// Attribute to indicate that a control is able to be created from XML.
	/// </summary>
	public class XmlControlAttribute : Attribute
	{
		/// <summary>
		/// Name of XML element. Default is a class name.
		/// </summary>
		public string ElementName { get; set; }

		/// <summary>
		/// Function name of the custom handler for the element creation.
		/// </summary>
		public string CustomHandler { get; set; }
	}
}
