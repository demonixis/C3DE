using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.VR
{
    public class OculusRiftService : GameComponent, IVRDevice
    {
        private OculusRift _oculusRift;

        public SpriteEffects PreviewRenderEffect => SpriteEffects.None;
        public Effect DistortionCorrectionEffect => null;

        public OculusRiftService(Game game) 
            : base(game)
        {
            _oculusRift = new OculusRift();
            _oculusRift.Initialize(game.GraphicsDevice);
			game.Components.Add(this);
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            return _oculusRift.CreateRenderTargetForEye(eye);
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            return _oculusRift.GetProjectionMatrix(eye);
        }

        public Matrix GetViewMatrix(int eye, Matrix playerPose)
        {
            return _oculusRift.GetEyeViewMatrix(eye, playerPose);
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
            return _oculusRift.SubmitRenderTargets(leftRT, rightRT);
        }

		public float GetRenderTargetAspectRatio(int eye)
		{
			return _oculusRift.GetRenderTargetAspectRatio(eye);
		}

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _oculusRift.TrackHead();
        }

        public void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
        }
    }
}
