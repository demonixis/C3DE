using C3DE.Components;
using C3DE.Components.Colliders;
using C3DE.Components.Lights;
using C3DE.Components.Renderers;
using C3DE.Editor.Core.Components;
using C3DE.Editor.Events;
using C3DE.Geometries;
using C3DE.Inputs;
using C3DE.Materials;
using C3DE.Prefabs;
using C3DE.Prefabs.Meshes;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace C3DE.Editor.Core
{
    public class EDScene : Scene
    {
        internal const string EditorTag = "C3DE_Editor";
        internal Camera camera;
        internal Light light;
        internal Terrain grid;

        private List<string> _addList;
        private List<SceneObject> _removeList;
        private SceneObjectSelector _selectedObject;
        private BasicEditionSceneObject _editionSceneObject;

        public EDScene(string name)
            : base(name)
        {
            _addList = new List<string>();
            _removeList = new List<SceneObject>();
            _selectedObject = new SceneObjectSelector();
            _editionSceneObject = new BasicEditionSceneObject();
        }

        public override void Initialize()
        {
            base.Initialize();

            DefaultMaterial = new SimpleMaterial(this);
            DefaultMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.LightSkyBlue, Color.LightGray, 64, 64, 1);

            camera = CreateAddSceneObject<Camera>("EditorCamera.Main");
            camera.Setup(new Vector3(0.0f, 20.0f, -20.0f), Vector3.Zero, Vector3.Up);
            camera.Transform.Rotation = new Vector3(MathHelper.Pi / 6, 0.0f, 0.0f);
            camera.AddComponent<EDFirstPersonCamera>();

            light = CreateAddSceneObject<Light>("Editor_MainLight");
            light.Transform.Position = new Vector3(0, 15, 15);
            light.Direction = new Vector3(0, 0.75f, 0.75f);

            // Grid
            var gridMaterial = new UnlitMaterial(this);
            gridMaterial.Texture = GraphicsHelper.CreateCheckboardTexture(new Color(0.6f, 0.6f, 0.6f), new Color(0.95f, 0.95f, 0.95f), 256, 256);;
            gridMaterial.Tiling = new Vector2(24);

            grid = new Terrain("Editor_Grid");
            grid.Renderer.Material = gridMaterial;
            grid.Flatten();
            grid.Transform.SetPosition(-grid.Width / 2, -1, -grid.Depth / 2);
            Add(grid);

            EDRegistry.Camera = camera;

            Messenger.Register(EditorEvent.CreateSceneObject, CreateNewObject);
            Messenger.Register(EditorEvent.CommandDelete, RemoveSceneObject);
            Messenger.Register(EditorEvent.SceneObjectRenamed, OnSceneObjectRenamed);
            Messenger.Register(EditorEvent.TransformChanged, OnTransformChanged);
            Messenger.Register(EditorEvent.CommandEscape, UnselectObject);
            Messenger.Register(EditorEvent.CommandCopy, CopySelection);
            Messenger.Register(EditorEvent.CommandPast, PastSelection);
            Messenger.Register(EditorEvent.CommandDuplicate, DuplicateSelection);
        }

        public override void Update()
        {
            base.Update();

            if (_removeList.Count > 0)
            {
                foreach (var sceneObject in _removeList)
                    InternalRemoveSceneObject(sceneObject);

                _removeList.Clear();
            }

            if (_addList.Count > 0)
            {
                foreach (var type in _addList)
                    InternalAddSceneObject(type);

                _addList.Clear();
            }

            if (EDRegistry.Mouse.Clicked(MouseButton.Left))
            {
                var ray = camera.GetRay(EDRegistry.Mouse.Position);
                RaycastInfo info;

                if (Raycast(ray, 100, out info))
                {
                    if (info.Collider.SceneObject == _selectedObject.SceneObject)
                        return;

                    if (info.Collider.SceneObject.Tag == EditorTag)
                        return;

                    if (info.Collider.SceneObject != _selectedObject.SceneObject)
                        UnselectObject();

                    SelectObject(info.Collider.SceneObject);
                }
            }

            else if (_selectedObject.SceneObject != null)
            {
                if (EDRegistry.Mouse.Down(MouseButton.Left))
                {
                    _selectedObject.SceneObject.Transform.Translate(EDRegistry.Mouse.Delta.X, 0, EDRegistry.Mouse.Delta.Y);
                    Messenger.Notify(EditorEvent.TransformUpdated, new TransformChanged(TransformChangeType.Position, _selectedObject.SceneObject.Transform.Position));
                }
            }
        }

        #region Add / Remove SceneObject

        private T CreateAddSceneObject<T>(string name) where T : Component, new()
        {
            var sceneObject = new SceneObject(name);
            sceneObject.Tag = EditorTag;
            Add(sceneObject);
            return sceneObject.AddComponent<T>();
        }

        public void CreateNewObject(BasicMessage m)
        {
            _addList.Add(m.Message);
        }

        public void RemoveSceneObject(BasicMessage m)
        {
            if (!_selectedObject.IsNull())
            {
                InternalRemoveSceneObject(_selectedObject.SceneObject);
                UnselectObject();
            }
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
                    var terrain = new Terrain(type);
                    terrain.Flatten();
                    terrain.Renderer.Material = DefaultMaterial;
                    sceneObject = terrain;
                    break;

                case "Water":
                    var water = new WaterPrefab(type);
                    Add(water);
                    water.Generate(string.Empty, string.Empty, new Vector3(10));
                    water.Renderer.Material.Texture = GraphicsHelper.CreateTexture(Color.LightSeaGreen, 1, 1);
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

            Add(sceneObject);

            Messenger.Notify(EditorEvent.SceneObjectAdded, new GenericMessage<SceneObject>(sceneObject));
            SelectObject(sceneObject);
        }

        private void InternalRemoveSceneObject(SceneObject sceneObject)
        {
            Messenger.Notify(EditorEvent.SceneObjectRemoved, sceneObject.Id);
            Remove(sceneObject);
        }

        #endregion

        #region Select / Unselect a SceneObject

        private void SelectObject(SceneObject sceneObject)
        {
            UnselectObject();

            _selectedObject.Set(sceneObject);
            _selectedObject.Select(true);
            _editionSceneObject.Selected = sceneObject;

            Messenger.Notify(EditorEvent.SceneObjectSelected, new GenericMessage<SceneObject>(sceneObject));
            Messenger.Notify(EditorEvent.TransformUpdated, new GenericMessage<Transform>(sceneObject.Transform));
        }

        private void UnselectObject(BasicMessage m = null)
        {
            _selectedObject.Select(false);
            _editionSceneObject.Reset();
            Messenger.Notify(EditorEvent.SceneObjectUnSelected);
        }

        #endregion


        #region Handler for component changes

        private void OnSceneObjectRenamed(BasicMessage m)
        {
            var data = m as GenericMessage<bool>;
            if (data != null && !_selectedObject.IsNull())
            {
                _selectedObject.SceneObject.Name = data.Message;
                _selectedObject.SceneObject.Enabled = data.Value;
            }
        }

        private void OnTransformChanged(BasicMessage m)
        {
            var data = m as TransformChanged;
            if (data != null && !_selectedObject.IsNull())
            {
                var type = (int)data.ChangeType;
                if (type == 0)
                    _selectedObject.SceneObject.Transform.SetPosition(data.X, data.Y, data.Z);
                else if (type == 1)
                    _selectedObject.SceneObject.Transform.SetRotation(data.X, data.Y, data.Z);
                else if (type == 2)
                    _selectedObject.SceneObject.Transform.LocalScale = new Vector3(data.X, data.Y, data.Z);
            }
        }

        #endregion

        #region Copy/Duplicate/Past

        public void CopySelection(BasicMessage m = null)
        {
            _editionSceneObject.CopySelection();
        }

        public void DuplicateSelection(BasicMessage m = null)
        {
            _editionSceneObject.Copy = _selectedObject.SceneObject;
            _editionSceneObject.PastSelection(InternalAddSceneObject);
        }

        public void PastSelection(BasicMessage m = null)
        {
            _editionSceneObject.PastSelection(InternalAddSceneObject);
        }

        public void DeleteSelection(BasicMessage m = null)
        {
            if (!_selectedObject.IsNull())
            {
                InternalRemoveSceneObject(_selectedObject.SceneObject);
                UnselectObject();
            }
        }

        #endregion
    }
}
