using System;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Arrange child controls into a column.
	/// </summary>
	[Xml.XmlControl]
	public class VerticalLayout : StackLayout
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="VerticalLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public VerticalLayout(ControlBase parent)
			: base(parent)
		{
			this.HorizontalAlignment = HorizontalAlignment.Stretch;
			this.VerticalAlignment = VerticalAlignment.Top;
		}
	}
}
