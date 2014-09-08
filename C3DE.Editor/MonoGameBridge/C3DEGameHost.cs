using C3DE.Components.Lights;
using C3DE.Editor.Components;
using C3DE.Inputs;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;

namespace C3DE.Editor.MonoGameBridge
{
    using C3DE.Components;
    using C3DE.Components.Renderers;
    using C3DE.Prefabs.Meshes;
    using XnaColor = Microsoft.Xna.Framework.Color;
    using XnaVector2 = Microsoft.Xna.Framework.Vector2;
    using XnaVector3 = Microsoft.Xna.Framework.Vector3;

    public class SceneObjectAddedEventArgs : EventArgs
    {
        public SceneObject SceneObject { get; private set; }

        public SceneObjectAddedEventArgs(SceneObject sceneObject)
        {
            SceneObject = sceneObject;
        }
    }

    public sealed class C3DEGameHost : D3D11Host, IServiceProvider
    {
        private GameTime _gameTime;
        private GameServiceContainer _services;
        private List<GameComponent> _gameComponents;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private ContentManager _content;
        private Scene _scene;
        private Camera _mainCamera;
        private List<string> _sceneObjectToAdd;
        private SceneObject _selectedObject;

        public event EventHandler<SceneObjectAddedEventArgs> SceneObjectAdded = null;
        public event EventHandler<EventArgs> SceneObjectRemoved = null;

        protected override void Initialize()
        {
            base.Initialize();

            _sceneObjectToAdd = new List<string>();

            _gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
            _gameComponents = new List<GameComponent>();

            _services = new GameServiceContainer();
            _services.AddService<IGraphicsDeviceService>(new GraphicsDeviceService(graphicsDevice));

            _content = new ContentManager(this);
            _content.RootDirectory = "Content";
            _scene = new Scene(_content);

            Application.Content = _content;
            Application.GraphicsDevice = GraphicsDevice;

            Input.Mouse = new EditorMouseComponent(null, this);

            _gameComponents.Add(new Time(null));
            _gameComponents.Add(Input.Mouse);

            Screen.Setup((int)ActualWidth, (int)ActualHeight, null, null);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderer = new Renderer(GraphicsDevice);
            _renderer.LoadContent(_content);
            _scene.Initialize();

            foreach (var component in _gameComponents)
                component.Initialize();

            PopulateSceneWithThings();
        }

        private void PopulateSceneWithThings()
        {
            var camera = new CameraPrefab("Editor_MainCamera");
            _scene.Add(camera);
            camera.AddComponent<EditorOrbitController>();
            _mainCamera = camera.Camera;

            var lightPrefab = new LightPrefab("Editor_MainLight", LightType.Directional);
            _scene.Add(lightPrefab);
            lightPrefab.Transform.Position = new XnaVector3(0, 15, 15);
            lightPrefab.Light.Direction = new XnaVector3(0, 1, -1);

            // Grid
            var terrain = new TerrainPrefab("terrain");
            _scene.Add(terrain);
            terrain.Flat();
            terrain.Renderer.Material = new TransparentMaterial(_scene);
            terrain.Renderer.Material.MainTexture = GraphicsHelper.CreateBorderTexture(XnaColor.Gray, XnaColor.Transparent, 256, 256, 2);
            terrain.Renderer.Material.Tiling = new XnaVector2(16); ;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            camera.Transform.Position = new XnaVector3(-terrain.Width >> 1, 2, -terrain.Depth / 2);

            _renderer.Skybox.Generate(GraphicsDevice, _content, new string[6] 
            {
                "Editor/Skybox/skybox_gradiant_side",   
                "Editor/Skybox/skybox_gradiant_side",
                "Editor/Skybox/skybox_gradiant_bottom",
                "Editor/Skybox/skybox_gradiant_top",
                "Editor/Skybox/skybox_gradiant_side",
                "Editor/Skybox/skybox_gradiant_side"
            });
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            int width = (int)sizeInfo.NewSize.Width;
            int height = (int)sizeInfo.NewSize.Height;

            Screen.Setup(width, height, null, null);

            _mainCamera.ComputeProjectionMatrix(MathHelper.PiOver4, (float)width / (float)height, 1, 2000);

            _renderer.NeedsBufferUpdate = true;
        }

        protected override void Update(Stopwatch timer)
        {
            base.Update(timer);

            _gameTime.ElapsedGameTime = timer.Elapsed;
            _gameTime.TotalGameTime += timer.Elapsed;

            if (_sceneObjectToAdd.Count > 0)
            {
                foreach (var type in _sceneObjectToAdd)
                    InternalAddSceneObject(type);

                _sceneObjectToAdd.Clear();
            }

            foreach (var component in _gameComponents)
                component.Update(_gameTime);

            if (Input.Mouse.Clicked(MouseButton.Left))
            {
                var ray = _mainCamera.GetRay(Input.Mouse.Position);
                RaycastInfo info;

                if (_scene.Raycast(ray, 100, out info))
                {
                    _selectedObject = info.Collider.SceneObject;
                    Debug.Log("SceneObject selected");
                }
            }

            if (_selectedObject != null)
            {
                if (Input.Mouse.Down(MouseButton.Left))
                    _selectedObject.Transform.Translate(Input.Mouse.Delta.X, 0, Input.Mouse.Delta.Y);
            }

            _scene.Update();
        }

        protected override void Draw(RenderTarget2D renderTarget)
        {
            graphicsDevice.Clear(XnaColor.CornflowerBlue);
            _renderer.RenderEditor(_scene, _scene.MainCamera, renderTarget);
        }

        public object GetService(Type serviceType)
        {
            return _services.GetService(serviceType);
        }

        public void Add(string type)
        {
            _sceneObjectToAdd.Add(type);
        }

        private void InternalAddSceneObject(string type)
        {
            SceneObject sceneObject = null;

            switch (type)
            {
                case "Cube":
                    sceneObject = new CubePrefab(type);
                    break;

                case "Cylinder": break;
                case "Quad": break;
                case "Plane": break;
                case "Pyramid": break;
                case "Sphere":
                    sceneObject = new SpherePrefab(type);
                    break;
                case "Torus": break;

                case "Terrain":
                    var terrain = new TerrainPrefab(type);
                    terrain.Flat();
                    terrain.Renderer.Material = _scene.DefaultMaterial;
                    sceneObject = terrain;
                    break;

                case "Water":
                    var water = new WaterPrefab(type);
                    water.Generate(string.Empty, string.Empty, new XnaVector3(10));
                    water.Renderer.Material.MainTexture = GraphicsHelper.CreateTexture(XnaColor.LightSeaGreen, 1, 1);
                    sceneObject = water;
                    break;

                case "Directional": sceneObject = new LightPrefab(type, LightType.Directional); break;
                case "Point": sceneObject = new LightPrefab(type, LightType.Point); break;
                case "Spot": sceneObject = new LightPrefab(type, LightType.Spot); break;

                case "Camera": sceneObject = new CameraPrefab(type); break;
                default: break;
            }

            _scene.Add(sceneObject);

            if (SceneObjectAdded != null)
                SceneObjectAdded(this, new SceneObjectAddedEventArgs(sceneObject));
        }
    }
}
