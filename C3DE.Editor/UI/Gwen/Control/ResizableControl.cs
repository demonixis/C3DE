using System;
using Gwen.Control.Internal;

namespace Gwen.Control
{
	public enum Resizing
	{
		None,
		Width,
		Height,
		Both,
	}

	/// <summary>
	/// Base resizable control.
	/// </summary>
	public class ResizableControl : ContentControl
	{
		private bool m_ClampMovement;
		private readonly Resizer[] m_Resizer;

		private const int ResizerThickness = 6;

		/// <summary>
		/// Enable or disable resizing.
		/// </summary>
		[Xml.XmlProperty]
		public Resizing Resizing
		{
			get
			{
				if (GetResizer(ResizerPos.Right).IsCollapsed)
				{
					if (GetResizer(ResizerPos.Bottom).IsCollapsed)
						return Resizing.None;
					else
						return Resizing.Height;
				}
				else if (GetResizer(ResizerPos.Bottom).IsCollapsed)
				{
					return Resizing.Width;
				}
				else
				{
					return Resizing.Both;
				}
			}
			set
			{
				switch (value)
				{
					case Resizing.None:
						EnableResizing(false, false, false, false);
						break;
					case Resizing.Width:
						EnableResizing(true, false, true, false);
						break;
					case Resizing.Height:
						EnableResizing(false, true, false, true);
						break;
					case Resizing.Both:
						EnableResizing();
						break;
				}
			}
		}

		/// <summary>
		/// Determines whether control's position should be restricted to its parent bounds.
		/// </summary>
		public bool ClampMovement { get { return m_ClampMovement; } set { m_ClampMovement = value; } }

		/// <summary>
		/// Invoked when the control has been resized.
		/// </summary>
		public event GwenEventHandler<EventArgs> Resized;

		/// <summary>
		/// Initializes a new instance of the <see cref="ResizableControl"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ResizableControl(ControlBase parent)
			: base(parent)
		{
			m_Resizer = new Resizer[8];
			MinimumSize = new Size(5, 5);
			m_ClampMovement = false;

			Resizer resizer;

			resizer = m_Resizer[(int)ResizerPos.Bottom] = new Resizer(this);
			resizer.ResizeDir = Dock.Bottom;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.LeftBottom] = new Resizer(this);
			resizer.ResizeDir = Dock.Bottom | Dock.Left;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.RightBottom] = new Resizer(this);
			resizer.ResizeDir = Dock.Bottom | Dock.Right;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.Top] = new Resizer(this);
			resizer.ResizeDir = Dock.Top;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.LeftTop] = new Resizer(this);
			resizer.ResizeDir = Dock.Left | Dock.Top;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.RightTop] = new Resizer(this);
			resizer.ResizeDir = Dock.Right | Dock.Top;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.Left] = new Resizer(this);
			resizer.ResizeDir = Dock.Left;
			resizer.Resized += OnResized;
			resizer.Target = this;

			resizer = m_Resizer[(int)ResizerPos.Right] = new Resizer(this);
			resizer.ResizeDir = Dock.Right;
			resizer.Resized += OnResized;
			resizer.Target = this;
		}

		/// <summary>
		/// Handler for the resized event.
		/// </summary>
		/// <param name="control">Event source.</param>
		protected virtual void OnResized(ControlBase control, EventArgs args)
		{
			if (Resized != null)
				Resized.Invoke(this, EventArgs.Empty);
		}

		protected Resizer GetResizer(ResizerPos resizerPos)
		{
			return m_Resizer[(int)resizerPos];
		}

		/// <summary>
		/// Enable or disable resizing.
		/// </summary>
		/// <param name="left">Is resizing left edge enabled.</param>
		/// <param name="top">Is resizing top edge enabled.</param>
		/// <param name="right">Is resizing right edge enabled.</param>
		/// <param name="bottom">Is resizing bottom edge enabled.</param>
		public virtual void EnableResizing(bool left = true, bool top = true, bool right = true, bool bottom = true)
		{
			bool[] d = new bool[8];

			if (!left) { d[(int)ResizerPos.Left] = d[(int)ResizerPos.LeftTop] = d[(int)ResizerPos.LeftBottom] = true; }
			if (!top) { d[(int)ResizerPos.Top] = d[(int)ResizerPos.LeftTop] = d[(int)ResizerPos.RightTop] = true; }
			if (!right) { d[(int)ResizerPos.Right] = d[(int)ResizerPos.RightTop] = d[(int)ResizerPos.RightBottom] = true; }
			if (!bottom) { d[(int)ResizerPos.Bottom] = d[(int)ResizerPos.LeftBottom] = d[(int)ResizerPos.RightBottom] = true; }

			for (int i = 0; i <  8; i++)
			{
				if (d[i])
				{
					m_Resizer[i].MouseInputEnabled = false;
					m_Resizer[i].Collapse(true, false);
				}
				else
				{
					m_Resizer[i].MouseInputEnabled = true;
					m_Resizer[i].Collapse(false, false);
				}
			}

			Invalidate();
		}

		/// <summary>
		/// Sets the control bounds.
		/// </summary>
		/// <param name="x">X position.</param>
		/// <param name="y">Y position.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <returns>
		/// True if bounds changed.
		/// </returns>
		public override bool SetBounds(int x, int y, int width, int height)
		{
			width = Util.Clamp(width, MinimumSize.Width, MaximumSize.Width);
			height = Util.Clamp(height, MinimumSize.Height, MaximumSize.Height);

			// Clamp to parent's window
			ControlBase parent = Parent;
			if (parent != null && m_ClampMovement)
			{
				if (x + width > parent.ActualWidth) x = parent.ActualWidth - width;
				if (x < 0) x = 0;
				if (y + height > parent.ActualHeight) y = parent.ActualHeight - height;
				if (y < 0) y = 0;
			}

			return base.SetBounds(x, y, width, height);
		}

		/// <summary>
		/// Sets the control size.
		/// </summary>
		/// <param name="width">New width.</param>
		/// <param name="height">New height.</param>
		/// <returns>True if bounds changed.</returns>
		public override bool SetSize(int width, int height)
		{
			bool Changed = base.SetSize(width, height);
			if (Changed)
			{
				OnResized(this, EventArgs.Empty);
			}
			return Changed;
		}

		protected override Size OnMeasure(Size availableSize)
		{
			m_Resizer[(int)ResizerPos.Left].Measure(new Size(ResizerThickness, availableSize.Height - 2 * ResizerThickness));
			m_Resizer[(int)ResizerPos.LeftTop].Measure(new Size(ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Top].Measure(new Size(availableSize.Width - 2 * ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.RightTop].Measure(new Size(ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Right].Measure(new Size(ResizerThickness, availableSize.Height - 2 * ResizerThickness));
			m_Resizer[(int)ResizerPos.RightBottom].Measure(new Size(ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Bottom].Measure(new Size(availableSize.Width - 2 * ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.LeftBottom].Measure(new Size(ResizerThickness, ResizerThickness));

			return availableSize;
		}

		protected override Size OnArrange(Size finalSize)
		{
			m_Resizer[(int)ResizerPos.Left].Arrange(new Rectangle(0, ResizerThickness, ResizerThickness, finalSize.Height - 2 * ResizerThickness));
			m_Resizer[(int)ResizerPos.LeftTop].Arrange(new Rectangle(0, 0, ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Top].Arrange(new Rectangle(ResizerThickness, 0, finalSize.Width - 2 * ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.RightTop].Arrange(new Rectangle(finalSize.Width - ResizerThickness, 0, ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Right].Arrange(new Rectangle(finalSize.Width - ResizerThickness, ResizerThickness, ResizerThickness, finalSize.Height - 2 * ResizerThickness));
			m_Resizer[(int)ResizerPos.RightBottom].Arrange(new Rectangle(finalSize.Width - ResizerThickness, finalSize.Height - ResizerThickness, ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.Bottom].Arrange(new Rectangle(ResizerThickness, finalSize.Height - ResizerThickness, finalSize.Width - 2 * ResizerThickness, ResizerThickness));
			m_Resizer[(int)ResizerPos.LeftBottom].Arrange(new Rectangle(0, finalSize.Height - ResizerThickness, ResizerThickness, ResizerThickness));

			return finalSize;
		}

		protected enum ResizerPos
		{
			Left, LeftTop, Top, RightTop, Right, RightBottom, Bottom, LeftBottom
		}
	}
}
