using C3DE.Components;
using C3DE.Editor.Core;
using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using XNAGizmo;

namespace C3DE.Editor.MonoGameBridge
{
    public sealed class C3DEGameHost : D3D11Host, IServiceProvider
    {
        public const string EditorTag = "C3DE_Editor";

        private GameTime _gameTime;
        private GameServiceContainer _services;
        private List<GameComponent> _gameComponents;
        private ForwardRenderer _renderer;
        private ContentManager _content;
        private ContentManager _projectContent;
        private EDScene _scene;
        internal GizmoComponent gizmoComponent;

        public Action EngineReady = null;

        #region GameHost implementation

        public object GetService(Type serviceType)
        {
            return _services.GetService(serviceType);
        }

        protected override void OnRenderSizeChanged(System.Windows.SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            int width = (int)sizeInfo.NewSize.Width;
            int height = (int)sizeInfo.NewSize.Height;

            Screen.Setup(width, height, null, null);
            Camera.Main.ComputeProjectionMatrix(MathHelper.PiOver4, (float)width / (float)height, 1, 2000);

            _renderer.Dirty = true;
        }

        #endregion

        #region Life cycle

        protected override void Initialize()
        {
            base.Initialize();

            _gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
            _gameComponents = new List<GameComponent>();

            _services = new GameServiceContainer();
            _services.AddService<IGraphicsDeviceService>(new GraphicsDeviceService(graphicsDevice));

            _content = new ContentManager(this);
            _content.RootDirectory = "Content";

            _projectContent = new ContentManager(this);
            _projectContent.RootDirectory = EDRegistry.ContentTempPath;

            AssetImporter.CreateFolderStructure("Temp");

            Application.Content = _content;
            Application.GraphicsDevice = GraphicsDevice;
            Application.SceneManager = new SceneManager();

            _gameComponents.Add(EDRegistry.Mouse);
            _gameComponents.Add(EDRegistry.Keys);
            _gameComponents.Add(new Time());

            _renderer = new ForwardRenderer(Application.GraphicsDevice);
            _renderer.Initialize(_content);

            foreach (var component in _gameComponents)
                component.Initialize();

            gizmoComponent = new GizmoComponent(GraphicsDevice);
            gizmoComponent.ActiveMode = GizmoMode.Translate;

            _scene = new EDScene("Root", gizmoComponent);
            _scene.Initialize();
            _scene.RenderSettings.Skybox.Generate();

            Application.SceneManager.Add(_scene, true);
            Application.SceneManager.Update();

            MouseDown += C3DEGameHost_MouseDown;
            MouseUp += C3DEGameHost_MouseUp;

            EngineReady?.Invoke();
        }

        void C3DEGameHost_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            Screen.LockCursor = false;
            Cursor = System.Windows.Input.Cursors.Arrow;
        }

        void C3DEGameHost_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                return;

            Focus();
            CaptureMouse();
            Screen.LockCursor = true;
            Cursor = System.Windows.Input.Cursors.None;
        }

        protected override void Update(Stopwatch timer)
        {
            base.Update(timer);

            _gameTime.ElapsedGameTime = timer.Elapsed;
            _gameTime.TotalGameTime += timer.Elapsed;

            foreach (var component in _gameComponents)
                component.Update(_gameTime);

            _scene.Update();

            gizmoComponent.Update();
        }

        protected override void Draw(RenderTarget2D renderTarget)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
            _renderer.RenderEditor(_scene, _scene.camera, renderTarget);
            gizmoComponent.Draw();

            if (Screen.LockCursor && EDSettings.current.AllowLockCursor)
                EDRegistry.Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);
        }

        #endregion

        #region New / Save and Load scene

        public void NewScene()
        {
            _scene.Unload();
            gizmoComponent.Selection.Clear();
            _scene = new EDScene("Root", gizmoComponent);
            _scene.Initialize();
            _scene.RenderSettings.Skybox.Generate();
        }

        public bool SaveScene(string path)
        {
            var result = true;

            try
            {
                var serScene = new SerializedScene()
                {
                    Materials = _scene.Materials.ToArray(),
                    GameObjects = _scene.GetUsedSceneObjects(),
                    RenderSettings = _scene.RenderSettings
                };

                Serializer.Serialize(path, serScene);
            }
            catch (Exception ex)
            {
                result = false;
                Debug.Log(ex.Message);
            }

            return result;
        }

        public bool LoadScene(string path)
        {
            var result = true;

            try
            {
                var data = Serializer.Deserialize(path, typeof(SerializedScene));
                var serializedScene = data as SerializedScene;
                if (serializedScene != null)
                {
                    NewScene();

                    foreach (var so in serializedScene.GameObjects)
                    {
                        so.PostDeserialize();
                        _scene.Add(so);
                    }

                    _scene.RenderSettings.Set(serializedScene.RenderSettings);
                }
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                result = false;
            }

            return result;
        }

        #endregion

        #region Live import

        public Texture2D LoadTempTexture(string assetName)
        {
            return _projectContent.Load<Texture2D>(assetName);
        }

        public Model LoadTempModel(string assetName)
        {
            return _projectContent.Load<Model>(assetName);
        }

        public SpriteFont LoadTempFont(string assetName)
        {
            return _projectContent.Load<SpriteFont>(assetName);
        }

        public Effect LoadTempEffect(string assetName)
        {
            return _projectContent.Load<Effect>(assetName);
        }

        public Model AddModelFromTemp(string assetName)
        {
            return null;
        }

        #endregion

        #region Export

        public string[] ExportSceneTo(string format)
        {
            return null;
        }

        #endregion
    }
}
