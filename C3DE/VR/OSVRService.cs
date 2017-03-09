using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using OSVR.ClientKit;
using OSVR.RenderManager;
using C3DE.Components;

namespace C3DE.VR
{
    public class MonoGameGraphicsToolkit : OpenGLToolkitFunctions
    {
        public MonoGameGraphicsToolkit()
        {
        }

        protected override bool AddOpenGLContext(ref OpenGLContextParams p)
        {
            var window = Application.Engine.Window;
            window.Position = new Point(p.XPos, p.YPos);
            window.Title = p.WindowTitle;
            Screen.Setup(p.Width, p.Height, null, null);
            return true;
        }

        protected override bool MakeCurrent(UIntPtr display)
        {
            return true;
        }

        protected override bool SetVerticalSync(bool verticalSync)
        {
            return true;
        }

        protected override bool SwapBuffers(UIntPtr display)
        {
            return true;
        }
    }

    public class OSVRService : GameComponent, IVRDevice
    {
        private RenderManagerOpenGL _renderManager;
        private ClientContext _context;
        private GraphicsLibraryOpenGL _graphicsLibrary;
        private ViewportDescription[] _normalizedCroppingViewports;
        private RenderBufferOpenGL[] _buffers;
        private RenderInfoOpenGL[] _renderInfo;
        private RenderParams _renderParams;
        private int width;
        private int height;
        private Matrix projectionMatrix;
        private Matrix viewMatrix;

        SpriteEffects IVRDevice.PreviewRenderEffect => SpriteEffects.None;

        public OSVRService(Game game, string appIdentifier = "net.demonixis.c3de")
            : base(game)
        {
            ClientContext.PreloadNativeLibraries();

            _context = new ClientContext(appIdentifier);

            _graphicsLibrary = new GraphicsLibraryOpenGL();
            _graphicsLibrary.Toolkit = new MonoGameGraphicsToolkit();
            _renderManager = new RenderManagerOpenGL(_context, "OpenGL", _graphicsLibrary);

            while (!_renderManager.DoingOkay)
                _context.update();

            _renderManager.OpenDisplay();
            _renderInfo = new RenderInfoOpenGL[2];
            _renderParams = new RenderParams();
            _renderManager.GetRenderInfo(_renderParams, ref _renderInfo);

            double w = 0;
            double h = 0;
            for (var i = 0; i < _renderInfo.Length; i++)
            {
                w += _renderInfo[i].Viewport.Width;
                if (height != 0 && height != _renderInfo[i].Viewport.Height)
                    throw new InvalidOperationException("All RT must have the same height.");

                h = _renderInfo[i].Viewport.Height;
            }

            width = (int)w;
            height = (int)h;

            _normalizedCroppingViewports = new ViewportDescription[_renderInfo.Length];

            for (var i = 0; i < _normalizedCroppingViewports.Length; i++)
            {
                _normalizedCroppingViewports[i] = new ViewportDescription
                {
                    Height = 1.0,
                    Width = _renderInfo[i].Viewport.Width / w,
                    Left = (i * _renderInfo[i].Viewport.Width) / w,
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

            var displayConfig = _context.GetDisplayConfig();

            if (displayConfig == null || !displayConfig.CheckDisplayStartup())
                return;

            var numViewers = displayConfig.GetNumViewers();
            for (uint viewer = 0; viewer < numViewers; viewer++)
            {
                var numEyes = displayConfig.GetNumEyesForViewer(viewer);
                var viewerPose = displayConfig.GetViewerPose(viewer);

                for (byte eye = 0; eye < numEyes; eye++)
                {
                    var numSurfaces = displayConfig.GetNumSurfacesForViewerEye(viewer, eye);
                    var viewerEyePose = displayConfig.GetViewerEyePose(viewer, eye);
                    var viewerEyeMatrixf = displayConfig.GetViewerEyeViewMatrixf(viewer, eye, MatrixConventionsFlags.Default);

                    for (uint surface = 0; surface < numSurfaces; surface++)
                    {
                        var viewport = displayConfig.GetRelativeViewportForViewerEyeSurface(viewer, eye, surface);
                        var wantsDistortion = displayConfig.DoesViewerEyeSurfaceWantDistortion(viewer, eye, surface);

                        if (wantsDistortion)
                        {
                            var radialDistortionPriority = displayConfig.GetViewerEyeSurfaceRadialDistortionPriority(viewer, eye, surface);

                            if (radialDistortionPriority >= 0)
                            {
                                var distortionParameters = displayConfig.GetViewerEyeSurfaceRadialDistortion(viewer, eye, surface);
                            }
                        }

                        var projectionf = displayConfig.GetProjectionMatrixForViewerEyeSurfacef(viewer, eye, surface, 1.0f, 1000.0f, MatrixConventionsFlags.Default);
                        var projectionClippingPlanes = displayConfig.GetViewerEyeSurfaceProjectionClippingPlanes(viewer, eye, surface);
                        var displayInputIndex = displayConfig.GetViewerEyeSurfaceDisplayInputIndex(viewer, eye, surface);
                    }
                }
            }
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
        {
            var renderTarget = new RenderTarget2D(Game.GraphicsDevice, (int)width, (int)height, false, SurfaceFormat.ColorSRgb, DepthFormat.Depth24Stencil8);
#if DESKTOP
            var info = typeof(RenderTarget2D).GetField("glTexture", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var glTexture = (int)info.GetValue(renderTarget);
            _buffers[eye].ColorBufferName = (uint)glTexture;
#endif
            return renderTarget;
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
            _renderManager.Present(_buffers, _renderInfo, _normalizedCroppingViewports, _renderParams, false);
            return 0;
        }
    }
}
