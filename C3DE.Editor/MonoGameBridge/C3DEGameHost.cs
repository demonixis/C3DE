using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Editor.Core;
using C3DE.Editor.Events;
using C3DE.Editor.Exporters;
using C3DE.Rendering;
using C3DE.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace C3DE.Editor.MonoGameBridge
{
    public sealed class C3DEGameHost : D3D11Host, IServiceProvider
    {
        public const string EditorTag = "C3DE_Editor";

        private GameTime _gameTime;
        private GameServiceContainer _services;
        private List<GameComponent> _gameComponents;
        private Renderer _renderer;
        private ContentManager _content;
        private EDScene _scene;

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
            Camera.main.ComputeProjectionMatrix(MathHelper.PiOver4, (float)width / (float)height, 1, 2000);

            _renderer.NeedsBufferUpdate = true;
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

            Application.Content = _content;
            Application.GraphicsDevice = GraphicsDevice;

            _gameComponents.Add(EDRegistry.Mouse);
            _gameComponents.Add(EDRegistry.Keys);
            _gameComponents.Add(new Time());

            Screen.Setup((int)ActualWidth, (int)ActualHeight, null, null);

            _renderer = new Renderer();
            _renderer.Initialize(_content);

            foreach (var component in _gameComponents)
                component.Initialize();

            _scene = new EDScene("Root");
            _scene.Initialize();
            _scene.RenderSettings.Skybox.Generate();
        }

        protected override void Update(Stopwatch timer)
        {
            base.Update(timer);

            _gameTime.ElapsedGameTime = timer.Elapsed;
            _gameTime.TotalGameTime += timer.Elapsed;

            foreach (var component in _gameComponents)
                component.Update(_gameTime);

            _scene.Update();
        }

        protected override void Draw(RenderTarget2D renderTarget)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
            _renderer.RenderEditor(_scene, _scene.camera, renderTarget);
        }

        #endregion

        #region New / Save and Load scene

        public void NewScene()
        {
            //_scene.Unload();
        }

        public string SaveScene()
        {
            var serialization = _scene.SerializeScene(new string[1] { "C3DE_Editor" });
            return JsonConvert.SerializeObject(serialization);
        }

        public void LoadScene(string strData)
        {
            var scene = JsonConvert.DeserializeObject<SerializedScene>(strData);
            if (scene != null)
            {
                NewScene();
                _scene.DeserializeScene(scene);
            }
        }

        #endregion

        #region Export

        public string[] ExportSceneTo(string format)
        {
            string[] result = null;

            var renderers = Scene.FindObjectsOfType<MeshRenderer>();

            if (format == "stl")
            {
                result = new string[1];
                result[0] = STLExporter.ExportMeshes(renderers);
            }
            else if (format == "obj")
            {
                result = new string[2];
                //result[0] = OBJExporter.ExportMesh(_selectedObject.SceneObject.GetComponent<MeshRenderer>());
                result[1] = string.Empty;
            }

            return result;
        }

        #endregion
    }
}
