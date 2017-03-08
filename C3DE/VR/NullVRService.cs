using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using C3DE.Components;

namespace C3DE.VR
{
    public class NullVRService : GameComponent, IVRDevice
    {
        public NullVRService(Game game) 
            : base(game)
        {
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
    }
}
