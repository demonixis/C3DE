using System;

namespace Gwen.Control.Layout
{
	/// <summary>
	/// Arrange child controls by anchoring them proportionally into the edges of this control.
	/// </summary>
	/// <remarks>
	/// You can control the anchoring process by setting Anchor and AnchorBounds
	/// properties of the child control. You must set an AnchorBounds property of this control to
	/// inform the layout process the default size of the area.
	/// </remarks>
	[Xml.XmlControl]
	public class AnchorLayout : ControlBase
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AnchorLayout"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public AnchorLayout(ControlBase parent)
			: base(parent)
		{
		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size size = availableSize - Padding;

			foreach (ControlBase child in Children)
			{
				child.Measure(size);
			}

			return availableSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			Size size = finalSize - Padding;

			Size initialSize = this.AnchorBounds.Size;

			foreach (ControlBase child in Children)
			{
				Anchor anchor = child.Anchor;
				Rectangle anchorBounds = child.AnchorBounds;

				int left = anchorBounds.Left + (size.Width - initialSize.Width) * anchor.Left / 100;
				int top = anchorBounds.Top + (size.Height - initialSize.Height) * anchor.Top / 100;
				int right = anchorBounds.Right + (size.Width - initialSize.Width) * anchor.Right / 100;
				int bottom = anchorBounds.Bottom + (size.Height - initialSize.Height) * anchor.Bottom / 100;

				child.Arrange(new Rectangle(left, top, right - left + 1, bottom - top + 1));
			}

			return finalSize;
		}
	}
}
