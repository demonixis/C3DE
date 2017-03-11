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
            LayerEyeFov layerEyeFov = _layers.AddLayerEyeFov();
        }

        public SpriteEffects PreviewRenderEffect => throw new NotImplementedException();

        public Effect DistortionCorrectionEffect => throw new NotImplementedException();

        public void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
            throw new NotImplementedException();
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            throw new NotImplementedException();
        }

        public Matrix GetProjectionMatrix(int eye)
        {
            throw new NotImplementedException();
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            throw new NotImplementedException();
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            throw new NotImplementedException();
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
           var result = _hmd.SubmitFrame(0, _layers);

            return result == OVRTypes.Result.Success ? 0 : -1;
        }
    }
}
