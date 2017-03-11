using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Components;
using System;

namespace C3DE.VR
{
    public class NullVRService : GameComponent, IVRDevice
    {
        private Effect _effect;

        public SpriteEffects PreviewRenderEffect => SpriteEffects.None;
        public Effect DistortionCorrectionEffect => _effect;

        public NullVRService(Game game)
            : base(game)
        {
            _effect = game.Content.Load<Effect>("FX/PostProcess/OsvrDistortion");
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            return new RenderTarget2D(Game.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            return Camera.main.projection;
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            return 1.0f;
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            return Camera.main.view;
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
            return 0;
        }

        public void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
            _effect.Parameters["TargetTexture"].SetValue(renderTarget);
            _effect.Parameters["K1_Red"].SetValue(0.8f);
            _effect.Parameters["K1_Green"].SetValue(0.8f);
            _effect.Parameters["K1_Blue"].SetValue(0.8f);
            _effect.Parameters["Center"].SetValue(new Vector2(0.5f, 0.5f));
            _effect.Techniques[0].Passes[0].Apply();
        }
    }
}
