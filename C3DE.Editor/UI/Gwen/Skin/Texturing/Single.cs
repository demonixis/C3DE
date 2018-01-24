using System;

namespace Gwen.Skin.Texturing
{
    /// <summary>
    /// Single textured element.
    /// </summary>
    public struct Single
    {
        private readonly Texture m_Texture;
        private readonly float[] m_uv;
        private readonly int m_Width;
        private readonly int m_Height;

        public Single(Texture texture, float x, float y, float w, float h )
        {
            m_Texture = texture;

            float texw = m_Texture.Width;
            float texh = m_Texture.Height;

            m_uv = new float[4];
            m_uv[0] = x / texw;
            m_uv[1] = y / texh;
            m_uv[2] = (x + w) / texw;
            m_uv[3] = (y + h) / texh;

            m_Width = (int) w;
            m_Height = (int) h;
        }

        // can't have this as default param
        public void Draw(Renderer.RendererBase render, Rectangle r)
        {
            Draw(render, r, Color.White);
        }

        public void Draw(Renderer.RendererBase render, Rectangle r, Color col)
        {
            if (m_Texture == null)
                return;

            render.DrawColor = col;
            render.DrawTexturedRect(m_Texture, r, m_uv[0], m_uv[1], m_uv[2], m_uv[3]);
        }
    }
}
