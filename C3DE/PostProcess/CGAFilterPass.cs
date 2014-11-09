using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.PostProcess
{
    public class CGAFilterPass : PostProcessPass
    {
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

        private Effect _cgafilterEffect;
        private Vector3[] _palette;

        public void SetPalette(float[][] palette)
        {
            if (palette.Length != 4)
                throw new Exception("The palette must contains four colors");

            _palette = new Vector3[4];

            for (int i = 0; i < 4; i++)
                _palette[i] = new Vector3(palette[i][0], palette[i][1], palette[i][2]);

            if (_cgafilterEffect != null)
                _cgafilterEffect.Parameters["Palette"].SetValue(_palette);
        }

        public override void Initialize(ContentManager content)
        {
            _cgafilterEffect = content.Load<Effect>("FX/PostProcess/CGAFilter");

            if (_palette == null)
                SetPalette(Palette0HI);
            else
                _cgafilterEffect.Parameters["Palette"].SetValue(_palette);
        }

        public override void Apply(SpriteBatch spriteBatch, RenderTarget2D renderTarget)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, _cgafilterEffect);
            spriteBatch.Draw(renderTarget, Vector2.Zero, Color.White);
            _cgafilterEffect.Parameters["TargetTexture"].SetValue(renderTarget);
            _cgafilterEffect.CurrentTechnique.Passes[0].Apply();
            spriteBatch.End();
        }
    }
}
