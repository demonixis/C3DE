﻿using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo;
using C3DE.Demo.Scripts;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using C3DE.UI;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using XNAGizmo;

namespace C3DE.Editor
{
    public class EditorDemo : Scene
    {
        public enum Primitive
        {
            Cube, Cylinder, Quad, Plane, Pyramid, Sphere, Torus
        }

        private const string EditorTag = "Editor_Object";
        protected Camera m_Camera;
        protected Light m_DirectionalLight;
        private EditorApp m_EditorApp;
        private GizmoComponent m_Gizmo;
        private ObjectSelector m_ObjectSelector;
        private UnlitMaterial m_DefaultMaterial;
        private List<string> _addList;
        private List<GameObject> _removeList;
        private CopyPast _editionSceneObject;

        private bool pendingAdd = false;

        private bool pendingRemove = false;

        public EditorDemo() : base("3D Editor")
        {
            _addList = new List<string>();
            _removeList = new List<GameObject>();
            m_ObjectSelector = new ObjectSelector();
            _editionSceneObject = new CopyPast();
            _editionSceneObject.GameObjectAdded += InternalAddSceneObject;
        }

        public override void Initialize()
        {
            base.Initialize();

            GUI.Skin = DemoGame.CreateSkin(Application.Content, false);

            // Add a camera with a FPS controller
            var camera = GameObjectFactory.CreateCamera(new Vector3(0, 2, -10), new Vector3(0, 0, 0), Vector3.Up);

            m_Camera = camera.GetComponent<Camera>();
            m_Camera.AddComponent<DemoBehaviour>();
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
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, Application.Content, DemoGame.BlueSkybox);

            // Fog: Setup fog mode with some value. It's still disabled, but those values are used by the post processing fog effect.
            RenderSettings.FogDensity = 0.0085f;
            RenderSettings.FogMode = FogMode.None;
            RenderSettings.FogColor = Color.FloralWhite;


            m_EditorApp = m_Camera.AddComponent<EditorApp>();
            m_EditorApp.Clicked += M_EditorApp_Clicked;
            m_Gizmo = new GizmoComponent(Application.Engine, Application.GraphicsDevice);
            m_Gizmo.ActiveMode = GizmoMode.Translate;

            Application.Engine.Components.Add(m_Gizmo);

            m_Gizmo.TranslateEvent += M_Gizmo_TranslateEvent;
            m_Gizmo.RotateEvent += M_Gizmo_RotateEvent;
            m_Gizmo.ScaleEvent += M_Gizmo_ScaleEvent;

            m_ObjectSelector = new ObjectSelector();

            // Grid
            var gridMaterial = new UnlitMaterial();
            gridMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(new Color(0.6f, 0.6f, 0.6f), new Color(0.95f, 0.95f, 0.95f), 256, 256); ;
            gridMaterial.Tiling = new Vector2(24);

            var terrain = GameObjectFactory.CreateTerrain();
            terrain.Tag = EditorTag;

            var grid = terrain.GetComponent<Terrain>();
            grid.Renderer.Material = gridMaterial;
            grid.Renderer.ReceiveShadow = true;
            grid.Renderer.CastShadow = false;
            grid.Flatten();

            m_DefaultMaterial = new UnlitMaterial();
            m_DefaultMaterial.MainTexture = GraphicsHelper.CreateCheckboardTexture(Color.WhiteSmoke, Color.DarkSlateGray);
        }

        private void M_EditorApp_Clicked(string name)
        {
            GameObject selected = InternalAddSceneObject(name);

            if (selected != null)
            {
                m_ObjectSelector.Set(selected);
                m_Gizmo.Selection.Add(selected.Transform);
            }
        }

        public override void Unload()
        {
            base.Unload();
            Application.Engine.Components.Remove(m_Gizmo);
        }

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

        #region Add

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
                    var go = GameObjectFactory.CreateTerrain();
                    var terrain = go.GetComponent<Terrain>();
                    terrain.Flatten();
                    terrain.Renderer.Material = DefaultMaterial;
                    gameObject = go;
                    break;

                case "Water":
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
            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            Add(sceneObject, true);

            SelectObject(sceneObject);
        }

        private void InternalRemoveSceneObject(GameObject sceneObject)
        {
            Remove(sceneObject, true);
        }

        #endregion

        #region Select / Unselect a SceneObject

        private void SelectObject(GameObject sceneObject)
        {
            UnselectObject();

            m_ObjectSelector.Set(sceneObject);
            m_ObjectSelector.Select(true);
            _editionSceneObject.Selected = sceneObject;
            m_Gizmo.Selection.Add(sceneObject.Transform);
        }

        private void UnselectObject()
        {
            m_Gizmo.Clear();
            m_ObjectSelector.Select(false);
            _editionSceneObject.Reset();
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
            _editionSceneObject.Reset();

            m_ObjectSelector.Set(sceneObject);
            m_ObjectSelector.Select(true);
            _editionSceneObject.Selected = sceneObject;
        }

        #endregion


        #region Copy/Duplicate/Past

        public void CopySelection()
        {
            _editionSceneObject.CopySelection();
        }

        public void DuplicateSelection()
        {
            _editionSceneObject.Copy = m_ObjectSelector.GameObject;
            _editionSceneObject.PastSelection();
        }

        public void PastSelection()
        {
            _editionSceneObject.PastSelection();
        }

        public void DeleteSelection()
        {
            if (m_ObjectSelector.IsNull())
                return;

            InternalRemoveSceneObject(m_ObjectSelector.GameObject);
            UnselectObject();
        }

        #endregion
    }
}