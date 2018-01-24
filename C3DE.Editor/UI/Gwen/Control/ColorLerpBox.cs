using System;
using Gwen.Input;

namespace Gwen.Control
{
	/// <summary>
	/// Linear-interpolated HSV color box.
	/// </summary>
	public class ColorLerpBox : ControlBase
	{
		private Point m_CursorPos;
		private bool m_Depressed;
		private float m_Hue;
		private Texture m_Texture;

		/// <summary>
		/// Invoked when the selected color has been changed.
		/// </summary>
		public event GwenEventHandler<EventArgs> ColorChanged;

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorLerpBox"/> class.
		/// </summary>
		/// <param name="parent">Parent control.</param>
		public ColorLerpBox(ControlBase parent) : base(parent)
		{
			SetColor(new Color(255, 255, 128, 0));
			MouseInputEnabled = true;
			m_Depressed = false;

			// texture is initialized in Render() if null
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public override void Dispose()
		{
			if (m_Texture != null)
				m_Texture.Dispose();
			base.Dispose();
		}

		/// <summary>
		/// Linear color interpolation.
		/// </summary>
		public static Color Lerp(Color toColor, Color fromColor, float amount)
		{
			Color delta = toColor.Subtract(fromColor);
			delta = delta.Multiply(amount);
			return fromColor.Add(delta);
		}

		/// <summary>
		/// Selected color.
		/// </summary>
		public Color SelectedColor
		{
			get { return GetColorAt(m_CursorPos.X, m_CursorPos.Y); }
		}

		/// <summary>
		/// Sets the selected color.
		/// </summary>
		/// <param name="value">Value to set.</param>
		/// <param name="onlyHue">Deetrmines whether to only set H value (not SV).</param>
		public void SetColor(Color value, bool onlyHue = true, bool doEvents = true)
		{
			HSV hsv = value.ToHSV();
			m_Hue = hsv.H;

			if (!onlyHue)
			{
				m_CursorPos.X = (int)(hsv.S * ActualWidth);
				m_CursorPos.Y = (int)((1 - hsv.V) * ActualHeight);
			}
			InvalidateTexture();

			if (doEvents && ColorChanged != null)
				ColorChanged.Invoke(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handler invoked on mouse moved event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="dx">X change.</param>
		/// <param name="dy">Y change.</param>
		protected override void OnMouseMoved(int x, int y, int dx, int dy)
		{
			if (m_Depressed)
			{
				m_CursorPos = CanvasPosToLocal(new Point(x, y));
				//Do we have clamp?
				if (m_CursorPos.X < 0)
					m_CursorPos.X = 0;
				if (m_CursorPos.X > ActualWidth)
					m_CursorPos.X = ActualWidth;

				if (m_CursorPos.Y < 0)
					m_CursorPos.Y = 0;
				if (m_CursorPos.Y > ActualHeight)
					m_CursorPos.Y = ActualHeight;

				if (ColorChanged != null)
					ColorChanged.Invoke(this, EventArgs.Empty);
			}
		}

		/// <summary>
		/// Handler invoked on mouse click (left) event.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <param name="down">If set to <c>true</c> mouse button is down.</param>
		protected override void OnMouseClickedLeft(int x, int y, bool down)
		{
			base.OnMouseClickedLeft(x, y, down);
			m_Depressed = down;
			if (down)
				InputHandler.MouseFocus = this;
			else
				InputHandler.MouseFocus = null;

			OnMouseMoved(x, y, 0, 0);
		}

		/// <summary>
		/// Gets the color from specified coordinates.
		/// </summary>
		/// <param name="x">X</param>
		/// <param name="y">Y</param>
		/// <returns>Color value.</returns>
		private Color GetColorAt(int x, int y)
		{
			float xPercent = (x / (float)ActualWidth);
			float yPercent = 1 - (y / (float)ActualHeight);

			Color result = Util.HSVToColor(m_Hue, xPercent, yPercent);

			return result;
		}

		/// <summary>
		/// Invalidates the control.
		/// </summary>
		private void InvalidateTexture()
		{
			if (m_Texture != null)
			{
				m_Texture.Dispose();
				m_Texture = null;
			}
		}

		/// <summary>
		/// Renders the control using specified skin.
		/// </summary>
		/// <param name="skin">Skin to use.</param>
		protected override void Render(Skin.SkinBase skin)
		{
			if (m_Texture == null)
			{
				byte[] pixelData = new byte[ActualWidth*ActualHeight*4];

				for (int x = 0; x < ActualWidth; x++)
				{
					for (int y = 0; y < ActualHeight; y++)
					{
						Color c = GetColorAt(x, y);
						pixelData[4*(x + y*ActualWidth)] = c.R;
						pixelData[4*(x + y*ActualWidth) + 1] = c.G;
						pixelData[4*(x + y*ActualWidth) + 2] = c.B;
						pixelData[4*(x + y*ActualWidth) + 3] = c.A;
					}
				}

				m_Texture = new Texture(skin.Renderer);
				m_Texture.Width = ActualWidth;
				m_Texture.Height = ActualHeight;
				m_Texture.LoadRaw(ActualWidth, ActualHeight, pixelData);
			}

			skin.Renderer.DrawColor = Color.White;
			skin.Renderer.DrawTexturedRect(m_Texture, RenderBounds);

			skin.Renderer.DrawColor = Color.Black;
			skin.Renderer.DrawLinedRect(RenderBounds);

			Color selected = SelectedColor;
			if ((selected.R + selected.G + selected.B)/3 < 170)
				skin.Renderer.DrawColor = Color.White;
			else
				skin.Renderer.DrawColor = Color.Black;

			Rectangle testRect = new Rectangle(m_CursorPos.X - 3, m_CursorPos.Y - 3, 6, 6);

			skin.Renderer.DrawShavedCornerRect(testRect);
		}

		protected override void OnBoundsChanged(Rectangle oldBounds)
		{
			if (m_Texture != null)
			{
				m_Texture.Dispose();
				m_Texture = null;
			}

			base.OnBoundsChanged(oldBounds);
		}

		protected override Size OnMeasure(Size availableSize)
		{
			m_CursorPos = new Point(0, 0);

			return new Size(128, 128);
		}

		protected override Size OnArrange(Size finalSize)
		{
			return finalSize;
		}
	}
}
