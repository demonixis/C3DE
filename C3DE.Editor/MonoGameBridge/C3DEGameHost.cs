using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Editor.Core.Components;
using C3DE.Editor.Events;
using C3DE.Editor.Exporters;
using C3DE.Geometries;
using C3DE.Inputs;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Prefabs.Meshes;
using C3DE.Rendering;
using C3DE.Serialization;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace C3DE.Editor.MonoGameBridge
{
    public class SceneChangedEventArgs : EventArgs
    {
        public bool Added { get; set; }
        public string Name { get; set; }

        public SceneChangedEventArgs(string name, bool added)
        {
            Name = name;
            Added = added;
        }
    }

    public sealed class C3DEGameHost : D3D11Host, IServiceProvider
    {
        private GameTime _gameTime;
        private GameServiceContainer _services;
        private List<GameComponent> _gameComponents;
        private EDMouseComponent _mouse;
        private SpriteBatch _spriteBatch;
        private Renderer _renderer;
        private ContentManager _content;
        private Scene _scene;
        private List<string> _toAdd;
        private List<SceneObject> _toRemove;
        private SceneObject _selected;
        private SceneObject _editorCopy;

        public event EventHandler<SceneChangedEventArgs> SceneObjectAdded = null;
        public event EventHandler<SceneChangedEventArgs> SceneObjectRemoved = null;

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

            _renderer.NeedsBufferUpdate = true;
        }

        #endregion

        #region Life cycle

        protected override void Initialize()
        {
            base.Initialize();

            _toAdd = new List<string>();
            _toRemove = new List<SceneObject>();

            _gameTime = new GameTime(TimeSpan.Zero, TimeSpan.Zero);
            _gameComponents = new List<GameComponent>();

            _services = new GameServiceContainer();
            _services.AddService<IGraphicsDeviceService>(new GraphicsDeviceService(graphicsDevice));

            _content = new ContentManager(this);
            _content.RootDirectory = "Content";
            _scene = new Scene("Root");

            Application.Content = _content;
            Application.GraphicsDevice = GraphicsDevice;

            _mouse = new EDMouseComponent(null, this);
            Input.Mouse = _mouse;

            _gameComponents.Add(new Time(null));
            _gameComponents.Add(Input.Mouse);

            Screen.Setup((int)ActualWidth, (int)ActualHeight, null, null);

            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _renderer = new Renderer();
            _renderer.Initialize(_content);
            _scene.Initialize();

            foreach (var component in _gameComponents)
                component.Initialize();

            CreateEditorScene();

            _scene.RenderSettings.Skybox.Generate();

            Messenger.Register(EditorEvent.SceneObjectChanged, OnSceneObjectChanged);
            Messenger.Register(EditorEvent.TransformChanged, OnTransformChanged);
            Messenger.Register(EditorEvent.KeyJustPressed, OnKeyDown);
            Messenger.Register(EditorEvent.CommandCopy, CopySelection);
            Messenger.Register(EditorEvent.CommandPast, PastSelection);
            Messenger.Register(EditorEvent.CommandDuplicate, DuplicateSelection);
        }



        protected override void Update(Stopwatch timer)
        {
            base.Update(timer);

            _gameTime.ElapsedGameTime = timer.Elapsed;
            _gameTime.TotalGameTime += timer.Elapsed;

            if (_toRemove.Count > 0)
            {
                foreach (var sceneObject in _toRemove)
                    InternalRemoveSceneObject(sceneObject);

                _toRemove.Clear();
            }

            if (_toAdd.Count > 0)
            {
                foreach (var type in _toAdd)
                    InternalAddSceneObject(type);

                _toAdd.Clear();
            }

            foreach (var component in _gameComponents)
                component.Update(_gameTime);

            if (Input.Mouse.Clicked(MouseButton.Left))
            {
                var ray = Camera.Main.GetRay((Input.Mouse as EDMouseComponent).Position);
                RaycastInfo info;

                if (_scene.Raycast(ray, 100, out info))
                {
                    if (info.Collider.SceneObject == _selected)
                        return;

                    if (info.Collider.SceneObject != _selected)
                        UnselectSceneObject();

                    SelectObject(info.Collider.SceneObject);
                }
            }

            else if (_selected != null)
            {
                if (Input.Mouse.Down(MouseButton.Left))
                {
                    _selected.Transform.Translate(Input.Mouse.Delta.X, 0, Input.Mouse.Delta.Y);
                    Messenger.Notify(EditorEvent.TransformUpdated, new TransformChanged(TransformChangeType.Position, _selected.Transform.Position.X, _selected.Transform.Position.Y, _selected.Transform.Position.Z));
                }
            }

            _scene.Update();
        }

        protected override void Draw(RenderTarget2D renderTarget)
        {
            graphicsDevice.Clear(Color.CornflowerBlue);
            _renderer.RenderEditor(_scene, _scene.MainCamera, renderTarget);
        }

        #endregion

        #region Scene settings

        private void CreateEditorScene()
        {
            var defaultMaterial = new SimpleMaterial(_scene);
            defaultMaterial.MainTexture = GraphicsHelper.CreateBorderTexture(Color.LightSkyBlue, Color.LightGray, 64, 64, 1);
            _scene.DefaultMaterial = defaultMaterial;

            var camera = new CameraPrefab("EditorCamera.Main");
            camera.AddComponent<EDOrbitController>();
            _scene.Add(camera);

            var lightPrefab = new LightPrefab("Editor_MainLight", LightType.Directional);
            _scene.Add(lightPrefab);
            lightPrefab.Transform.Position = new Vector3(0, 15, 15);
            lightPrefab.Light.Direction = new Vector3(0, 0.75f, 0.75f);

            // Grid
            var gridMaterial = new SimpleMaterial(_scene);
            gridMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(0.4f, 0.4f, 0.4f), new Color(0.9f, 0.9f, 0.9f), 256, 256);
            gridMaterial.Tiling = new Vector2(24.0f);
            gridMaterial.Alpha = 0.6f;

            var terrain = new TerrainPrefab("Editor_Grid");
            _scene.Add(terrain);
            terrain.Flat();
            terrain.Renderer.Material = gridMaterial;
            terrain.Transform.Translate(-terrain.Width >> 1, -1.0f, -terrain.Depth / 2);

            camera.Transform.Position = new Vector3(-terrain.Width >> 1, 2, -terrain.Depth / 2);
        }

        #endregion

        #region Other Handlers

        private void OnKeyDown(BasicMessage m)
        {
            if (m.Message == "Escape")
                UnselectSceneObject();
        }

        #endregion

        #region Add / Remove SceneObject

        public void Add(string type)
        {
            _toAdd.Add(type);
        }

        private void InternalAddSceneObject(string type)
        {
            SceneObject sceneObject = null;

            switch (type)
            {
                case "Cube": sceneObject = new MeshPrefab<CubeGeometry>(type); break;
                case "Cylinder": sceneObject = new MeshPrefab<CylinderGeometry>(type); break;
                case "Quad": sceneObject = new MeshPrefab<QuadGeometry>(type); break;
                case "Plane": sceneObject = new MeshPrefab<PlaneGeometry>(type); break;
                case "Pyramid": sceneObject = new MeshPrefab<PyramidGeometry>(type); break;
                case "Sphere": sceneObject = new MeshPrefab<SphereGeometry>(type); break;
                case "Torus": sceneObject = new MeshPrefab<TorusGeometry>(type); break;

                case "Terrain":
                    var terrain = new TerrainPrefab(type);
                    terrain.Flat();
                    terrain.Renderer.Material = _scene.DefaultMaterial;
                    sceneObject = terrain;
                    break;

                case "Water":
                    var water = new WaterPrefab(type);
                    _scene.Add(water);
                    water.Generate(string.Empty, string.Empty, new Vector3(10));
                    water.Renderer.Material.MainTexture = GraphicsHelper.CreateTexture(Color.LightSeaGreen, 1, 1);
                    sceneObject = water;
                    break;

                case "Directional": sceneObject = new LightPrefab(type, LightType.Directional); break;
                case "Point": sceneObject = new LightPrefab(type, LightType.Point); break;
                case "Spot": sceneObject = new LightPrefab(type, LightType.Spot); break;

                case "Camera": sceneObject = new CameraPrefab(type); break;
                default: break;
            }

            InternalAddSceneObject(sceneObject);
        }

        private void InternalAddSceneObject(SceneObject sceneObject)
        {
            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            _scene.Add(sceneObject);

            if (SceneObjectAdded != null)
                SceneObjectAdded(this, new SceneChangedEventArgs(sceneObject.Name, true));

            SelectObject(sceneObject);
        }

        private void InternalRemoveSceneObject(SceneObject sceneObject)
        {
            if (SceneObjectRemoved != null)
                SceneObjectAdded(this, new SceneChangedEventArgs(sceneObject.Name, false));

            _scene.Remove(sceneObject);
        }

        #endregion

        #region Select / Unselect a SceneObject

        private void SelectObject(SceneObject sceneObject)
        {
            UnselectSceneObject();

            _selected = sceneObject;

            var boxRenderer = _selected.GetComponent<BoundingBoxRenderer>();
            if (boxRenderer == null)
                boxRenderer = _selected.AddComponent<BoundingBoxRenderer>();

            boxRenderer.Enabled = true;

            Messenger.Notify(EditorEvent.SceneObjectUpdated, new SceneObjectControlChanged(_selected.Name, _selected.Enabled));
            Messenger.Notify(EditorEvent.TransformUpdated, new GenericMessage<Transform>(_selected.Transform));
        }

        private void UnselectSceneObject()
        {
            if (_selected != null)
            {
                _selected.GetComponent<BoundingBoxRenderer>().Enabled = false;
                _selected = null;
                _editorCopy = null;
            }
        }

        #endregion

        #region Handler for component changes

        private void OnSceneObjectChanged(BasicMessage m)
        {
            var data = m as SceneObjectControlChanged;
            if (data != null && _selected != null)
            {
                _selected.Name = data.Name;
                _selected.Enabled = data.Enable;
            }
        }

        private void OnTransformChanged(BasicMessage m)
        {
            var data = m as TransformChanged;
            if (data != null && _selected != null)
            {
                var type = (int)data.ChangeType;
                if (type == 0)
                    _selected.Transform.SetPosition(data.X, data.Y, data.Z);
                else if (type == 1)
                    _selected.Transform.SetRotation(data.X, data.Y, data.Z);
                else if (type == 2)
                    _selected.Transform.LocalScale = new Vector3(data.X, data.Y, data.Z);
            }
        }

        #endregion

        #region New / Save and Load scene

        public void NewScene()
        {
            //_scene.Unload();
        }

        public string SaveScene()
        {
            var serialization = _scene.SerializeScene();
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

            var renderers = SceneObject.FindObjectsOfType<MeshRenderer>();

            if (format == "stl")
            {
                result = new string[1];
                result[0] = STLExporter.ExportMeshes(renderers);
            }
            else if (format == "obj")
            {
                result = new string[2];
                result[0] = OBJExporter.ExportMesh(_selected.GetComponent<MeshRenderer>());
                result[1] = string.Empty;
            }

            return result;
        }

        #endregion

        #region Copy/Duplicate/Past

        private void CopySelection(BasicMessage m)
        {
            _editorCopy = _selected;
        }

        private void DuplicateSelection(BasicMessage m)
        {
            _editorCopy = _selected;
            PastSelection(null);
        }

        private void PastSelection(BasicMessage m)
        {
            if (_editorCopy != null)
            {
                var sceneObject = (SceneObject)_editorCopy.Clone();

                var previous = _editorCopy;

                InternalAddSceneObject(sceneObject);

                _editorCopy = previous;

                var x = _editorCopy.Transform.Position.X;
                x += _editorCopy.GetComponent<RenderableComponent>().BoundingSphere.Radius * 2.0f;

                sceneObject.Transform.SetPosition(x, null, null);

                if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    _editorCopy = _selected;
            }
        }

        #endregion
    }
}
