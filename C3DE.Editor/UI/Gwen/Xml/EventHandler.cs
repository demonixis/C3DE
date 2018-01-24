using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Gwen.Xml
{
	/// <summary>
	/// XML based event handler.
	/// </summary>
	/// <typeparam name="T">Type of event arguments.</typeparam>
	public class XmlEventHandler <T> where T : System.EventArgs
	{
		private string m_eventName;
		private string m_handlerName;
		private Type[] m_paramsType = new Type[] { typeof(Gwen.Control.ControlBase), typeof(T) };

		public XmlEventHandler(string handlerName, string eventName)
		{
			m_eventName = eventName;
			m_handlerName = handlerName;
		}

		public void OnEvent(Gwen.Control.ControlBase sender, T args)
		{
			Gwen.Control.ControlBase handlerElement = sender.Parent;

			if (sender is Gwen.Control.Window)
				handlerElement = sender;
			else if (sender is Gwen.Control.TreeNode)
				handlerElement = ((Gwen.Control.TreeNode)sender).TreeControl.Parent;

			while (handlerElement != null)
			{
				if (handlerElement.Component != null)
				{
					if (handlerElement.Component.HandleEvent(m_eventName, m_handlerName, sender, args))
					{
						break;
					}
					else
					{
						Type type = handlerElement.Component.GetType();

						MethodInfo methodInfo = null;
						do
						{
							MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
							foreach (MethodInfo mi in methods)
							{
								if (mi.Name != m_handlerName)
									continue;
								ParameterInfo[] parameters = mi.GetParameters();
								if (parameters.Length != 2)
									continue;
								if (parameters[0].ParameterType != typeof(Gwen.Control.ControlBase) || (parameters[1].ParameterType != typeof(T) && parameters[1].ParameterType != typeof(T).BaseType))
									continue;
								methodInfo = mi;
								break;
							}
							if (methodInfo != null)
								break;
							type = type.BaseType;
						}
						while (type != null);

						if (methodInfo != null)
						{
							methodInfo.Invoke(handlerElement.Component, new object[] { sender, args });
							break;
						}
					}
				}

				if (handlerElement is Gwen.Control.Menu)
				{
					Gwen.Control.Menu menu = handlerElement as Gwen.Control.Menu;
					if (menu.ParentMenuItem != null)
						handlerElement = menu.ParentMenuItem;
					else
						handlerElement = handlerElement.Parent;
				}
				else
				{
					handlerElement = handlerElement.Parent;
				}
			}
		}
	}
}
