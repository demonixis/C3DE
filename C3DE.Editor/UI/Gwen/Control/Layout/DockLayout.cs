using System;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Dock child controls into the edges of this control. This is controlled by
	/// the Dock property of the child control.
	/// </summary>
	[Xml.XmlControl]
	public class DockLayout : ControlBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DockLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public DockLayout(ControlBase parent)
			: base(parent)
		{
		}
	}
}
