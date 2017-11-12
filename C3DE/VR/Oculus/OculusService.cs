using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OculusRiftSample;

namespace C3DE.VR
{
    public class OculusRiftService : VRService
    {
        private OculusRift _oculusRift;

        public OculusRiftService(Game game)
            : base(game)
        {
            _oculusRift = new OculusRift();
        }

        public override int TryInitialize()
        {
            if (_oculusRift.Initialize(Game.GraphicsDevice) != 0)
                return -1;

            Game.Components.Add(this);

            return 0;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (_oculusRift != null)
            {
                _oculusRift.Shutdown();
                _oculusRift = null;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _oculusRift.TrackHead();
        }

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            return _oculusRift.CreateRenderTargetForEye(eye);
        }

        public override Matrix GetProjectionMatrix(int eye)
        {
            return _oculusRift.GetProjectionMatrix(eye);
        }

        public override Matrix GetViewMatrix(int eye, Matrix playerPose)
        {
            return _oculusRift.GetEyeViewMatrix(eye, playerPose);
        }

        public override float GetRenderTargetAspectRatio(int eye)
        {
            return _oculusRift.GetRenderTargetAspectRatio(eye);
        }

        public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
            return _oculusRift.SubmitRenderTargets(renderTargetLeft, renderTargetRight);
        }
    }
}