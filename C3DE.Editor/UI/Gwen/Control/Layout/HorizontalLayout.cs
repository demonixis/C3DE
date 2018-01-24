using System;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Arrange child controls into a row.
	/// </summary>
	[Xml.XmlControl]
	public class HorizontalLayout : StackLayout
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="HorizontalLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public HorizontalLayout(ControlBase parent)
			: base(parent)
		{
			this.Horizontal = true;
			this.HorizontalAlignment = HorizontalAlignment.Left;
			this.VerticalAlignment = VerticalAlignment.Stretch;
		}
	}
}
