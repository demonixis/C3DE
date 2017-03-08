using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using OSVR.ClientKit;
using OSVR.RenderManager;

namespace C3DE.VR
{
    public class MonoGameGraphicsToolkit : OpenGLToolkitFunctions
    {
        public MonoGameGraphicsToolkit()
        {
        }

        protected override bool AddOpenGLContext(ref OpenGLContextParams p)
        {
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
        private ClientContext _context;
        private RenderManagerOpenGL _renderManager;
        private GraphicsLibraryOpenGL _graphicsLibrary;
        private ViewportDescription[] _normalizedCroppingViewports;
        private RenderBufferOpenGL[] _buffers;
        private RenderInfoOpenGL[] _renderInfo;
        private RenderParams _renderParams;

        public OSVRService(Game game, string appIdentifier = "net.demonixis.c3de") 
            : base(game)
        {
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
            _renderManager.Present(_buffers, _renderInfo, _normalizedCroppingViewports, _renderParams, false);
            return 0;
        }
    }
}
