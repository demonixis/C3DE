﻿using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts;
using C3DE.Demo.Scripts.Diagnostic;
using C3DE.Demo.Scripts.FPS;
using C3DE.Demo.Scripts.Lighting;
using C3DE.Demo.Scripts.Utils;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Rendering;
using C3DE.Utils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace C3DE.Demo.Scenes
{
    public class FPSDemo : BaseDemo
    {
        public static bool Instancing = false;
        public static bool DebugPhysics = false;

        public static readonly int[,] LevelGrid = new int[,]
        {
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1 },
            { 1, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1 },
            { 1, 0, 0, 0, 1, 1, 1, 1, 0, 1, 0, 1, 1, 0, 1, 1, 1, 0, 1, 1 },
            { 1, 1, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1, 0, 0, 1 },
            { 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 3, 1, 0, 0, 1, 1, 0, 1 },
            { 1, 0, 0, 0, 0, 0, 0, 3, 0, 1, 0, 1, 0, 1, 0, 1, 0, 0, 0, 1 },
            { 1, 0, 3, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 3, 0, 1 },
            { 1, 0, 0, 0, 1, 1, 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        };

        private ReflectionProbe _reflectionProbe;

        public FPSDemo() : base("First Person Shooter") { }

        public override void Initialize()
        {
            base.Initialize();

            var content = Application.Content;

            // And a light
            var lightContainer = new GameObject("LightContainer");
            var lightGo = GameObjectFactory.CreateLight(LightType.Directional, Color.White, 2.5f, 4096);
            lightGo.Transform.LocalPosition = new Vector3(10, 50, 0);
            lightGo.Transform.Parent = lightContainer.Transform;

            _directionalLight = lightGo.GetComponent<Light>();

            // Skybox
            RenderSettings.Skybox.Generate(Application.GraphicsDevice, DemoGame.StarsSkybox, 2000);

            // Reflection Probe
            _reflectionProbe = GameObjectFactory.CreateReflectionProbe(new Vector3(0, 25, 0));

            var player = new PlayerShooter();
            player.Start();

            // Ground
            var ground = GameObjectFactory.CreateTerrain();
            var terrain = ground.GetComponent<Terrain>();
            terrain.Geometry.Size = new Vector3(1);
            terrain.Geometry.Build();
            terrain.Flatten();
            terrain.Renderer.Material = CreateGroundMaterial(content);
            terrain.Renderer.ReceiveShadow = false;
            terrain.Renderer.CastShadow = false;
            terrain.Transform.LocalPosition = new Vector3(terrain.Width * 0.5f, 0, terrain.Depth * 0.5f);
            terrain.AddComponent<StatsDisplay>();

            terrain.AddComponent<BoxCollider>();
            var rb = terrain.AddComponent<Rigidbody>();
            rb.IsStatic = true;
            rb.AddComponent<RigidbodyRenderer>();

            if (DebugPhysics)
                rb.AddComponent<BoundingBoxRenderer>().LineColor = Color.Red;

            rb.IsKinematic = true;

            var cubeSize = 4;
            var startPosition = new Vector2(0, 0);
            BuildMap(content, player.Transform, cubeSize, startPosition, 2);

            var go = new GameObject("MobManager");
            var mobSpawner = go.AddComponent<MobSpawner>();
            mobSpawner.SetGrid(LevelGrid, cubeSize, startPosition, _reflectionProbe);

            // Planets
            var planetContainer = new GameObject("PlanetContainer");
            var autoRotation = planetContainer.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, 0.05f, 0);

            var material = new StandardMaterial();
            material.MainTexture = content.Load<Texture2D>($"Textures/Planets/2k_mercury");
            material.NormalMap = content.Load<Texture2D>($"Textures/Planets/2k_mercury_normal");

            var planet = GameObjectFactory.CreateMesh(GeometryType.Sphere, true, true, false);
            planet.GetComponent<Renderer>().Material = material;
            planet.Transform.LocalPosition = new Vector3(150, 60, 100);
            planet.Transform.LocalScale = new Vector3(60);
            planet.Transform.Parent = planetContainer.Transform;

            autoRotation = planet.AddComponent<AutoRotation>();
            autoRotation.Rotation = new Vector3(0, -0.1f, 0);

            // planet.AddComponent<PostProcessSwitcher>();
            //planet.AddComponent<RendererSwitcher>();
            planet.AddComponent<DeferredDebuger>();

            // Bloom
            /* var bloom = new FastBloom(Application.GraphicsDevice);
             bloom.blurIterations = 8;
             bloom.blurType = FastBloom.BlurType.Sgx;
             bloom.resolution = FastBloom.Resolution.High;
             SetPostProcess(bloom, true);*/
        }

        public override void Update()
        {
            base.Update();

            if (Input.Keys.JustPressed(Keys.F1))
                Application.Engine.Renderer.SetVREnabled(true);
            else if (Input.Keys.JustPressed(Keys.F2))
                Application.Engine.Renderer.SetVREnabled(false);
        }

        private void BuildMap(ContentManager content, Transform player, int cubeSize, Vector2 startPosition, int lightModulo)
        {
            Rigidbody rb;
            GameObject wall;
            MeshRenderer renderer;
            MeshRenderer instancedWall = null;
            Material material;
            CubeMesh cubeMesh = new CubeMesh();
            cubeMesh.Size = new Vector3(cubeSize);
            cubeMesh.Build();

            var wallMaterial = CreateWallMaterial(content);
            var counter = 0;
            var inc = cubeSize;
            var sizeX = LevelGrid.GetLength(0);
            var sizeY = LevelGrid.GetLength(1);

            var mesh = new SphereMesh(0.1f, 16);
            mesh.Build();

            var matCount = ValidColors.Length;
            var materials = new UnlitMaterial[matCount];
            for (var i = 0; i < matCount; i++)
                materials[i] = new UnlitMaterial { DiffuseColor = ValidColors[i] };

            var lightMats = new Dictionary<Material, List<MeshRenderer>>();

            for (var x = 0; x < sizeX; x++)
            {
                for (var y = 0; y < sizeY; y++)
                {
                    var posX = (x * inc) + startPosition.X;
                    var posY = (y * inc) + startPosition.Y;

                    // Ground.
                    if (LevelGrid[x, y] == 0)
                    {
                        if (counter++ % lightModulo == 0)
                        {
                            material = materials[RandomHelper.Range(0, materials.Length)];
                            var light = SpawnLight(new Vector3(posX, 3, posY), material.DiffuseColor, 2, 0.5f, false, mesh, material);
                            //light.AddComponent<SinIntensity>();
                            var sin = light.AddComponent<SinMovement>();
                            sin.Min = 2;
                            sin.Max = 2;

                            renderer = light.GetComponent<MeshRenderer>();

                            if (!lightMats.ContainsKey(material))
                                lightMats.Add(material, new List<MeshRenderer>());

                            lightMats[material].Add(renderer);
                        }

                        continue;
                    }
                    // Player.
                    if (LevelGrid[x, y] == 2)
                    {
                        player.Position = new Vector3(posX, 0, posY);
                        continue;
                    }

                    // Wall.
                    wall = GameObjectFactory.CreateMesh(GeometryType.Cube);
                    wall.Transform.Position = new Vector3(posX, (float)cubeSize / 2.0f, posY);
                    wall.Transform.UpdateWorldMatrix();
                    wall.IsStatic = true;

                    renderer = wall.GetComponent<MeshRenderer>();
                    renderer.Mesh = cubeMesh;
                    renderer.ComputeBoundingInfos();
                    renderer.Material = wallMaterial;

                    rb = wall.AddComponent<Rigidbody>();
                    rb.IsStatic = true;
                    rb.AddComponent<BoxCollider>();

                    /*   if (DebugPhysics)
                           rb.AddComponent<BoundingBoxRenderer>();*/

                    /* if (Instancing)
                     {
                         if (instancedWall == null)
                         {
                             instancedWall = renderer;
                         }
                         else
                         {
                             instancedWall.AddInstance(renderer);
                             renderer.Enabled = false;
                         }
                     }*/
                }
            }

            /*     if (Instancing)
                 {
                     foreach (var keyValue in lightMats)
                     {
                         var renderers = keyValue.Value;

                         if (renderers.Count > 1)
                         {
                             for (var i = 1; i < renderers.Count; i++)
                                 renderers[0].AddInstance(renderers[i]);
                         }
                     }
                 }*/
        }

        protected override void SceneSetup()
        {
        }

        private Material CreateWallMaterial(ContentManager content)
        {
            /*  if (PreferePBRMaterials)
              {
                  var wallMatPBR = new PBRMaterial()
                  {
                      MainTexture = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_basecolor"),
                      NormalMap = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_normal"),
                  };

                  wallMatPBR.CreateRoughnessMetallicAO(
                      content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_metallic"),
                      content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_roughness"),
                      content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_ambientOcclusion"));

                  return wallMatPBR;
              }*/

            return new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Wall/Sci-fi_Walll_001_normal"),
                SpecularColor = Color.LightGray,
                SpecularPower = 5,
                SpecularIntensity = 1,
                ReflectionIntensity = 0.45f,
                ReflectionMap = _reflectionProbe.ReflectionMap
            };
        }

        private Material CreateGroundMaterial(ContentManager content)
        {
            /*   if (PreferePBRMaterials)
               {
                   var mat = new PBRMaterial()
                   {
                       MainTexture = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_basecolor"),
                       NormalMap = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_normal"),
                       Tiling = new Vector2(16)
                   };

                   mat.CreateRoughnessMetallicAO(
                       content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_roughness"),
                       content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_metallic"),
                       content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_ambientOcclusion"));

                   return mat;
               }*/

            return new StandardMaterial()
            {
                MainTexture = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_basecolor"),
                NormalMap = content.Load<Texture2D>("Textures/pbr/Metal Plate/Metal_Plate_015_normal"),
                SpecularColor = Color.LightGray,
                SpecularPower = 2,
                ReflectionIntensity = 0.85f,
                ReflectionMap = _reflectionProbe.ReflectionMap,
                Tiling = new Vector2(16)
            };
        }
    }
}
