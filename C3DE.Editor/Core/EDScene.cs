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
using System;
using System.Collections.Generic;
using System.IO;
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

        public bool GridVisible
        {
            get { return grid.Enabled; }
            set { grid.Enabled = value; }
        }

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

            DefaultMaterial.Texture = GraphicsHelper.CreateBorderTexture(Color.DimGray, Color.LightGray, 128, 128, 2);

            camera = CreateAddSceneObject<Camera>("EditorCamera.Main");
            camera.Setup(new Vector3(0.0f, 10.0f, 30.0f), Vector3.Zero, Vector3.Up);
            camera.Transform.Rotation = new Vector3(-MathHelper.Pi / 6, 0.0f, 0.0f);
            camera.AddComponent<EDFirstPersonCamera>();

            light = CreateAddSceneObject<Light>("Directional Light", false);
            light.Transform.Position = new Vector3(0, 150, 150);
            light.Transform.Rotation = new Vector3(-0.6f, 1, 0.6f);
            light.TypeLight = LightType.Directional;
            light.Backing = LightRenderMode.RealTime;
            light.Color = Color.White;
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
            var i = 0;
            var colors = new Color[] { Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Purple };
            var names = new string[] { "Red", "Green", "Blue", "Yellow", "Purple" };
            var size = colors.Length;

            // Procedurales textures.
            for (i = 0; i < size; i++)
                CreateMaterial(string.Format("Border {0}", names[i]), GraphicsHelper.CreateBorderTexture(colors[i], Color.LightGray, 64, 64, 1));

            for (i = 0; i < size; i++)
                CreateMaterial(string.Format("Checkboard {0}", names[i]), GraphicsHelper.CreateCheckboardTexture(colors[i], Color.LightGray, 64, 64));

            // Voxel pack by Kenney Vleugels for Kenney (www.kenney.nl) / License (Creative Commons Zero, CC0)
            var path = Path.Combine("Content", "Textures", "VoxelPack");
            var files = Directory.GetFiles(Path.Combine(path, "Tiles"));
            var name = string.Empty;
            for (i = 0; i < files.Length; i++)
            {
                name = Path.GetFileNameWithoutExtension(files[i]);
                CreateMaterial(name, "Textures/VoxelPack/Tiles/" + name);
            }

            files = Directory.GetFiles(Path.Combine(path, "Tiles", "Details"));
            for (i = 0; i < files.Length; i++)
            {
                name = Path.GetFileNameWithoutExtension(files[i]);
                CreateBillboardMaterial(name, "Textures/VoxelPack/Tiles/Details/" + name);
            }

            // Terrain textures
            CreateMaterial("Terrain Grass", "Textures/Terrain/Grass");
            CreateMaterial("Terrain Rock", "Textures/Terrain/Rock");
            CreateMaterial("Terrain Sand", "Textures/Terrain/Sand");
            CreateMaterial("Terrain Snow", "Textures/Terrain/Snow");

            // Billboards for the editor
            CreateBillboardMaterial("ED_Camera", "Icons/Camera_Icon");
            CreateBillboardMaterial("ED_Light", "Icons/Light_Icon");

            // Water material
            var waterMaterial = new WaterMaterial(this, "WaterMaterial");
            waterMaterial.Texture = Asset.LoadTexture("Textures/water");
            waterMaterial.NormalMap = Asset.LoadTexture("Textures/wavesbump");
        }

        private void CreateMaterial(string name, string path)
        {
            CreateMaterial(name, Asset.LoadTexture(path));
        }

        private void CreateBillboardMaterial(string name, string path)
        {
            CreateBillboardMaterial(name, Asset.LoadTexture(path));
        }

        private void CreateMaterial(string name, Texture2D texture)
        {
            Material material = null;

            if (name.Contains("_alpha"))
                material = new TransparentMaterial(this, name.Replace("_alpha", ""));
            else
                material = new StandardMaterial(this, name);

            material.Texture = texture;
        }

        private void CreateBillboardMaterial(string name, Texture2D texture)
        {
            var mat = new BillboardMaterial(this, name);
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

            if (EDRegistry.Mouse.JustClicked(MouseButton.Left) && _gizmo.ActiveAxis == GizmoAxis.None)
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

        private void GizmoTranslateEvent(Transform target, TransformationEventArgs e)
        {
            var value = (Vector3)e.Value;

            if (System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
            {
                if (Vector3.Distance((Vector3)e.Value, target.Position) > 0.05f)
                {
                    var x = target.Position.X + Math.Sign(value.X);
                    var y = target.Position.Y + Math.Sign(value.Y);
                    var z = target.Position.Z + Math.Sign(value.Z);
                    target.SetPosition(x, y, z);
                }
            }
            else
                target.Position += value;

            Messenger.Notify(EditorEvent.TransformUpdated);
        }

        private void GizmoRotateEvent(Transform target, TransformationEventArgs e)
        {
            _gizmo.RotationHelper(target, e);
            Messenger.Notify(EditorEvent.TransformUpdated);
        }

        private void GizmoScaleEvent(Transform target, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value;

            if (_gizmo.ActiveMode == GizmoMode.UniformScale)
                target.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                target.LocalScale += delta;

            target.LocalScale = Vector3.Clamp(target.LocalScale, Vector3.Zero, target.LocalScale);

            Messenger.Notify(EditorEvent.TransformUpdated);
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
                    sceneObject.AddComponent<EDBoxCollider>();

                    var camRenderer = sceneObject.AddComponent<EDMeshRenderer>();
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
            sceneObject.AddComponent<EDBoxCollider>();

            var light = sceneObject.AddComponent<Light>();
            light.TypeLight = type;

            var lightRenderer = sceneObject.AddComponent<EDMeshRenderer>();
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
            _gizmo.Selection.Add(sceneObject.Transform);
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
