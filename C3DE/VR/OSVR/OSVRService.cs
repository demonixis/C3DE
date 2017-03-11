using Microsoft.Xna.Framework;
using System;
using Microsoft.Xna.Framework.Graphics;
using OSVR.ClientKit;
using OSVR.RenderManager;
using OSVR.MonoGame;

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
            //window.Position = new Point(p.XPos, p.YPos);
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
        private Effect _effect;
        private DisplayConfig _displayConfig;
        private RenderManagerOpenGL _renderManager;
        private ClientContext _context;
        private GraphicsLibraryOpenGL _graphicsLibrary;
        private ViewportDescription[] _normalizedCroppingViewports;
        private RenderBufferOpenGL[] _buffers;
        private RenderInfoOpenGL[] _renderInfo;
        private RenderParams _renderParams;
        private int _width;
        private int _height;
        private bool _useRenderManager;
        private VRHead _head;

        public SpriteEffects PreviewRenderEffect => SpriteEffects.None;
        public Effect DistortionCorrectionEffect => _effect;
        public bool ShowDistorition { get; set; } = true;

        public OSVRService(Game game, string appIdentifier = "net.demonixis.c3de")
            : base(game)
        {
            ClientContext.PreloadNativeLibraries();

            _context = new ClientContext(appIdentifier);
            _useRenderManager = !string.IsNullOrEmpty(_context.getStringParameter("/renderManagerConfig"));

            _head = new VRHead(null, _context, new XnaPoseInterface(_context.GetPoseInterface("/me/head")));
            _width = Screen.Width / 2;
            _height = Screen.Height;

            _effect = game.Content.Load<Effect>("FX/PostProcess/OsvrDistortion");

            //if (_useRenderManager)
            //SetupRenderManager();

            game.Components.Add(this);
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

            double w = 0;
            double h = 0;
            for (var i = 0; i < _renderInfo.Length; i++)
            {
                w += _renderInfo[i].Viewport.Width;
                if (_height != 0 && _height != _renderInfo[i].Viewport.Height)
                    throw new InvalidOperationException("All RT must have the same height.");

                h = _renderInfo[i].Viewport.Height;
            }

            _width = (int)w;
            _height = (int)h;

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
            _head.Update();
        }

        public RenderTarget2D CreateRenderTargetForEye(int eye)
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

        public Matrix GetProjectionMatrix(int eye)
        {
            return eye == 0 ? _head.LeftEye.Projection : _head.RightEye.Projection;
        }

        public float GetRenderTargetAspectRatio(int eye)
        {
            return 1.0f;
        }

        public Matrix GetViewMatrix(int eye, Matrix playerScale)
        {
            return eye == 0 ? _head.LeftEye.Transform : _head.RightEye.Transform;
        }

        public int SubmitRenderTargets(RenderTarget2D leftRT, RenderTarget2D rightRT)
        {
            // _renderManager.Present(_buffers, _renderInfo, _normalizedCroppingViewports, _renderParams, false);
            return 0;
        }

        public void ApplyDistortion(RenderTarget2D renderTarget, int eye)
        {
            if (!ShowDistorition)
                return;

            _effect.Parameters["TargetTexture"].SetValue(renderTarget);
            _effect.Parameters["K1_Red"].SetValue(0.5f);
            _effect.Parameters["K1_Green"].SetValue(0.5f);
            _effect.Parameters["K1_Blue"].SetValue(0.5f);
            _effect.Parameters["Center"].SetValue(new Vector2(0.5f, 0.5f));
            _effect.Techniques[0].Passes[0].Apply();
        }
    }
}
