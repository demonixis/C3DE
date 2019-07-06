using System;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// FlowLayout is a layout like <see cref="GridLayout"/> with auto sized columns
	/// but you don't need to know exact number of columns.
	/// </summary>
	[Xml.XmlControl]
	public class FlowLayout : ControlBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FlowLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public FlowLayout(ControlBase parent)
			: base(parent)
		{
		}

		protected override Size OnMeasure(Size availableSize)
		{
			availableSize -= Padding;

			int lineWidth = 0;
			int lineHeight = 0;
			int width = 0;
			int height = 0;
			int y = 0;

			foreach (ControlBase child in Children)
			{
				Size size = child.Measure(availableSize);

				if ((lineWidth + size.Width) > availableSize.Width)
				{
					y += lineHeight;

					lineWidth = size.Width;
					lineHeight = size.Height;
				}
				else
				{
					lineWidth += size.Width;
					lineHeight = Math.Max(lineHeight, size.Height);
				}

				width = Math.Max(width, lineWidth);
				height = Math.Max(height, y + size.Height);
			}

			width = Math.Max(width, lineWidth);
			height = Math.Max(height, lineHeight);

			return new Size(width, height) + Padding;
		}

		protected override Size OnArrange(Size finalSize)
		{
			finalSize -= Padding;

			int lineHeight = 0;
			int width = 0;
			int height = 0;
			int x = 0;
			int y = 0;

			foreach (ControlBase child in Children)
			{
				if ((x + child.MeasuredSize.Width) > finalSize.Width)
				{
					y += lineHeight;
					x = 0;

					lineHeight = 0;
				}

				child.Arrange(new Rectangle(x + Padding.Left, y + Padding.Top, child.MeasuredSize.Width, child.MeasuredSize.Height));
				width = Math.Max(width, x + child.MeasuredSize.Width);
				height = Math.Max(height, y + child.MeasuredSize.Height);

				x += child.MeasuredSize.Width;
				lineHeight = Math.Max(lineHeight, child.MeasuredSize.Height);
			}

			return new Size(width, height) + Padding;
		}
	}
}
