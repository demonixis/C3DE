using System;

namespace Gwen.Control
{
    /// <summary>
    /// Image container.
    /// </summary>
    public class ImagePanel : ControlBase
    {
        private readonly Texture m_Texture;
        private readonly float[] m_uv;
        private Color m_DrawColor;
		private Size m_ImageSize;

		/// <summary>
		/// Texture name.
		/// </summary>
		public string ImageName
		{
			get { return m_Texture.Name; }
			set { m_Texture.Load(value); }
		}

		/// <summary>
		/// Gets or sets the size of the image.
		/// </summary>
		public Size ImageSize
		{
			get { return m_ImageSize; }
			set { if (value == m_ImageSize) return; m_ImageSize = value; Invalidate(); }
		}

		/// <summary>
		/// Gets or sets the texture coordinates of the image in pixels.
		/// </summary>
		public Rectangle TextureRect
		{
			get
			{
				if (m_Texture == null)
					return Rectangle.Empty;
				
				int x1 = (int)(m_uv[0] * m_Texture.Width);
				int y1 = (int)(m_uv[1] * m_Texture.Height);
				int x2 = Util.Ceil(m_uv[2] * m_Texture.Width);
				int y2 = Util.Ceil(m_uv[3] * m_Texture.Height);
				return new Rectangle(x1, y1, x2 - x1, y2 - y1);
			}
			set
			{
				if (m_Texture == null)
					return;
				
				m_uv[0] = (float)value.X / (float)m_Texture.Width;
				m_uv[1] = (float)value.Y / (float)m_Texture.Height;
				m_uv[2] = m_uv[0] + (float)value.Width / (float)m_Texture.Width;
				m_uv[3] = m_uv[1] + (float)value.Height / (float)m_Texture.Height;
			}
		}

		/// <summary>
		/// Gets or sets the color of the image.
		/// </summary>
		public Color ImageColor
		{
			get { return m_DrawColor; }
			set { m_DrawColor = value; }
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="ImagePanel"/> class.
        /// </summary>
        /// <param name="parent">Parent control.</param>
        public ImagePanel(ControlBase parent)
            : base(parent)
        {
            m_uv = new float[4];
            m_Texture = new Texture(Skin.Renderer);
			m_ImageSize = Size.Zero;
            SetUV(0, 0, 1, 1);
            MouseInputEnabled = true;
            m_DrawColor = Color.White;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            m_Texture.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Sets the texture coordinates of the image in uv-coordinates.
        /// </summary>
        public virtual void SetUV(float u1, float v1, float u2, float v2)
        {
            m_uv[0] = u1;
            m_uv[1] = v1;
            m_uv[2] = u2;
            m_uv[3] = v2;
        }
			
        /// <summary>
        /// Renders the control using specified skin.
        /// </summary>
        /// <param name="skin">Skin to use.</param>
        protected override void Render(Skin.SkinBase skin)
        {
            base.Render(skin);
            skin.Renderer.DrawColor = m_DrawColor;
            skin.Renderer.DrawTexturedRect(m_Texture, RenderBounds, m_uv[0], m_uv[1], m_uv[2], m_uv[3]);
        }

        /// <summary>
        /// Control has been clicked - invoked by input system. Windows use it to propagate activation.
        /// </summary>
        public override void Touch()
        {
            base.Touch();
        }

		protected override Size OnMeasure(Size availableSize)
		{
			if (m_Texture == null)
				return Size.Zero;
			
			float scale = this.Scale;

			Size size = m_ImageSize;
			if (size.Width == 0) size.Width = m_Texture.Width;
			if (size.Height == 0) size.Height = m_Texture.Height;
			
			return new Size(Util.Ceil(size.Width * scale), Util.Ceil(size.Height * scale));
		}

		protected override Size OnArrange(Size finalSize)
		{
			return finalSize;
		}

		/// <summary>
		/// Handler for Space keyboard event.
		/// </summary>
		/// <param name="down">Indicates whether the key was pressed or released.</param>
		/// <returns>
		/// True if handled.
		/// </returns>
		protected override bool OnKeySpace(bool down)
        {
            if (down)
                base.OnMouseClickedLeft(0, 0, true);
            return true;
        }
    }
}
