using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	/// <summary>
	/// Control with multiple tabs that can be reordered and dragged.
	/// </summary>
	[Xml.XmlControl(CustomHandler = "XmlElementHandler")]
	public class TabControl : ContentControl
	{
		private readonly TabStrip m_TabStrip;
		private readonly ScrollBarButton[] m_Scroll;
		private TabButton m_CurrentButton;

		private Padding m_ActualPadding;

		/// <summary>
		/// Invoked when a tab has been added.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> TabAdded;

		/// <summary>
		/// Invoked when a tab has been removed.
		/// </summary>
		[Xml.XmlEvent]
		public event GwenEventHandler<EventArgs> TabRemoved;

		/// <summary>
		/// Determines if tabs can be reordered by dragging.
		/// </summary>
		[Xml.XmlProperty]
		public bool AllowReorder { get { return m_TabStrip.AllowReorder; } set { m_TabStrip.AllowReorder = value; } }

		/// <summary>
		/// Currently active tab button.
		/// </summary>
		public TabButton CurrentButton { get { return m_CurrentButton; } }

		/// <summary>
		/// Current tab strip position.
		/// </summary>
		[Xml.XmlProperty]
		public Dock TabStripPosition { get { return m_TabStrip.StripPosition; }set { m_TabStrip.StripPosition = value; } }

		/// <summary>
		/// Tab strip.
		/// </summary>
		public TabStrip TabStrip { get { return m_TabStrip; } }

		/// <summary>
		/// Number of tabs in the control.
		/// </summary>
		public int TabCount { get { return m_TabStrip.Children.Count; } }

		// Ugly way to implement padding but other ways would be more complicated
		[Xml.XmlProperty]
		public override Padding Padding
		{
			get
			{
				return m_ActualPadding;
			}
			set
			{
				m_ActualPadding = value;

				foreach (ControlBase tab in m_TabStrip.Children)
				{
					tab.Margin = (Margin)value;
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="TabControl"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public TabControl(ControlBase parent)
			: base(parent)
		{
			m_TabStrip = new TabStrip(this);
			m_TabStrip.StripPosition = Dock.Top;

			// Actually these should be inside the TabStrip but it would make things complicated
			// because TabStrip contains only TabButtons. ScrollButtons being here we don't need
			// an inner panel for TabButtons on the TabStrip.
			m_Scroll = new ScrollBarButton[2];

			m_Scroll[0] = new ScrollBarButton(this);
			m_Scroll[0].SetDirectionLeft();
			m_Scroll[0].Clicked += ScrollPressedLeft;
			m_Scroll[0].Size = new Size(BaseUnit);

			m_Scroll[1] = new ScrollBarButton(this);
			m_Scroll[1].SetDirectionRight();
			m_Scroll[1].Clicked += ScrollPressedRight;
			m_Scroll[1].Size = new Size(BaseUnit);

			m_InnerPanel = new TabControlInner(this);
			m_InnerPanel.Dock = Dock.Fill;
			m_InnerPanel.SendToBack();

			IsTabable = false;

			m_ActualPadding = new Padding(6, 6, 6, 6);
		}

		/// <summary>
		/// Adds a new page/tab.
		/// </summary>
		/// <param name="label">Tab label.</param>
		/// <param name="page">Page contents.</param>
		/// <returns>Newly created control.</returns>
		public TabButton AddPage(string label, ControlBase page = null)
		{
			if (null == page)
			{
				page = new Layout.DockLayout(this);
			}
			else
			{
				page.Parent = this;
			}

			TabButton button = new TabButton(m_TabStrip);
			button.Text = label;
			button.Page = page;
			button.IsTabable = false;

			AddPage(button);
			return button;
		}

		/// <summary>
		/// Adds a page/tab.
		/// </summary>
		/// <param name="button">Page to add. (well, it's a TabButton which is a parent to the page).</param>
		internal void AddPage(TabButton button)
		{
			ControlBase page = button.Page;
			page.Parent = this;
			page.IsHidden = true;
			page.Dock = Dock.Fill;
			page.Margin = (Margin)this.Padding;

			button.Parent = m_TabStrip;
			if (button.TabControl != null)
				button.TabControl.UnsubscribeTabEvent(button);
			button.TabControl = this;
			button.Clicked += OnTabPressed;

			if (null == m_CurrentButton)
			{
				button.Press();
			}

			if (TabAdded != null)
				TabAdded.Invoke(this, EventArgs.Empty);

			Invalidate();
		}

		private void UnsubscribeTabEvent(TabButton button)
		{
			button.Clicked -= OnTabPressed;
		}

		/// <summary>
		/// Handler for tab selection.
		/// </summary>
		/// <param name="control">Event source (TabButton).</param>
		internal virtual void OnTabPressed(ControlBase control, EventArgs args)
		{
			TabButton button = control as TabButton;
			if (null == button) return;

			ControlBase page = button.Page;
			if (null == page) return;

			if (m_CurrentButton == button)
				return;

			if (null != m_CurrentButton)
			{
				ControlBase page2 = m_CurrentButton.Page;
				if (page2 != null)
				{
					page2.Hide();
				}
				m_CurrentButton.Redraw();
				m_CurrentButton = null;
			}

			m_CurrentButton = button;

			page.Show();
		}

		protected override Size OnArrange(Size finalSize)
		{
			Size size = base.OnArrange(finalSize);

			// At this point we know TabStrip location so lets move ScrollButtons
			int buttonSize = m_Scroll[0].Size.Width;
			switch (m_TabStrip.StripPosition)
			{
				case Dock.Top:
					m_Scroll[0].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize - buttonSize, m_TabStrip.ActualTop + 5);
					m_Scroll[1].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize, m_TabStrip.ActualTop + 5);
					m_Scroll[0].SetDirectionLeft();
					m_Scroll[1].SetDirectionRight();
					break;
				case Dock.Bottom:
					m_Scroll[0].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize - buttonSize, m_TabStrip.ActualBottom - 5 - buttonSize);
					m_Scroll[1].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize, m_TabStrip.ActualBottom - 5 - buttonSize);
					m_Scroll[0].SetDirectionLeft();
					m_Scroll[1].SetDirectionRight();
					break;
				case Dock.Left:
					m_Scroll[0].SetPosition(m_TabStrip.ActualLeft + 5, m_TabStrip.ActualBottom - 5 - buttonSize - buttonSize);
					m_Scroll[1].SetPosition(m_TabStrip.ActualLeft + 5, m_TabStrip.ActualBottom - 5 - buttonSize);
					m_Scroll[0].SetDirectionUp();
					m_Scroll[1].SetDirectionDown();
					break;
				case Dock.Right:
					m_Scroll[0].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize, m_TabStrip.ActualBottom - 5 - buttonSize - buttonSize);
					m_Scroll[1].SetPosition(m_TabStrip.ActualRight - 5 - buttonSize, m_TabStrip.ActualBottom - 5 - buttonSize);
					m_Scroll[0].SetDirectionUp();
					m_Scroll[1].SetDirectionDown();
					break;
			}

			return size;
		}

		/// <summary>
		/// Handler for tab removing.
		/// </summary>
		/// <param name="button"></param>
		internal virtual void OnLoseTab(TabButton button)
		{
			if (m_CurrentButton == button)
				m_CurrentButton = null;

			if (TabCount > 0)
			{
				button = m_TabStrip.Children[0] as TabButton;
				if (button != null)
				{
					button.Page.Show();
					m_CurrentButton = button;
				}
			}

			if (TabRemoved != null)
				TabRemoved.Invoke(this, EventArgs.Empty);

			Invalidate();
		}

		protected override void OnBoundsChanged(Rectangle oldBounds)
		{
			bool needed = false;

			switch (TabStripPosition)
			{
				case Dock.Top:
				case Dock.Bottom:
					needed = m_TabStrip.TotalSize.Width > ActualWidth;
					break;
				case Dock.Left:
				case Dock.Right:
					needed = m_TabStrip.TotalSize.Height > ActualHeight;
					break;
			}

			m_Scroll[0].IsHidden = !needed;
			m_Scroll[1].IsHidden = !needed;

			base.OnBoundsChanged(oldBounds);
		}

		protected virtual void ScrollPressedLeft(ControlBase control, EventArgs args)
		{
			m_TabStrip.ScrollOffset--;
		}

		protected virtual void ScrollPressedRight(ControlBase control, EventArgs args)
		{
			m_TabStrip.ScrollOffset++;
		}

		internal static ControlBase XmlElementHandler(Xml.Parser parser, Type type, ControlBase parent)
		{
			TabControl element = new TabControl(parent);
			parser.ParseAttributes(element);
			if (parser.MoveToContent())
			{
				foreach (string elementName in parser.NextElement())
				{
					if (elementName == "TabPage")
					{
						string pageLabel = parser.GetAttribute("Text");
						if (pageLabel == null)
							pageLabel = "";

						string pageName = parser.GetAttribute("Name");
						if (pageName == null)
							pageName = "";

						TabButton button = element.AddPage(pageLabel);
						button.Name = pageName;

						ControlBase page = button.Page;
						parser.ParseContainerContent(page);
					}
				}
			}
			return element;
		}
	}
}
