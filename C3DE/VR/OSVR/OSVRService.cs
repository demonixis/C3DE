using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OSVR.ClientKit;
using OSVR.MonoGame;
using OSVR.RenderManager;

namespace C3DE.VR
{
	public class OSVRService : VRService
	{
		private RenderManagerOpenGL _renderManager;
		private ClientContext _context;
		private GraphicsLibraryOpenGL _graphicsLibrary;
		private ViewportDescription[] _normalizedCroppingViewports;
		private RenderBufferOpenGL[] _buffers;
		private RenderInfoOpenGL[] _renderInfo;
		private RenderParams _renderParams;
		private int _width;
		private int _height;
		private Vector3[] _distortionParams;
		private Vector2 _distortionCenter;
		private bool _useRenderManager;
		private VRHead _head;

		public OSVRService(Game game, string appIdentifier = "net.demonixis.c3de")
			: base(game)
		{
#if !DESKTOP
			throw new NotSupportedException("[OSVR] This plugin is not supported on this platform");
#endif
			ClientContext.PreloadNativeLibraries();

			_context = new ClientContext(appIdentifier);
			_head = new VRHead(null, _context, new XnaPoseInterface(_context.GetPoseInterface("/me/head")));
			_width = Screen.Width / 2;
			_height = Screen.Height;
			_distortionCenter = new Vector2(0.5f, 0.5f);
			_useRenderManager = !string.IsNullOrEmpty(_context.getStringParameter("/renderManagerConfig"));
		}

		public override int TryInitialize()
		{
			DistortionEffect = Game.Content.Load<Effect>("FX/PostProcess/OsvrDistortion");

			_useRenderManager = false;

			if (_useRenderManager)
				SetupRenderManager();

			_context.update();

			var displayConfig = _context.GetDisplayConfig();
			var numViewers = displayConfig.GetNumViewers();

			if (numViewers != 1)
				return -1;

			for (uint viewer = 0; viewer < numViewers; viewer++)
			{
				var numEyes = displayConfig.GetNumEyesForViewer(viewer);
				if (numEyes != 2)
					return -2;

				_distortionParams = new Vector3[2];

				for (byte eye = 0; eye < numEyes; eye++)
				{
					var numSurfaces = displayConfig.GetNumSurfacesForViewerEye(viewer, eye);
					if (numSurfaces != 1)
						return -3;

					for (uint surface = 0; surface < numSurfaces; surface++)
					{
						DistortionCorrectionRequired = displayConfig.DoesViewerEyeSurfaceWantDistortion(viewer, eye, surface);
						if (DistortionCorrectionRequired)
						{
							var radialDistortionPriority = displayConfig.GetViewerEyeSurfaceRadialDistortionPriority(viewer, eye, surface);
							if (radialDistortionPriority >= 0)
							{
								var dist = displayConfig.GetViewerEyeSurfaceRadialDistortion(viewer, eye, surface);
								_distortionParams[eye].X = (float)dist.k1.x;
								_distortionParams[eye].Y = (float)dist.k1.y;
								_distortionParams[eye].Z = (float)dist.k1.z;
							}
						}
					}
				}
			}

			Game.Components.Add(this);

			return 0;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (_renderManager != null)
				_renderManager.Dispose();
			
			if (_context != null)
				_context.Dispose();
		}

        private void SetupRenderManager()
        {
            _graphicsLibrary = new GraphicsLibraryOpenGL();
            _graphicsLibrary.Toolkit = new MonoGameGraphicsToolkit();
            _renderManager = new RenderManagerOpenGL(_context, "OpenGL", _graphicsLibrary);

            while (!_renderManager.DoingOkay)
                _context.update();

            _renderManager.OpenDisplay();
            _renderInfo = new RenderInfoOpenGL[2];
            _renderParams = new RenderParams();
            _renderManager.GetRenderInfo(_renderParams, ref _renderInfo);

            for (var i = 0; i < _renderInfo.Length; i++)
            {
				_width += (int)_renderInfo[i].Viewport.Width;
				if (_height != 0 && _height != (int)_renderInfo[i].Viewport.Height)
                    throw new InvalidOperationException("All RT must have the same height.");

				_height = (int)_renderInfo[i].Viewport.Height;
            }

            _normalizedCroppingViewports = new ViewportDescription[_renderInfo.Length];

            for (var i = 0; i < _normalizedCroppingViewports.Length; i++)
            {
                _normalizedCroppingViewports[i] = new ViewportDescription
                {
                    Height = 1.0,
					Width = _renderInfo[i].Viewport.Width / _width,
					Left = (i * _renderInfo[i].Viewport.Width) / _width,
                    Lower = 0
                };
            }

            _buffers = new RenderBufferOpenGL[2];

            for (var i = 0; i < _buffers.Length; i++)
                _buffers[i] = new RenderBufferOpenGL();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _context.update();
            _head.Update();
        }

        public override RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)_width, (int)_height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);

#if DESKTOP
            if (_buffers == null)
                _buffers = new RenderBufferOpenGL[2];

            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var glTexture = (int)info.GetValue(renderTarget);
            _buffers[eye].ColorBufferName = (uint)glTexture;
#endif
            return renderTarget;
        }

        public override Matrix GetProjectionMatrix(int eye)
        {
            return eye == 0 ? _head.LeftEye.Projection : _head.RightEye.Projection;
        }

        public override Matrix GetViewMatrix(int eye, Matrix playerPose)
        {
            return eye == 0 ? _head.LeftEye.Transform : _head.RightEye.Transform;
        }

		public override float GetRenderTargetAspectRatio(int eye) => 1.0f;

        public override int SubmitRenderTargets(RenderTarget2D renderTargetLeft, RenderTarget2D renderTargetRight)
        {
			if (_useRenderManager)
            	_renderManager.Present(_buffers, _renderInfo, _normalizedCroppingViewports, _renderParams, false);
            
			return 0;
        }

		public override void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
			if (!DistortionCorrectionRequired)
                return;

			DistortionEffect.Parameters["TargetTexture"].SetValue(renderTarget);
			DistortionEffect.Parameters["K1_Red"].SetValue(_distortionParams[eye].X);
            DistortionEffect.Parameters["K1_Green"].SetValue(_distortionParams[eye].Y);
            DistortionEffect.Parameters["K1_Blue"].SetValue(_distortionParams[eye].Z);
			DistortionEffect.Parameters["Center"].SetValue(_distortionCenter);
            DistortionEffect.Techniques[0].Passes[0].Apply();
        }
    }
}
