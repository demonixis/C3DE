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
    using Color = Microsoft.Xna.Framework.Color;

    public sealed class C3DEGameHost : D3D11Host, IServiceProvider
    {
        private GameTime _gameTime;
        private GameServiceContainer _services;
        private List<GameComponent> _gameComponents;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private ContentManager _content;
        private Scene _scene;

        protected override void Initialize()
        {
            base.Initialize();

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
            var camera = new CameraPrefab("camera", _scene);
            camera.AddComponent<EditorOrbitController>();

            var lightPrefab = new LightPrefab("lightPrefab", LightType.Directional, _scene);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Direction = new Vector3(0, 1, -1);

            // Terrain
            var terrainMaterial = new StandardMaterial(_scene);
            terrainMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.Blue, Color.LightBlue, 128, 128, 1);
            terrainMaterial.Shininess = 10;
            terrainMaterial.Tiling = new Vector2(16);

            var terrain = new TerrainPrefab("terrain", _scene);
            terrain.Flat();
            terrain.Renderer.Material = terrainMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, 0, -terrain.Depth / 2);

            camera.Transform.Position = new Vector3(-terrain.Width >> 1, 2, -terrain.Depth / 2);

            _renderer.Skybox.Generate(GraphicsDevice, _content, new string[6] 
            {
                "Textures/Skybox/bluesky/px",   
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            });
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            Screen.Setup((int)sizeInfo.NewSize.Width, (int)sizeInfo.NewSize.Height, null, null);
            _renderer.NeedsBufferUpdate = true;
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
            _renderer.RenderEditor(_scene, _scene.MainCamera, renderTarget);
        }

        public object GetService(Type serviceType)
        {
            return _services.GetService(serviceType);
        }
    }
}
