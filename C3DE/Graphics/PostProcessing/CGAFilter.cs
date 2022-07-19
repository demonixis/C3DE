using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.PostProcessing
{
    public class CGAFilter : PostProcessPass
    {
        #region Palettes

        // Palette 0 Higth Intensity
        public readonly float[][] Palette0HI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 100, 255, 100 },
            new float[3] { 255, 100, 100 },
            new float[3] { 255, 255, 100 }
        };

        // Palette 0 Low Intensity
        public readonly float[][] Palette0LI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 0, 180, 0 },
            new float[3] { 180, 0, 0 },
            new float[3] { 180, 100, 0 }
        };

        // Palette 1 Higth Intensity
        public readonly float[][] Palette1HI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 100, 255, 255 },
            new float[3] { 255, 100, 255 },
            new float[3] { 255, 255, 255 }
        };

        // Palette 1 Low Intensity
        public readonly float[][] Palette1LI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 0, 170, 170 },
            new float[3] { 170, 0, 170 },
            new float[3] { 168, 168, 168 }
        };

        // Palette 2 Higth Intensity
        public readonly float[][] Palette2HI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 100, 255, 255 },
            new float[3] { 255, 100, 100 },
            new float[3] { 255, 255, 255 }
        };

        // Palette 2 Low Intensity
        public readonly float[][] Palette2LI = new float[4][]
        {
            new float[3] { 0, 0, 0 },
            new float[3] { 0, 170, 170 },
            new float[3] { 170, 0, 0 },
            new float[3] { 170, 170, 170 }
        };

        #endregion

        private Vector3[] m_Palette;

        public CGAFilter(GraphicsDevice graphics) : base(graphics)
        {
        }

        public void SetPalette(float[][] palette)
        {
            if (palette.Length != 4)
                throw new Exception("The palette must contains four colors");

            m_Palette = new Vector3[4];

            for (int i = 0; i < 4; i++)
                m_Palette[i] = new Vector3(palette[i][0], palette[i][1], palette[i][2]);

            _effect?.Parameters["Palette"].SetValue(m_Palette);
        }

        public override void Initialize(ContentManager content)
        {
            base.Initialize(content);

            _effect = content.Load<Effect>("Shaders/PostProcessing/CGAFilter");

            if (m_Palette == null)
                SetPalette(Palette0HI);
            else
                _effect.Parameters["Palette"].SetValue(m_Palette);
        }
    }
}
