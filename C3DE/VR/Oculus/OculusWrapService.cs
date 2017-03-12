using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OculusWrap;

namespace C3DE.VR
{
    public class OculusWrapService : VRService
    {
        private Wrap _wrap;
        private Hmd _hmd;
        private Layers _layers;
		private LayerEyeFov _layerEyeFov;

        public OculusWrapService(Game game) 
			: base(game)
        {
            _wrap = new Wrap();

			OVRTypes.InitParams initializationParameters = new OVRTypes.InitParams();
			initializationParameters.Flags = OVRTypes.InitFlags.Debug;

			if (!_wrap.Initialize(initializationParameters))
				throw new Exception("Can't initialize Wrap");
        }

		public override int TryInitialize()
		{
			OVRTypes.GraphicsLuid graphicsLuid;
			_hmd = _wrap.Hmd_Create(out graphicsLuid);
			if (_hmd == null)
				return -1;

			if (_hmd.ProductName == string.Empty)
				return -2;

			_layers = new Layers();
			_layerEyeFov = _layers.AddLayerEyeFov();

			Game.Components.Add(this);

			return 0;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_wrap != null)
				_wrap.Dispose();
		}

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
		{
			return new RenderTarget2D(Game.GraphicsDevice, Screen.Width / 2, Screen.Height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);
		}

		public override Matrix GetProjectionMatrix(int eye)
		{
			return Components.Camera.main.projection;
		}

		public override Matrix GetViewMatrix(int eye, Matrix playerPose)
		{
			return Components.Camera.main.view;
		}

		public override float GetRenderTargetAspectRatio(int eye) => 1.0f;

        public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
			var result = _hmd.SubmitFrame(0, _layers);
            return result == OVRTypes.Result.Success ? 0 : -1;
        }
    }
}
