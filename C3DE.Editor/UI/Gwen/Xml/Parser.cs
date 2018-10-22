using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Reflection;
using System.Linq;

namespace Gwen.Xml
{
	public delegate Gwen.Control.ControlBase ElementHandler(Parser parser, Type type, Gwen.Control.ControlBase parent);
	public delegate object AttributeValueConverter(object element, string value);
	public delegate Delegate EventHandlerConverter(string attribute, string value);

	/// <summary>
	/// XML parser for creating controls and components using XML.
	/// </summary>
	public class Parser : IDisposable
	{
		private static Dictionary<string, ElementDef> m_ElementHandlers = new Dictionary<string, ElementDef>();

		private static Dictionary<Type, AttributeValueConverter> m_AttributeValueConverters = new Dictionary<Type, AttributeValueConverter>();
		private static Dictionary<Type, EventHandlerConverter> m_EventHandlerConverters = new Dictionary<Type, EventHandlerConverter>();

		private XmlReader m_Reader;

		private ElementDef m_CurrentElement;

		/// <summary>
		/// Current XML node name.
		/// </summary>
		public string Name { get { return m_Reader.Name; } }

		static Parser()
		{
			XmlHelper.RegisterDefaultHandlers();

			Assembly assembly = typeof(Gwen.Control.ControlBase).Assembly;
			if (assembly != null)
				ScanControls(assembly);
		}

		/// <summary>
		/// Register a XML element. All XML elements must be registered before usage. 
		/// </summary>
		/// <param name="name">Name of the element.</param>
		/// <param name="type">Type of the control or component.</param>
		/// <param name="handler">Handler function for creating the control or component.</param>
		/// <returns>True if registered successfully or false is already registered.</returns>
		public static bool RegisterElement(string name, Type type, ElementHandler handler)
		{
			if (!m_ElementHandlers.ContainsKey(name))
			{
				ElementDef elementDef = new ElementDef(type, handler);

				m_ElementHandlers[name] = elementDef;

				ScanProperties(elementDef);
				ScanEvents(elementDef);

				return true;
			}

			return false;
		}

		/// <summary>
		/// Remove a XML element registration. After this the element is not usable anymore.
		/// </summary>
		/// <param name="name">Name of the element.</param>
		/// <returns>True if unregistered successfully.</returns>
		public static bool UnregisterElement(string name)
		{
			if (m_ElementHandlers.ContainsKey(name))
			{
				m_ElementHandlers.Remove(name);
				return true;
			}

			return false;
		}

		/// <summary>
		/// Register an attribute value converter for a property value type. All types of properties must be registered
		/// to be able to be created using XML.
		/// </summary>
		/// <param name="type">Value type.</param>
		/// <param name="converter">Converter function.</param>
		public static void RegisterAttributeValueConverter(Type type, AttributeValueConverter converter)
		{
			if (!m_AttributeValueConverters.ContainsKey(type))
				m_AttributeValueConverters[type] = converter;
		}

		/// <summary>
		/// Register an event argument converter. All types of event arguments must be registered before usage.
		/// </summary>
		/// <param name="type">Event argument type.</param>
		/// <param name="converter">Converter function.</param>
		// Todo: Is this necessary? Maybe it could be avoided using reflection?
		public static void RegisterEventHandlerConverter(Type type, EventHandlerConverter converter)
		{
			if (!m_EventHandlerConverters.ContainsKey(type))
				m_EventHandlerConverters[type] = converter;
		}

		/// <summary>
		/// Parser constructor.
		/// </summary>
		/// <param name="stream">XML stream.</param>
		public Parser(Stream stream)
		{
			m_Reader = XmlReader.Create(stream);
        }

		protected Parser() { }

		/// <summary>
		/// Parse XML.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		/// <returns>XML root control.</returns>
		public Gwen.Control.ControlBase Parse(Gwen.Control.ControlBase parent)
		{
			Gwen.Control.ControlBase container = null;

			while (m_Reader.Read())
			{
				switch (m_Reader.NodeType)
				{
					case XmlNodeType.Element:
						container = ParseElement(parent);
						break;
				}
			}

			return container;
		}

		/// <summary>
		/// Parse element and call it's handler.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		/// <returns>Control.</returns>
		public Gwen.Control.ControlBase ParseElement(Gwen.Control.ControlBase parent)
		{
			ElementDef elementDef;
			if (m_ElementHandlers.TryGetValue(m_Reader.Name, out elementDef))
			{
				m_CurrentElement = elementDef;

				return elementDef.Handler(this, elementDef.Type, parent);
			}

			return null;
		}

		/// <summary>
		/// Parse typed element and call it's handler.
		/// </summary>
		/// <typeparam name="T">Control type to be created.</typeparam>
		/// <param name="parent">Parent control.</param>
		/// <returns>Control.</returns>
		public T ParseElement<T>(Gwen.Control.ControlBase parent) where T: Gwen.Control.ControlBase
		{
			Type type = typeof(T);
			XmlControlAttribute attrib = null;
			object[] attribs = type.GetCustomAttributes(typeof(XmlControlAttribute), false);
			if (attribs.Length > 0)
				attrib = attribs[0] as XmlControlAttribute;

			ElementDef elementDef;
			if (m_ElementHandlers.TryGetValue(attrib != null && attrib.ElementName != null ? attrib.ElementName : type.Name, out elementDef))
			{
				if (elementDef.Type == type)
				{
					m_CurrentElement = elementDef;
					return elementDef.Handler(this, elementDef.Type, parent) as T;
				}
			}

			return null;
		}

		/// <summary>
		/// Parse attributes.
		/// </summary>
		/// <param name="element">Control.</param>
		public void ParseAttributes(Gwen.Control.ControlBase element)
		{
			if (m_Reader.HasAttributes)
			{
				while (m_Reader.MoveToNextAttribute())
				{
					if (m_CurrentElement != null)
					{
						if (!SetAttributeValue(m_CurrentElement, element, m_Reader.Name, m_Reader.Value))
							throw new XmlException(String.Format("Attribute '{0}' not found.", m_Reader.Name));
					}
					else
					{
						throw new XmlException("Trying to set an attribute value without an element.");
					}
				}

				m_Reader.MoveToElement();
			}
		}

		internal void ParseComponentAttributes(Component component)
		{
			Type componentType = component.GetType();
			Type viewType = component.View.GetType();

			ElementDef componentDef = null;
			ElementDef viewDef = null;

			foreach (ElementDef elementDef in m_ElementHandlers.Values)
			{
				if (elementDef.Type == componentType)
					componentDef = elementDef;
				else if (elementDef.Type == viewType)
					viewDef = elementDef;
			}

			if (componentDef == null)
				throw new XmlException("Component is not registered.");
			if (viewDef == null)
				throw new XmlException("Component view is not registered.");

			if (m_Reader.HasAttributes)
			{
				while (m_Reader.MoveToNextAttribute())
				{
					if (!SetAttributeValue(componentDef, component, m_Reader.Name, m_Reader.Value))
					{
						if (!SetAttributeValue(viewDef, component.View, m_Reader.Name, m_Reader.Value))
						{
							if (!SetComponentAttribute(component, m_Reader.Name, m_Reader.Value))
							{
								throw new XmlException(String.Format("Attribute '{0}' not found.", m_Reader.Name));
							}
						}
					}
				}

				m_Reader.MoveToElement();
			}
		}
		
		private bool SetComponentAttribute(Component component, string attribute, string value)
		{
			Type type;
			if (component.GetValueType(attribute, out type))
			{
				if (type == null)
				{
					if (component.SetValue(attribute, value))
						return true;
				}
				else
				{
					AttributeValueConverter attributeConverter;
					if (m_AttributeValueConverters.TryGetValue(type, out attributeConverter))
					{
						if (component.SetValue(attribute, attributeConverter(component, value)))
							return true;
					}

					EventHandlerConverter eventConverter;
					if (m_EventHandlerConverters.TryGetValue(type, out eventConverter))
					{
						if (component.SetValue(attribute, eventConverter(attribute, value)))
							return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Check that the current element contains a content and moves to it.
		/// </summary>
		/// <returns>True if the element contains a content. False otherwise.</returns>
		public bool MoveToContent()
		{
			if (!m_Reader.IsEmptyElement)
			{
				m_Reader.MoveToContent();

				return true;
			}

			return false;
		}

		/// <summary>
		/// Parse content of the container element.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public void ParseContainerContent(Gwen.Control.ControlBase parent)
		{
			foreach (string elementName in NextElement())
			{
				ParseElement(parent);
			}
		}

		/// <summary>
		/// Enumerate content of the container element.
		/// </summary>
		/// <returns>Element name.</returns>
		public IEnumerable<string> NextElement()
		{
			while (m_Reader.Read())
			{
				switch (m_Reader.NodeType)
				{
					case XmlNodeType.Element:
						yield return m_Reader.Name;
						break;
					case XmlNodeType.EndElement:
						yield break;
				}
			}
		}

		/// <summary>
		/// Get attribute value.
		/// </summary>
		/// <param name="attribute">Attribute name.</param>
		/// <returns>Attribute value. Null if an empty attribute or attribute not found.</returns>
		public string GetAttribute(string attribute)
		{
			return m_Reader.GetAttribute(attribute);
		}

		private bool SetAttributeValue(ElementDef elementDef, object element, string attribute, string value)
		{
			MemberInfo memberInfo = elementDef.GetAttribute(attribute);
			if (memberInfo != null)
			{
				if (memberInfo is PropertyInfo)
				{
					return SetPropertyValue(element, memberInfo as PropertyInfo, value);
				}
				else if (memberInfo is EventInfo)
				{
					return SetEventValue(element, memberInfo as EventInfo, value);
				}
			}

			return false;
		}

		private bool SetPropertyValue(object element, PropertyInfo propertyInfo, string value)
		{
			AttributeValueConverter converter;
			if (m_AttributeValueConverters.TryGetValue(propertyInfo.PropertyType, out converter))
			{
				propertyInfo.SetValue(element, converter(element, value), null);
				return true;
			}

			throw new XmlException(String.Format("No converter found for an attribute '{0}' value type '{1}'.", propertyInfo.Name, propertyInfo.PropertyType.Name));
		}

		private bool SetEventValue(object element, EventInfo eventInfo, string value)
		{
			if (eventInfo.EventHandlerType.IsGenericType)
			{
				Type[] ga = eventInfo.EventHandlerType.GetGenericArguments();
				if (ga.Length == 1)
				{
					EventHandlerConverter converter;
					if (m_EventHandlerConverters.TryGetValue(ga[0], out converter))
					{
						eventInfo.AddEventHandler(element, converter(eventInfo.Name, value));
						return true;
					}
					else
					{
						throw new XmlException(String.Format("No event handler converter found for an event '{0}' event args type '{1}'.", eventInfo.Name, ga[0].Name));
					}
				}
			}

			throw new XmlException(String.Format("Event '{0}' is not a Gwen event", eventInfo.Name));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (m_Reader != null)
				{
					m_Reader.Dispose();
					((IDisposable)m_Reader).Dispose();
					m_Reader = null;
				}
			}
		}

		private static Gwen.Control.ControlBase DefaultElementHandler(Parser parser, Type type, Gwen.Control.ControlBase parent)
		{
			Gwen.Control.ControlBase element = Activator.CreateInstance(type, parent) as Gwen.Control.ControlBase;

			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				parser.ParseContainerContent(element);
			}

			return element;
		}

		/// <summary>
		/// Scan an assembly to find all controls that can be created using XML.
		/// </summary>
		/// <param name="assembly">Assembly.</param>
		public static void ScanControls(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes().Where(t => t.IsDefined(typeof(XmlControlAttribute), false)))
			{
				object[] attribs = type.GetCustomAttributes(typeof(XmlControlAttribute), false);
				if (attribs.Length > 0)
				{
					XmlControlAttribute attrib = attribs[0] as XmlControlAttribute;
					if (attrib != null)
					{
						ElementHandler handler;
						if (attrib.CustomHandler != null)
						{
							MethodInfo mi = type.GetMethod(attrib.CustomHandler, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
							if (mi != null)
							{
								handler = Delegate.CreateDelegate(typeof(ElementHandler), mi) as ElementHandler;
							}
							else
							{
								throw new XmlException("Elemant handler not found.");
							}
						}
						else
						{
							handler = DefaultElementHandler;
						}

						string name = attrib.ElementName != null ? attrib.ElementName : type.Name;

						RegisterElement(name, type, handler);
					}
				}
			}
		}

		private static void ScanProperties(ElementDef elementDef)
		{
			foreach (var propertyInfo in elementDef.Type.GetProperties().Where(pi => pi.IsDefined(typeof(XmlPropertyAttribute), false)))
			{
				if (m_AttributeValueConverters.ContainsKey(propertyInfo.PropertyType))
					elementDef.AddAttribute(propertyInfo.Name, propertyInfo);
				else
					throw new XmlException(String.Format("No converter found for an attribute '{0}' value type '{1}'.", propertyInfo.Name, propertyInfo.PropertyType.Name));
			}
		}

		private static void ScanEvents(ElementDef elementDef)
		{
			foreach (var eventInfo in elementDef.Type.GetEvents().Where(ei => ei.IsDefined(typeof(XmlEventAttribute), false)))
			{
				elementDef.AddAttribute(eventInfo.Name, eventInfo);
			}
		}

		/// <summary>
		/// Get list of controls that can be created using XML.
		/// </summary>
		/// <returns></returns>
		public static Dictionary<string, Type> GetElements()
		{
			Dictionary<string, Type> elements = new Dictionary<string, Type>();

			foreach (var element in m_ElementHandlers)
			{
				elements[element.Key] = element.Value.Type;
			}

			return elements;
		}

		/// <summary>
		/// Get list of properties that can be set using XML.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static Dictionary<string, MemberInfo> GetAttributes(string element)
		{
			ElementDef elementDef;
			if (m_ElementHandlers.TryGetValue(element, out elementDef))
			{
				Dictionary<string, MemberInfo> attributes = new Dictionary<string, MemberInfo>();

				foreach (var attribute in elementDef.Attributes)
				{
					attributes[attribute.Key] = attribute.Value;
				}

				return attributes;
			}

			return null;
		}

		public readonly static NumberFormatInfo NumberFormatInfo = new NumberFormatInfo() { NumberGroupSeparator = "" };
		public readonly static char[] ArraySeparator = new char[] { ',' };

		private class ElementDef
		{
			public Type Type { get; set; }
			public ElementHandler Handler { get; set; }

			internal Dictionary<string, MemberInfo> Attributes { get { return m_Attributes; } }

			public ElementDef(Type type, ElementHandler handler)
			{
				Type = type;
				Handler = handler;
			}

			public void AddAttribute(string name, MemberInfo memberInfo)
			{
				m_Attributes[name] = memberInfo;
			}

			public MemberInfo GetAttribute(string name)
			{
				MemberInfo mi;
				if (m_Attributes.TryGetValue(name, out mi))
					return mi;
				else
					return null;
			}

			private Dictionary<string, MemberInfo> m_Attributes = new Dictionary<string, MemberInfo>();
		}
	}
}
