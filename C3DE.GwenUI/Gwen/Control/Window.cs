using System;
using System.Linq;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Movable window with title bar.
	/// </summary>
	[Xml.XmlControl]
	public class Window : WindowBase
	{
		private readonly WindowTitleBar m_TitleBar;
		private Modal m_Modal;

		/// <summary>
		/// Window caption.
		/// </summary>
		[Xml.XmlProperty]
		public string Title { get { return m_TitleBar.Title.Text; } set { m_TitleBar.Title.Text = value; } }

		/// <summary>
		/// Determines whether the window has close button.
		/// </summary>
		[Xml.XmlProperty]
		public bool IsClosable { get { return !m_TitleBar.CloseButton.IsCollapsed; } set { m_TitleBar.CloseButton.IsCollapsed = !value; } }

		/// <summary>
		/// Make window modal and set background color. If alpha value is zero, make background dimmed.
		/// </summary>
		[Xml.XmlProperty]
		public Color ModalBackground
		{
			get
			{
				if (m_Modal != null && m_Modal.BackgroundColor != null)
					return (Color)m_Modal.BackgroundColor;
				else
					return Color.Transparent;
			}
			set
			{
				if (value.A == 0)
					MakeModal(true);
				else
					MakeModal(true, value);
			}
		}

		/// <summary>
		/// Set true to make window modal.
		/// </summary>
		[Xml.XmlProperty]
		public bool Modal { get { return m_Modal != null; } set { MakeModal(); } }

		/// <summary>
		/// Initializes a new instance of the <see cref="Window"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public Window(ControlBase parent)
			: base(parent)
		{
			m_TitleBar = new WindowTitleBar(this);
			m_TitleBar.Height = BaseUnit + 9;
			m_TitleBar.Title.TextColor = Skin.Colors.Window.TitleInactive;
			m_TitleBar.CloseButton.Clicked += CloseButtonPressed;
			m_TitleBar.SendToBack();
			m_TitleBar.Dragged += OnDragged;

			m_DragBar = m_TitleBar;

			m_InnerPanel = new InnerContentControl(this);
			m_InnerPanel.SendToBack();
		}

		public override void Close()
		{
			if (m_Modal != null)
			{
				m_Modal.DelayedDelete();
				m_Modal = null;
			}

			base.Close();
		}

		protected virtual void CloseButtonPressed(ControlBase control, EventArgs args)
		{
			Close();
		}

		/// <summary>
		/// Makes the window modal: covers the whole canvas and gets all input.
		/// </summary>
		/// <param name="dim">Determines whether all the background should be dimmed.</param>
		/// <param name="backgroundColor">Determines background color.</param>
		public void MakeModal(bool dim = false, Color? backgroundColor = null)
		{
			if (m_Modal != null)
				return;

			m_Modal = new Modal(GetCanvas());
			Parent = m_Modal;

			if (dim)
				m_Modal.ShouldDrawBackground = true;
			else
				m_Modal.ShouldDrawBackground = false;

			if (backgroundColor != null)
			{
				m_Modal.ShouldDrawBackground = true;
				m_Modal.BackgroundColor = backgroundColor;
			}
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			bool hasFocus = IsOnTop;

			if (hasFocus)
				m_TitleBar.Title.TextColor = Skin.Colors.Window.TitleActive;
			else
				m_TitleBar.Title.TextColor = Skin.Colors.Window.TitleInactive;

			skin.DrawWindow(this, m_TitleBar.ActualHeight, hasFocus);
		}

		/// <summary>
		/// Renders under the actual control (shadows etc).
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderUnder(Skin.SkinBase skin)
		{
			base.RenderUnder(skin);
			skin.DrawShadow(this);
		}

		/// <summary>
		/// Renders the focus overlay.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void RenderFocus(Skin.SkinBase skin)
		{
			
		}

		protected override Size OnMeasure(Size availableSize)
		{
			Size titleBarSize = m_TitleBar.Measure(new Size(availableSize.Width, availableSize.Height));

			if (m_InnerPanel != null)
				m_InnerPanel.Measure(new Size(availableSize.Width, availableSize.Height - titleBarSize.Height));

			return base.OnMeasure(new Size(m_InnerPanel.MeasuredSize.Width, m_InnerPanel.MeasuredSize.Height + titleBarSize.Height));
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_TitleBar.Arrange(new Rectangle(0, 0, finalSize.Width, m_TitleBar.MeasuredSize.Height));

			if (m_InnerPanel != null)
				m_InnerPanel.Arrange(new Rectangle(0, m_TitleBar.MeasuredSize.Height, finalSize.Width, finalSize.Height - m_TitleBar.MeasuredSize.Height));

			return base.OnArrange(finalSize);
		}

		public override void EnableResizing(bool left = true, bool top = true, bool right = true, bool bottom = true)
		{
			base.EnableResizing(left, false, right, bottom);
		}

		public override void Dispose()
		{
			if (m_Modal != null)
			{
				m_Modal.DelayedDelete();
				m_Modal = null;
			}
			else
			{
				base.Dispose();
			}
		}
	}
}
