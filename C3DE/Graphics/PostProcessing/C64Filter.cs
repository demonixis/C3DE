using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.PostProcessing
{
    public class C64Filter : PostProcessPass
    {
        public readonly float[][] C64Palette = new float[16][]
        {
            new float[3] { 0.0f, 0.0f, 0.0f },
	        new float[3] { 62.0f, 49.0f, 162.0f },
	        new float[3] { 87.0f, 66.0f, 0.0f },
	        new float[3] { 140.0f, 62.0f, 52.0f },
	        new float[3] { 84.0f, 84.0f, 84.0f },
	        new float[3] { 141.0f, 71.0f, 179.0f },
	        new float[3] { 144.0f, 95.0f, 37.0f },
	        new float[3] { 124.0f, 112.0f, 218.0f },
	        new float[3] { 128.0f, 128.0f, 129.0f },
	        new float[3] { 104.0f, 169.0f, 65.0f },
	        new float[3] { 187.0f, 119.0f, 109.0f },
	        new float[3] { 122.0f, 191.0f, 199.0f },
	        new float[3] { 171.0f, 171.0f, 171.0f },
	        new float[3] { 208.0f, 220.0f, 113.0f },
	        new float[3] { 172.0f, 234.0f, 136.0f },
	        new float[3] { 255.0f, 255.0f, 255.0f }
        };

        private Vector3[] m_Palette;

        public C64Filter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public void SetPalette(float[][] palette)
        {
            if (palette.Length != 16)
                throw new Exception("The palette must contains 16 colors");

            m_Palette = new Vector3[16];

            for (int i = 0; i < 16; i++)
                m_Palette[i] = new Vector3(palette[i][0], palette[i][1], palette[i][2]);

            if (_effect != null)
                _effect.Parameters["Palette"].SetValue(m_Palette);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            _effect = content.Load<Effect>("Shaders/PostProcessing/C64Filter");

            if (m_Palette == null)
                SetPalette(C64Palette);
            else
                _effect.Parameters["Palette"].SetValue(m_Palette);

            _mainRenderTarget = GetRenderTarget();
        }
    }
}
