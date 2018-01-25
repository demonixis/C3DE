using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.Inputs;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using XNAGizmo;

namespace C3DE.Editor
{
    public class EditorScene : Scene
    {
        public enum Primitive
        {
            Cube, Cylinder, Quad, Plane, Pyramid, Sphere, Torus
        }

        private const string EditorTag = "Editor_Object";
        protected Camera m_Camera;
        protected Light m_DirectionalLight;
        private GizmoComponent m_Gizmo;
        private ObjectSelector m_ObjectSelector;
        private StandardMaterial m_DefaultMaterial;
        private List<string> m_AddList;
        private List<GameObject> m_RemoveList;
        private CopyPast m_EditionSceneObject;

        public event Action<GameObject, bool> GameObjectAdded = null;
        public event Action<GameObject, bool> GameObjectSelected = null;

        public EditorScene() : base("3D Editor")
        {
            m_AddList = new List<string>();
            m_RemoveList = new List<GameObject>();
            m_ObjectSelector = new ObjectSelector();
            m_EditionSceneObject = new CopyPast();
            m_EditionSceneObject.GameObjectAdded += InternalAddSceneObject;
        }

        public void Reset()
        {
            Unload();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            m_Camera = camera.GetComponent<Camera>();
            m_Camera.Setup(new Vector3(0, 2, 5), Vector3.Forward, Vector3.Up);
            m_Camera.Far = 10000;
            m_Camera.AddComponent<EditorController>();

            // And a light
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 1f);
            lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
            lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
            m_DirectionalLight = lightGo.GetComponent<Light>();

            // Sun Flares
            var content = Application.Content;
            var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
            var flareTextures = new Texture2D[]
            {
                content.Load<Texture2D>("Textures/Flares/circle"),
                content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                content.Load<Texture2D>("Textures/Flares/circle_soft_1")
            };

            var direction = m_DirectionalLight.Direction;
            var sunflares = m_Camera.AddComponent<LensFlare>();
            sunflares.LightDirection = direction;
            sunflares.Setup(glowTexture, flareTextures);

            // Skybox

            var blueSkybox = new string[6]
            {
                "Textures/Skybox/bluesky/px",
                "Textures/Skybox/bluesky/nx",
                "Textures/Skybox/bluesky/py",
                "Textures/Skybox/bluesky/ny",
                "Textures/Skybox/bluesky/pz",
                "Textures/Skybox/bluesky/nz"
            };
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, blueSkybox);

            foreach (var component in Application.Engine.Components)
                if (component is GizmoComponent)
                    m_Gizmo = (GizmoComponent)component;

            m_Gizmo.ActiveMode = GizmoMode.Translate;
            m_Gizmo.TranslateEvent += M_Gizmo_TranslateEvent;
            m_Gizmo.RotateEvent += M_Gizmo_RotateEvent;
            m_Gizmo.ScaleEvent += M_Gizmo_ScaleEvent;

            m_ObjectSelector = new ObjectSelector();

            // Grid
            var gridMaterial = new UnlitMaterial();
            gridMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(0.6f, 0.6f, 0.6f), new Color(0.95f, 0.95f, 0.95f), 256, 256); ;

            var terrain = GameObjectFactory.CreateTerrain();
            terrain.Tag = EditorTag;

            var grid = terrain.GetComponent<Terrain>();
            grid.Geometry.Size = new Vector3(1.0f);
            grid.Geometry.TextureRepeat = new Vector2(96);
            grid.Renderer.Material = gridMaterial;
            grid.Renderer.ReceiveShadow = true;
            grid.Renderer.CastShadow = false;
            grid.Flatten();

            m_DefaultMaterial = new StandardMaterial();
            m_DefaultMaterial.MainTexture = GraphicsHelper.CreateTexture(Color.WhiteSmoke, 1, 1);

            GameObjectAdded?.Invoke(camera, true);
            GameObjectAdded?.Invoke(lightGo, true);
        }

        public void AddObject(string name)
        {
            var selected = InternalAddSceneObject(name);

            if (selected != null)
            {
                m_ObjectSelector.Set(selected);
                m_Gizmo.Selection.Add(selected.Transform);
            }
        }

        public override void Unload()
        {
            m_Gizmo.TranslateEvent -= M_Gizmo_TranslateEvent;
            m_Gizmo.RotateEvent -= M_Gizmo_RotateEvent;
            m_Gizmo.ScaleEvent -= M_Gizmo_ScaleEvent;
            base.Unload();
        }

        public override void Update()
        {
            base.Update();

            if (m_RemoveList.Count > 0)
            {
                foreach (var sceneObject in m_RemoveList)
                    InternalRemoveSceneObject(sceneObject);

                m_RemoveList.Clear();
            }

            if (m_AddList.Count > 0)
            {
                foreach (var type in m_AddList)
                    InternalAddSceneObject(type);

                m_AddList.Clear();
            }

            if (Input.Mouse.JustClicked(MouseButton.Left) && m_Gizmo.ActiveAxis == GizmoAxis.None)
            {
                var ray = Camera.Main.GetRay(Input.Mouse.Position);
                RaycastInfo info;

                if (Raycast(ray, 100, out info))
                {
                    if (info.Collider.GameObject == m_ObjectSelector.GameObject)
                        return;

                    if (info.Collider.GameObject.Tag == EditorTag)
                        return;

                    if (info.Collider.GameObject != m_ObjectSelector.GameObject)
                        UnselectObject();

                    SelectObject(info.Collider.GameObject);
                }
            }
        }

        #region Gizmo Management

        private void M_Gizmo_ScaleEvent(Transform target, TransformationEventArgs e)
        {
            Vector3 delta = (Vector3)e.Value;

            if (m_Gizmo.ActiveMode == GizmoMode.UniformScale)
                target.LocalScale *= 1 + ((delta.X + delta.Y + delta.Z) / 3);
            else
                target.LocalScale += delta;

            target.LocalScale = Vector3.Clamp(target.LocalScale, Vector3.Zero, target.LocalScale);
        }

        private void M_Gizmo_RotateEvent(Transform target, TransformationEventArgs e)
        {
            m_Gizmo.RotationHelper(target, e);
        }

        private void M_Gizmo_TranslateEvent(Transform target, TransformationEventArgs e)
        {
            var value = (Vector3)e.Value;

            if (Input.Keys.Pressed(Keys.LeftControl))
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
                target.LocalPosition += value;
        }

        #endregion

        #region Add

        public void AddComponent(string name)
        {
            switch (name)
            {

            }
        }

        private GameObject CreatePrimitive(Primitive type)
        {
            var gameObject = new GameObject();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.Material = m_DefaultMaterial;

            switch (type)
            {
                case Primitive.Cube: meshRenderer.Geometry = new CubeMesh(); break;
                case Primitive.Cylinder: meshRenderer.Geometry = new CylinderMesh(); break;
                case Primitive.Plane: meshRenderer.Geometry = new PlaneMesh(); break;
                case Primitive.Pyramid: meshRenderer.Geometry = new PyramidMesh(); break;
                case Primitive.Quad: meshRenderer.Geometry = new QuadMesh(); break;
                case Primitive.Sphere: meshRenderer.Geometry = new SphereMesh(); break;
                case Primitive.Torus: meshRenderer.Geometry = new TorusMesh(); break;
            }

            meshRenderer.Geometry.Build();

            gameObject.AddComponent<SphereCollider>();
            gameObject.Transform.Translate(0, meshRenderer.BoundingSphere.Radius, 0);

            return gameObject;
        }

        private GameObject InternalAddSceneObject(string type)
        {
            GameObject gameObject = null;

            switch (type)
            {
                case "Cube": gameObject = CreatePrimitive(Primitive.Cube); break;
                case "Cylinder": gameObject = CreatePrimitive(Primitive.Cylinder); break;
                case "Quad": gameObject = CreatePrimitive(Primitive.Quad); break;
                case "Plane": gameObject = CreatePrimitive(Primitive.Plane); break;
                case "Pyramid": gameObject = CreatePrimitive(Primitive.Pyramid); break;
                case "Sphere": gameObject = CreatePrimitive(Primitive.Sphere); break;
                case "Torus": gameObject = CreatePrimitive(Primitive.Torus); break;

                case "Terrain":
                    gameObject = GameObjectFactory.CreateTerrain();
                    var terrain = gameObject.GetComponent<Terrain>();
                    terrain.Flatten();
                    terrain.Renderer.Material = m_DefaultMaterial;
                    break;

                case "Water":
                    gameObject = GameObjectFactory.CreateWater(GraphicsHelper.CreateTexture(Color.AliceBlue, 1, 1), GraphicsHelper.CreateRandomTexture(32), Vector3.One);
                    break;

                case "Directional": gameObject = CreateLightNode(type, LightType.Directional); break;
                case "Point": gameObject = CreateLightNode(type, LightType.Point); break;
                case "Spot": gameObject = CreateLightNode(type, LightType.Spot); break;

                case "Camera":
                    gameObject = GameObjectFactory.CreateCamera();
                    gameObject.AddComponent<BoxCollider>();

                    var camRenderer = gameObject.AddComponent<MeshRenderer>();
                    camRenderer.CastShadow = false;
                    camRenderer.ReceiveShadow = false;
                    camRenderer.Geometry = new QuadMesh();
                    camRenderer.Geometry.Build();
                    camRenderer.Material = GetMaterialByName("Camera");
                    break;
                default: break;
            }

            gameObject.Name = type;

            InternalAddSceneObject(gameObject);

            return gameObject;
        }

        private GameObject CreateLightNode(string name, LightType type)
        {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<BoxCollider>();

            var light = gameObject.AddComponent<Light>();
            light.TypeLight = type;

            var lightRenderer = gameObject.AddComponent<MeshRenderer>();
            lightRenderer.CastShadow = false;
            lightRenderer.ReceiveShadow = false;
            lightRenderer.Geometry = new QuadMesh();
            lightRenderer.Geometry.Build();
            lightRenderer.Material = GetMaterialByName("Light");

            return gameObject;
        }

        private void InternalAddSceneObject(GameObject sceneObject)
        {
            if (sceneObject == null)
                return;

            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            Add(sceneObject, true);

            SelectObject(sceneObject);
            GameObjectAdded?.Invoke(sceneObject, true);
        }

        private void InternalRemoveSceneObject(GameObject sceneObject)
        {
            Remove(sceneObject, true);
            GameObjectAdded?.Invoke(sceneObject, false);
        }

        #endregion

        #region Select / Unselect a SceneObject

        private void SelectObject(GameObject sceneObject)
        {
            UnselectObject();

            m_ObjectSelector.Set(sceneObject);
            m_ObjectSelector.Select(true);
            m_EditionSceneObject.Selected = sceneObject;
            m_Gizmo.Selection.Add(sceneObject.Transform);
        }

        private void UnselectObject()
        {
            m_Gizmo.Clear();
            m_ObjectSelector.Select(false);
            m_EditionSceneObject.Reset();
        }

        public void SetSeletected(string id)
        {
            SetSelected(FindById(id));
        }

        private void SetSelected(GameObject sceneObject)
        {
            if (sceneObject == null)
                return;

            m_ObjectSelector.Select(false);
            m_EditionSceneObject.Reset();

            m_ObjectSelector.Set(sceneObject);
            m_ObjectSelector.Select(true);
            m_EditionSceneObject.Selected = sceneObject;
        }

        #endregion

        public GameObject[] GetUsedSceneObjects()
        {
            var list = new List<GameObject>();

            foreach (var gameObject in gameObjects)
                if (gameObject.Tag != EditorTag)
                    list.Add(gameObject);

            return list.ToArray();
        }
    }
}
