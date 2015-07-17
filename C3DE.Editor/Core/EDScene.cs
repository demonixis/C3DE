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
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Runtime.Serialization;
using XNAGizmo;

namespace C3DE.Editor.Core
{
    [DataContract]
    public class EDScene : Scene
    {
        internal const string EditorTag = "C3DE_Editor";
        internal Camera camera;
        internal Light light;
        internal TerrainPrefab grid;
        private List<string> _addList;
        private List<SceneObject> _removeList;
        private SceneObjectSelector _selectedObject;
        private BasicEditionSceneObject _editionSceneObject;
        private bool pendingAdd = false;
        private bool pendingRemove = false;
        private GizmoComponent _gizmo;

        public EDScene(string name, GizmoComponent gizmo)
            : base(name)
        {
            _addList = new List<string>();
            _removeList = new List<SceneObject>();
            _selectedObject = new SceneObjectSelector();
            _editionSceneObject = new BasicEditionSceneObject();
            _gizmo = gizmo;
            _gizmo.TranslateEvent += GizmoTranslateEvent;
            _gizmo.RotateEvent += GizmoRotateEvent;
            _gizmo.ScaleEvent += GizmoScaleEvent;
        }

        public override void Initialize()
        {
            base.Initialize();

            DefaultMaterial = new UnlitColorMaterial(this, "DefaultMaterial");
            DefaultMaterial.DiffuseColor = Color.DarkBlue;

            camera = CreateAddSceneObject<Camera>("EditorCamera.Main");
            camera.Setup(new Vector3(0.0f, 10.0f, -30.0f), Vector3.Zero, Vector3.Up);
            camera.Transform.Rotation = new Vector3(MathHelper.Pi / 6, 0.0f, 0.0f);
            camera.AddComponent<EDFirstPersonCamera>();

            light = CreateAddSceneObject<Light>("Directional Light", false);
            light.Transform.Position = new Vector3(0, 150, 150);
            light.Direction = new Vector3(-0.5f, 0.75f, -0.5f);
            light.TypeLight = LightType.Directional;
            light.Backing = LightRenderMode.RealTime;
            light.DiffuseColor = Color.White;
            light.EnableShadow = true;
            light.Intensity = 1.0f;
            light.ShadowGenerator.ShadowMapSize = 1024;
            light.ShadowGenerator.ShadowStrength = 0.4f;

            // Grid
            var gridMaterial = new StandardMaterial(this, "GridMaterial");
            gridMaterial.Texture = GraphicsHelper.CreateCheckboardTexture(new Color(0.6f, 0.6f, 0.6f), new Color(0.95f, 0.95f, 0.95f), 256, 256); ;
            gridMaterial.Tiling = new Vector2(24);

            grid = new TerrainPrefab("Editor_Grid");
            grid.Tag = EditorTag;
            grid.Renderer.Material = gridMaterial;
            grid.Renderer.ReceiveShadow = true;
            grid.Renderer.CastShadow = false;
            grid.Flatten();
            grid.Transform.SetPosition(-grid.Width / 2, -1, -grid.Depth / 2);
            Add(grid);

            CreateMaterialCollection();

            EDRegistry.Camera = camera;

            Messenger.Register(EditorEvent.CreateSceneObject, CreateNewObject);
            Messenger.Register(EditorEvent.CommandDelete, RemoveSceneObject);
            Messenger.Register(EditorEvent.CommandEscape, UnselectObject);
            Messenger.Register(EditorEvent.CommandCopy, CopySelection);
            Messenger.Register(EditorEvent.CommandPast, PastSelection);
            Messenger.Register(EditorEvent.CommandDuplicate, DuplicateSelection);
        }

        public override void Unload()
        {
            _gizmo.TranslateEvent -= GizmoTranslateEvent;
            _gizmo.RotateEvent -= GizmoRotateEvent;
            _gizmo.ScaleEvent -= GizmoScaleEvent;

            base.Unload();
        }

        private void CreateMaterialCollection()
        {
            // TODO: It's here for testing unitil the material creator is created.
            CreateMaterial("Border Red", GraphicsHelper.CreateBorderTexture(Color.Red, Color.LightGray, 64, 64, 1));
            CreateMaterial("Border Green", GraphicsHelper.CreateBorderTexture(Color.Green, Color.LightGray, 64, 64, 1));
            CreateMaterial("Border Blue", GraphicsHelper.CreateBorderTexture(Color.Blue, Color.LightGray, 64, 64, 1));

            CreateMaterial("Checkboard Red", GraphicsHelper.CreateCheckboardTexture(Color.Red, Color.LightGray, 64, 64));
            CreateMaterial("Checkboard Green", GraphicsHelper.CreateCheckboardTexture(Color.Green, Color.LightGray, 64, 64));
            CreateMaterial("Checkboard Blue", GraphicsHelper.CreateCheckboardTexture(Color.Blue, Color.LightGray, 64, 64));

            CreateMaterial("Circle Red", GraphicsHelper.CreateCircleTexture(Color.Red, Color.LightGray, 64));
            CreateMaterial("Circle Green", GraphicsHelper.CreateCircleTexture(Color.Green, Color.LightGray, 64));
            CreateMaterial("Circle Blue", GraphicsHelper.CreateCircleTexture(Color.Blue, Color.LightGray, 64));

            CreateMaterial("Random 1", GraphicsHelper.CreateRandomTexture(64));
            CreateMaterial("Random 2", GraphicsHelper.CreateRandomTexture(64));
            CreateMaterial("Random 3", GraphicsHelper.CreateRandomTexture(64));

            CreateMaterial("Grass", "Textures/Terrain/Grass");
            CreateMaterial("Rock", "Textures/Terrain/Rock");
            CreateMaterial("Sand", "Textures/Terrain/Sand");
            CreateMaterial("Snow", "Textures/Terrain/Snow");
            CreateMaterial("Hexa", "Textures/hexa_tex");

            var camMaterial = new BillboardMaterial(this, "Camera");
            camMaterial.Texture = Application.Content.Load<Texture2D>("Textures/Camera_Icon");

            var lightMaterial = new BillboardMaterial(this, "Light");
            lightMaterial.Texture = Application.Content.Load<Texture2D>("Textures/Light_Icon");

            var waterMaterial = new WaterMaterial(this, "WaterMaterial");
            waterMaterial.Texture = Application.Content.Load<Texture2D>("Textures/water");
            waterMaterial.NormalMap = Application.Content.Load<Texture2D>("Textures/wavesbump");
        }

        private void CreateMaterial(string name, string path)
        {
            CreateMaterial(name, Application.Content.Load<Texture2D>(path));
        }

        private void CreateMaterial(string name, Texture2D texture)
        {
            var mat = new StandardMaterial(this, name);
            mat.Texture = texture;
        }

        public override void Update()
        {
            base.Update();

            if (pendingAdd)
            {
                Messenger.Notify(EditorEvent.SceneObjectAdded);
                pendingAdd = false;
            }

            if (pendingRemove)
            {
                Messenger.Notify(EditorEvent.SceneObjectRemoved);
                pendingRemove = false;
            }

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

                pendingAdd = true;

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
        }

        #region Gizmo Handlers

        private void GizmoTranslateEvent(Renderer renderer, TransformationEventArgs e)
        {
            renderer.Transform.Position += (Vector3)e.Value;
            Messenger.Notify(EditorEvent.TransformUpdated, new TransformChanged(TransformChangeType.Position, _selectedObject.SceneObject.Transform.Position));
        }

        private void GizmoRotateEvent(Renderer renderer, TransformationEventArgs e)
        {
            _gizmo.RotationHelper(renderer, e);
            Messenger.Notify(EditorEvent.TransformUpdated, new TransformChanged(TransformChangeType.Position, _selectedObject.SceneObject.Transform.Position));
        }

        private void GizmoScaleEvent(Renderer renderer, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value;

            if (_gizmo.ActiveMode == GizmoMode.UniformScale)
                renderer.Transform.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                renderer.Transform.LocalScale += delta;

            renderer.Transform.LocalScale = Vector3.Clamp(renderer.Transform.LocalScale, Vector3.Zero, renderer.Transform.LocalScale);

            Messenger.Notify(EditorEvent.TransformUpdated, new TransformChanged(TransformChangeType.Position, _selectedObject.SceneObject.Transform.Position));
        }

        #endregion

        #region Add / Remove SceneObject

        private T CreateAddSceneObject<T>(string name, bool tag = true) where T : Component, new()
        {
            var sceneObject = new SceneObject(name);
            if (tag)
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
                case "Cube": sceneObject = new MeshPrefab(type, new CubeGeometry()); break;
                case "Cylinder": sceneObject = new MeshPrefab(type, new CylinderGeometry()); break;
                case "Quad": sceneObject = new MeshPrefab(type, new QuadGeometry()); break;
                case "Plane": sceneObject = new MeshPrefab(type, new PlaneGeometry()); break;
                case "Pyramid": sceneObject = new MeshPrefab(type, new PyramidGeometry()); break;
                case "Sphere": sceneObject = new MeshPrefab(type, new SphereGeometry()); break;
                case "Torus": sceneObject = new MeshPrefab(type, new TorusGeometry()); break;

                case "Terrain":
                    var terrain = new TerrainPrefab(type);
                    terrain.Flatten();
                    terrain.Renderer.Material = DefaultMaterial;
                    sceneObject = terrain;
                    break;

                case "Water":
                    var water = new WaterPrefab(type);
                    Add(water);
                    water.Generate(string.Empty, string.Empty, new Vector3(10));
                    water.Renderer.Material = GetMaterialByName("WaterMaterial");
                    sceneObject = water;
                    break;

                case "Directional": sceneObject = CreateLightNode(type, LightType.Directional); break;
                case "Point": sceneObject = CreateLightNode(type, LightType.Point); break;
                case "Spot": sceneObject = CreateLightNode(type, LightType.Spot); break;

                case "Camera":
                    sceneObject = new CameraPrefab(type);
                    sceneObject.AddComponent<BoxCollider>();

                    var camRenderer = sceneObject.AddComponent<MeshRenderer>();
                    camRenderer.CastShadow = false;
                    camRenderer.ReceiveShadow = false;
                    camRenderer.Geometry = new QuadGeometry();
                    camRenderer.Geometry.Build();
                    camRenderer.Material = GetMaterialByName("Camera");
                    break;
                default: break;
            }

            InternalAddSceneObject(sceneObject);
        }

        private SceneObject CreateLightNode(string name, LightType type)
        {
            var sceneObject = new SceneObject(name);
            sceneObject.AddComponent<BoxCollider>();

            var light = sceneObject.AddComponent<Light>();
            light.TypeLight = type;

            var lightRenderer = sceneObject.AddComponent<MeshRenderer>();
            lightRenderer.CastShadow = false;
            lightRenderer.ReceiveShadow = false;
            lightRenderer.Geometry = new QuadGeometry();
            lightRenderer.Geometry.Build();
            lightRenderer.Material = GetMaterialByName("Light");

            return sceneObject;
        }

        private void InternalAddSceneObject(SceneObject sceneObject)
        {
            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            Add(sceneObject, true);

            SelectObject(sceneObject);
        }

        private void InternalRemoveSceneObject(SceneObject sceneObject)
        {
            Messenger.Notify(EditorEvent.SceneObjectRemoved, sceneObject.Id);
            Remove(sceneObject, true);
        }

        #endregion

        #region Select / Unselect a SceneObject

        private void SelectObject(SceneObject sceneObject)
        {
            UnselectObject();

            _selectedObject.Set(sceneObject);
            _selectedObject.Select(true);
            _editionSceneObject.Selected = sceneObject;

            var renderer = sceneObject.GetComponent<Renderer>();
            if (renderer != null)
                _gizmo.Selection.Add(renderer);
            Messenger.Notify(EditorEvent.SceneObjectSelected, new GenericMessage<SceneObject>(sceneObject));
        }

        private void UnselectObject(BasicMessage m = null)
        {
            _gizmo.Clear();

            _selectedObject.Select(false);
            _editionSceneObject.Reset();

            Messenger.Notify(EditorEvent.SceneObjectUnSelected);
        }

        public void SetSeletected(string id)
        {
            SetSelected(FindById(id));
        }

        private void SetSelected(SceneObject sceneObject)
        {
            if (sceneObject != null)
            {
                _selectedObject.Select(false);
                _editionSceneObject.Reset();

                Messenger.Notify(EditorEvent.SceneObjectUnSelected);

                _selectedObject.Set(sceneObject);
                _selectedObject.Select(true);
                _editionSceneObject.Selected = sceneObject;

                Messenger.Notify(EditorEvent.SceneObjectSelected, new GenericMessage<SceneObject>(sceneObject));
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

        #region Utility / Misc

        public SceneObject[] GetSceneObjects()
        {
            return sceneObjects.ToArray();
        }

        public Material[] GetUsedMaterials()
        {
            var renderers = FindObjectsOfType<Renderer>();
            var list = new List<Material>();

            for (int i = 0, l = renderers.Length; i < l; i++)
                if (renderers[i].SceneObject.Tag != EditorTag && renderers[i].Material != null)
                    list.Add(renderers[i].Material);

            return list.ToArray();
        }

        public SceneObject[] GetUsedSceneObjects()
        {
            var list = new List<SceneObject>();

            for (int i = 0, l = sceneObjects.Count; i < l; i++)
                if (sceneObjects[i].Tag != EditorTag)
                    list.Add(sceneObjects[i]);

            return list.ToArray();
        }

        #endregion
    }
}
