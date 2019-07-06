using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	public class LinkClickedEventArgs : EventArgs
	{
		public string Link { get; private set; }

		internal LinkClickedEventArgs(string link)
		{
			this.Link = link;
		}
	}

	[Xml.XmlControl]
	public class LinkLabel : Label
	{
		private Color m_normalColor;
		private Font m_normalFont;
		private Color? m_hoverColor;

		[Xml.XmlProperty]
		public string Link { get; set; }

		[Xml.XmlProperty]
		public Color HoverColor { get { return m_hoverColor != null ? (Color)m_hoverColor : this.TextColor; } set { m_hoverColor = value; } }

		[Xml.XmlProperty]
		public Font HoverFont { get; set; }

		[Xml.XmlEvent]
		public event ControlBase.GwenEventHandler<LinkClickedEventArgs> LinkClicked;

		public LinkLabel(ControlBase parent)
			: base(parent)
		{
			m_hoverColor = null;
			HoverFont = null;

			base.HoverEnter += OnHoverEnter;
			base.HoverLeave += OnHoverLeave;
			base.Clicked += OnClicked;
		}

		private void OnHoverEnter(ControlBase control, EventArgs args)
		{
			Cursor = Cursor.Finger;

			m_normalColor = m_Text.TextColor;
			m_Text.TextColor = this.HoverColor;

			if (this.HoverFont != null)
			{
				m_normalFont = m_Text.Font;
				m_Text.Font = this.HoverFont;
			}
		}

		private void OnHoverLeave(ControlBase control, EventArgs args)
		{
			m_Text.TextColor = m_normalColor;

			if (this.HoverFont != null)
			{
				m_Text.Font = m_normalFont;
			}
		}

		private void OnClicked(ControlBase control, ClickedEventArgs args)
		{
			if (LinkClicked != null)
				LinkClicked(this, new LinkClickedEventArgs(this.Link));
		}
	}
}
