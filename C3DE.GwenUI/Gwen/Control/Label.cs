using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Static text label.
	/// </summary>
	[Xml.XmlControl]
	public class Label : ControlBase
	{
		protected readonly Text m_Text;
		private Alignment m_Align;
		private Padding m_TextPadding;
		private bool m_AutoSizeToContent;

		/// <summary>
		/// Text alignment.
		/// </summary>
		[Xml.XmlProperty]
		public Alignment Alignment { get { return m_Align; } set { m_Align = value; Invalidate(); } }

		/// <summary>
		/// Text.
		/// </summary>
		[Xml.XmlProperty]
		public virtual string Text { get { return m_Text.String; } set { m_Text.String = value; } }

		/// <summary>
		/// Font.
		/// </summary>
		[Xml.XmlProperty]
		public Font Font { get { return m_Text.Font; } set { m_Text.Font = value; Invalidate(); } }

		/// <summary>
		/// Text color.
		/// </summary>
		[Xml.XmlProperty]
		public Color TextColor { get { return m_Text.TextColor; } set { m_Text.TextColor = value; } }

		/// <summary>
		/// Override text color (used by tooltips).
		/// </summary>
		[Xml.XmlProperty]
		public Color TextColorOverride { get { return m_Text.TextColorOverride; } set { m_Text.TextColorOverride = value; } }

		/// <summary>
		/// Text override - used to display different string.
		/// </summary>
		[Xml.XmlProperty]
		public string TextOverride { get { return m_Text.TextOverride; } set { m_Text.TextOverride = value; } }
		
		/// <summary>
		/// Determines if the control should autosize to its text.
		/// </summary>
		[Xml.XmlProperty]
		public bool AutoSizeToContents { get { return m_AutoSizeToContent; } set { m_AutoSizeToContent = value; IsVirtualControl = !value; if (value) Invalidate(); } }

		/// <summary>
		/// Text padding.
		/// </summary>
		[Xml.XmlProperty]
		public Padding TextPadding { get { return m_TextPadding; } set { m_TextPadding = value; Invalidate(); } }

		[Xml.XmlEvent]
		public override event ControlBase.GwenEventHandler<ClickedEventArgs> Clicked {
			add {
				base.Clicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.Clicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		[Xml.XmlEvent]
		public override event ControlBase.GwenEventHandler<ClickedEventArgs> DoubleClicked {
			add {
				base.DoubleClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.DoubleClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		[Xml.XmlEvent]
		public override event ControlBase.GwenEventHandler<ClickedEventArgs> RightClicked {
			add {
				base.RightClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.RightClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}

		[Xml.XmlEvent]
		public override event ControlBase.GwenEventHandler<ClickedEventArgs> DoubleRightClicked {
			add {
				base.DoubleRightClicked += value;
				MouseInputEnabled = ClickEventAssigned;
			}
			remove {
				base.DoubleRightClicked -= value;
				MouseInputEnabled = ClickEventAssigned;
			}
		}


		/// <summary>
		/// Initializes a new instance of the <see cref="Label"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public Label(ControlBase parent) : base(parent)
		{
			m_Text = new Text(this);
			//m_Text.Font = Skin.DefaultFont;

			m_AutoSizeToContent = true;

			MouseInputEnabled = false;
			Alignment = Alignment.Left | Alignment.Top;
		}

		/// <summary>
		/// Returns index of the character closest to specified point (in canvas coordinates).
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		protected virtual Point GetClosestCharacter(int x, int y)
		{ 
			return new Point(m_Text.GetClosestCharacter(m_Text.CanvasPosToLocal(new Point(x, y))), 0); 
		}

		/// <summary>
		/// Handler for text changed event.
		/// </summary>
		protected virtual void OnTextChanged() {}

		protected override Size OnMeasure(Size availableSize)
		{
			return m_Text.Measure(availableSize) + m_TextPadding + Padding;
		}

		protected override Size OnArrange(Size finalSize)
		{
			Size innerSize = finalSize - m_TextPadding - Padding;
			Rectangle rect = new Rectangle(Point.Zero, Size.Min(m_Text.MeasuredSize, innerSize));

			if ((m_Align & Alignment.CenterH) != 0)
				rect.X = (innerSize.Width - m_Text.MeasuredSize.Width) / 2;
			else if ((m_Align & Alignment.Right) != 0)
				rect.X = innerSize.Width - m_Text.MeasuredSize.Width;

			if ((m_Align & Alignment.CenterV) != 0)
				rect.Y = (innerSize.Height - m_Text.MeasuredSize.Height) / 2;
			else if ((m_Align & Alignment.Bottom) != 0)
				rect.Y = innerSize.Height - m_Text.MeasuredSize.Height;

			rect.Offset(m_TextPadding + Padding);

			m_Text.Arrange(rect);

			return finalSize;
		}

		/// <summary>
		/// Gets the coordinates of specified character.
		/// </summary>
		/// <param name="index">Character index.</param>
		/// <returns>Character coordinates (local).</returns>
		public virtual Point GetCharacterPosition(int index)
		{
			Point p = m_Text.GetCharacterPosition(index);
			return new Point(p.X + m_Text.ActualLeft, p.Y + m_Text.ActualTop);
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
		}
	}
}
