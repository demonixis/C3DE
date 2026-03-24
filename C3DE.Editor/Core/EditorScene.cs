using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Editor.Core.Components;
using C3DE.Editor.GameComponents;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Editor
{
    public class EditorScene : Scene
    {
        private List<string> _addList;
        private List<GameObject> _removeList;
        private readonly bool _bootstrapDefaults;

        public event Action<GameObject[]> SceneInitialized = null;

        public EditorScene(bool bootstrapDefaults = true) : base("3D Editor")
        {
            _bootstrapDefaults = bootstrapDefaults;
            _addList = new List<string>();
            _removeList = new List<GameObject>();
        }

        public void Reset()
        {
            Unload();
            Initialize();
        }

        public override void Initialize()
        {
            base.Initialize();

            EnsureEditorInfrastructure();

            if (_bootstrapDefaults)
                EnsureEditorBootstrapObjects();

            SceneInitialized?.Invoke(GetGameObjects());
        }

        internal void EnsureEditorInfrastructure()
        {
            if (RenderSettings.Skybox.Texture == null)
            {
                var blueSkybox = new string[6]
                {
                    "Textures/Skybox/bluesky/px",
                    "Textures/Skybox/bluesky/nx",
                    "Textures/Skybox/bluesky/py",
                    "Textures/Skybox/bluesky/ny",
                    "Textures/Skybox/bluesky/pz",
                    "Textures/Skybox/bluesky/nz"
                };

                RenderSettings.Skybox.Generate(Application.GraphicsDevice, blueSkybox);
            }

            var grid = FindGameObjectByName("Grid");
            if (grid == null)
            {
                grid = new GameObject("Grid");
                grid.Tag = EditorGame.EditorTag;
                grid.AddComponent<Grid>();
            }

            _defaultMaterial = new StandardMaterial();
            _defaultMaterial.MainTexture = TextureFactory.CreateColor(Color.WhiteSmoke, 1, 1);
            _defaultMaterial.LoadContent(Application.Content);
        }

        internal void EnsureEditorBootstrapObjects()
        {
            var bootstrapObjects = new List<GameObject>();

            // Add a camera with a FPS controller
            if (FindObjectOfType<Camera>() == null)
            {
                var cameraGo = AddGameObject("Camera");
                var camera = cameraGo.GetComponent<Camera>();
                camera.Setup(new Vector3(0, 2, 5), Vector3.Forward, Vector3.Up);
                camera.Far = 10000;
                camera.AddComponent<EditorController>();
                bootstrapObjects.Add(cameraGo);
            }

            if (_lights.Count == 0)
            {
                var lightGo = AddGameObject("Directional");
                lightGo.Transform.LocalPosition = new Vector3(500, 500, 0);
                lightGo.Transform.LocalRotation = new Vector3(MathHelper.PiOver2, -MathHelper.PiOver4, 0);
                var directionalLight = lightGo.GetComponent<Light>();
                directionalLight.IsSun = true;

                var content = Application.Content;
                var glowTexture = content.Load<Texture2D>("Textures/Flares/SunGlow");
                var flareTextures = new Texture2D[]
                {
                    content.Load<Texture2D>("Textures/Flares/circle"),
                    content.Load<Texture2D>("Textures/Flares/circle_sharp_1"),
                    content.Load<Texture2D>("Textures/Flares/circle_soft_1")
                };

                var sunflares = directionalLight.AddComponent<LensFlare>();
                sunflares.Setup(glowTexture, flareTextures);
                bootstrapObjects.Add(lightGo);
            }

            if (bootstrapObjects.Count > 0)
                SceneInitialized?.Invoke(bootstrapObjects.ToArray());
        }

        public override void Update()
        {
            base.Update();

            if (_removeList.Count > 0)
            {
                foreach (var sceneObject in _removeList)
                    RemoveGameObject(sceneObject);

                _removeList.Clear();
            }

            if (_addList.Count > 0)
            {
                foreach (var type in _addList)
                    AddGameObject(type);

                _addList.Clear();
            }
        }

        #region Add

        public void AddComponent(string name)
        {
            var selected = EditorGame.Instance?.SelectedGameObject;
            if (selected == null)
                return;

            switch (name)
            {
                case "Camera":
                    if (selected.GetComponent<Camera>() == null)
                        selected.AddComponent<Camera>();
                    break;
                case "Directional":
                case "Point":
                case "Spot":
                    var light = selected.GetComponent<Light>() ?? selected.AddComponent<Light>();
                    light.Type = name == "Directional" ? LightType.Directional : name == "Point" ? LightType.Point : LightType.Spot;
                    break;
                case "BoxCollider":
                    if (selected.GetComponent<BoxCollider>() == null)
                        selected.AddComponent<BoxCollider>();
                    break;
                case "SphereCollider":
                    if (selected.GetComponent<SphereCollider>() == null)
                        selected.AddComponent<SphereCollider>();
                    break;
                case "Rigidbody":
                    if (selected.GetComponent<Rigidbody>() == null)
                        selected.AddComponent<Rigidbody>();
                    break;
                case "Terrain":
                    if (selected.GetComponent<Terrain>() == null)
                    {
                        var terrain = selected.AddComponent<Terrain>();
                        terrain.Flatten();
                        terrain.Renderer.Material = _defaultMaterial;
                    }
                    break;
            }
        }

        public GameObject AddGameObject(string type)
        {
            GameObject gameObject = null;

            switch (type)
            {
                case "Empty":
                    gameObject = new GameObject("GameObject");
                    break;
                case "Cube": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Cube, _defaultMaterial); break;
                case "Cylinder": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Cylinder, _defaultMaterial); break;
                case "Quad": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Quad, _defaultMaterial); break;
                case "Plane": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Plane, _defaultMaterial); break;
                case "Pyramid": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Pyramid, _defaultMaterial); break;
                case "Sphere": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Sphere, _defaultMaterial); break;
                case "Torus": gameObject = GameObject.CreatePrimitive(PrimitiveTypes.Torus, _defaultMaterial); break;

                case "Terrain":
                    gameObject = GameObjectFactory.CreateTerrain().GameObject;
                    var terrain = gameObject.GetComponent<Terrain>();
                    terrain.Flatten();
                    terrain.Renderer.Material = _defaultMaterial;
                    break;

                case "Water":
                    gameObject = GameObjectFactory.CreateWater(TextureFactory.CreateColor(Color.AliceBlue, 1, 1), TextureFactory.CreateNoise(32), Vector3.One);
                    break;

                case "Directional": gameObject = CreateLight(type, LightType.Directional); break;
                case "Point": gameObject = CreateLight(type, LightType.Point); break;
                case "Spot": gameObject = CreateLight(type, LightType.Spot); break;
                case "Reflection Probe":
                    gameObject = GameObjectFactory.CreateReflectionProbe(Vector3.Zero).GameObject;
                    break;

                case "Camera":
                    gameObject = GameObjectFactory.CreateCamera();
                    var cameraRenderer = gameObject.AddComponent<EditorIconRenderer>();
                    cameraRenderer.Setup("Camera_Icon");
                    cameraRenderer.AddComponent<SphereCollider>();
                    break;
                default: break;
            }

            gameObject.Name = type;

            AddGameObject(gameObject);

            return gameObject;
        }

        public void AddGameObject(GameObject sceneObject)
        {
            if (sceneObject == null)
                return;

            var collider = sceneObject.GetComponent<Collider>();
            if (collider != null)
                collider.IsPickable = true;

            Add(sceneObject, true);
        }

        private GameObject CreateLight(string name, LightType type)
        {
            var gameObject = new GameObject(name);
            gameObject.AddComponent<SphereCollider>();

            var light = gameObject.AddComponent<Light>();
            light.Type = type;

            var lightRenderer = light.AddComponent<EditorIconRenderer>();
            lightRenderer.Setup("Light_Icon");
            lightRenderer.AddComponent<SphereCollider>();
            return gameObject;
        }

        public void RemoveGameObject(GameObject sceneObject)
        {
            Remove(sceneObject);
        }

        #endregion

        public GameObject[] GetGameObjects()
        {
            var list = new List<GameObject>();

            foreach (var go in _gameObjects)
                if (go.Tag != EditorGame.EditorTag)
                    list.Add(go);

            return list.ToArray();
        }

        public GameObject[] GetRootGameObjects()
        {
            var list = new List<GameObject>();
            foreach (var go in _gameObjects)
            {
                if (go.Tag == EditorGame.EditorTag)
                    continue;

                var parent = go.Transform.Parent;
                if (parent == null || parent.GameObject is Scene || parent.GameObject.Tag == EditorGame.EditorTag)
                    list.Add(go);
            }

            return list.ToArray();
        }

        private GameObject FindGameObjectByName(string name)
        {
            foreach (var go in _gameObjects)
            {
                if (go.Name == name)
                    return go;
            }

            return null;
        }
    }
}
