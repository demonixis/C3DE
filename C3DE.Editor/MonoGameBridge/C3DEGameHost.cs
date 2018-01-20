using C3DE.Editor.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using XNAGizmo;
using MerjTek.WpfIntegration;

namespace C3DE.Editor.MonoGameBridge
{
    public sealed class C3DEGameHost : MonoGameControl
    {
        public const string EditorTag = "C3DE_Editor";

        private GameTime _gameTime;
        private EDScene _scene;
        internal GizmoComponent gizmoComponent;

        public Action EngineReady = null;

        public C3DEGameHost()
            : base()
        {
            ControlLoaded += C3DEGameHost_ControlLoaded;
        }

        private void C3DEGameHost_ControlLoaded(object sender, GraphicsDeviceEventArgs e)
        {
            Initialize(e.GraphicsDevice);
            Render += C3DEGameHost_Render;
        }

        private void C3DEGameHost_Render(object sender, GraphicsDeviceEventArgs e)
        {
            Draw(e.GraphicsDevice);
        }

        #region Life cycle

        private void Initialize(GraphicsDevice graphicsDevice)
        {
            _gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);

            AssetImporter.CreateFolderStructure("Temp");

            var engine = GraphicsDeviceService.Instance.Engine;
            engine.Services.RemoveService(typeof(IGraphicsDeviceService));

            Application.Engine = engine;
            Application.Engine.Services.AddService<IGraphicsDeviceService>(GraphicsDeviceService.Instance);
            Application.Engine.InitializeEditor(GraphicsDevice);

            gizmoComponent = new GizmoComponent(GraphicsDevice);
            gizmoComponent.ActiveMode = GizmoMode.Translate;

            _scene = new EDScene("Root", gizmoComponent);
            _scene.Initialize();
            _scene.RenderSettings.Skybox.Generate();

            Application.SceneManager.Add(_scene, true);

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

        private void Update()
        {
            Application.Engine.UpdateEditor();
            gizmoComponent.Update();
        }

        private void Draw(GraphicsDevice graphicsDevice)
        {
            Update();

            graphicsDevice.Clear(Color.CornflowerBlue);
          
            Application.Engine.DrawEditor();
            gizmoComponent.Draw();

            if (Screen.LockCursor && EDSettings.current.AllowLockCursor)
                EDRegistry.Mouse.SetPosition(Screen.WidthPerTwo, Screen.HeightPerTwo);

            Application.Engine.EndDrawEditor();
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

        public Texture2D LoadTempTexture(string assetName) => null;
        public Model LoadTempModel(string assetName) => null;
        public SpriteFont LoadTempFont(string assetName) => null;
        public Effect LoadTempEffect(string assetName) => null;
        public Model AddModelFromTemp(string assetName) => null;

        #endregion

        #region Export

        public string[] ExportSceneTo(string format)
        {
            return null;
        }

        #endregion
    }
}
