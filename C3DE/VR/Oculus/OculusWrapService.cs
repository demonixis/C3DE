using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OculusWrap;

namespace C3DE.VR
{
    public class OculusWrapService : GameComponent, IVRDevice
    {
        private Wrap _wrap;
        private Hmd _hmd;
        private Layers _layers;
		private LayerEyeFov _layerEyeFov;

        public OculusWrapService(Game game) : base(game)
        {
            _wrap = new Wrap();

            OVRTypes.InitParams initializationParameters = new OVRTypes.InitParams();
            initializationParameters.Flags = OVRTypes.InitFlags.Debug;

            bool success = _wrap.Initialize(initializationParameters);
            if (!success)
                throw new Exception("Can't initialize Wrap");

            OVRTypes.GraphicsLuid graphicsLuid;
            _hmd = _wrap.Hmd_Create(out graphicsLuid);
            if (_hmd == null)
                throw new Exception("Can't initialize HMD");

            if (_hmd.ProductName == string.Empty)
                throw new Exception("Empty product");

            _layers = new Layers();
            _layerEyeFov = _layers.AddLayerEyeFov();
        }

		public SpriteEffects PreviewRenderEffect => SpriteEffects.None;
		public Effect DistortionCorrectionEffect => null;

		public void ApplyDistortion(RenderTarget2D renderTarget, int eye)
		{
		}

        public RenderTarget2D CreateRenderTargetForEye(int eye)
		{
			return new RenderTarget2D(Game.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);
		}

		public Matrix GetProjectionMatrix(int eye)
		{
			return Components.Camera.main.projection;
		}

		public float GetRenderTargetAspectRatio(int eye)
		{
			return 1.0f;
		}

		public Matrix GetViewMatrix(int eye, Matrix playerScale)
		{
			return Components.Camera.main.view;
		}

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
			var result = _hmd.SubmitFrame(0, _layers);
            return result == OVRTypes.Result.Success ? 0 : -1;
        }
    }
}
