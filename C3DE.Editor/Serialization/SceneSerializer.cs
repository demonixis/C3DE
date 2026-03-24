using System;
using System.Collections.Generic;
using System.IO;
using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Editor.Assets;
using C3DE.Graphics;
using C3DE.Graphics.Materials;
using C3DE.Graphics.PostProcessing;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace C3DE.Editor.Serialization
{
    public sealed class SceneSerializer
    {
        private static readonly string[] DefaultSkybox =
        {
            "Textures/Skybox/bluesky/px",
            "Textures/Skybox/bluesky/nx",
            "Textures/Skybox/bluesky/py",
            "Textures/Skybox/bluesky/ny",
            "Textures/Skybox/bluesky/pz",
            "Textures/Skybox/bluesky/nz"
        };

        public SceneData Capture(EditorScene scene)
        {
            var data = new SceneData
            {
                SceneGuid = Path.GetFileNameWithoutExtension(scene.Name) ?? Guid.NewGuid().ToString("N"),
                Name = scene.Name,
                RenderSettings = Capture(scene.RenderSettings)
            };

            foreach (var gameObject in scene.GetGameObjects())
            {
                var item = new GameObjectData
                {
                    Id = gameObject.Id,
                    Name = gameObject.Name,
                    Tag = gameObject.Tag,
                    Enabled = gameObject.Enabled,
                    IsStatic = gameObject.IsStatic,
                    ParentId = GetParentId(gameObject)
                };

                item.Components.Add(new ComponentData
                {
                    Type = nameof(Transform),
                    Data = new JObject
                    {
                        ["LocalPosition"] = JArray.FromObject(ToArray(gameObject.Transform.LocalPosition)),
                        ["LocalRotation"] = JArray.FromObject(ToArray(gameObject.Transform.LocalRotation)),
                        ["LocalScale"] = JArray.FromObject(ToArray(gameObject.Transform.LocalScale))
                    }
                });

                CaptureOptionalComponents(gameObject, item.Components);
                data.GameObjects.Add(item);
            }

            return data;
        }

        public void Save(EditorScene scene, string path)
        {
            var dto = Capture(scene);
            var json = JsonConvert.SerializeObject(dto, Formatting.Indented);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, json);
        }

        public EditorScene Load(string path)
        {
            var json = File.ReadAllText(path);
            var data = JsonConvert.DeserializeObject<SceneData>(json);
            return Build(data);
        }

        public EditorScene Build(SceneData data)
        {
            var scene = new EditorScene(false)
            {
                Name = string.IsNullOrWhiteSpace(data?.Name) ? "Scene" : data.Name
            };

            Apply(scene.RenderSettings, data?.RenderSettings);

            var map = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
            if (data?.GameObjects != null)
            {
                foreach (var goData in data.GameObjects)
                {
                    var go = new GameObject(goData.Name ?? "GameObject")
                    {
                        Id = string.IsNullOrWhiteSpace(goData.Id) ? Guid.NewGuid().ToString() : goData.Id,
                        Name = goData.Name ?? "GameObject",
                        Tag = goData.Tag,
                        IsStatic = goData.IsStatic,
                        Enabled = goData.Enabled
                    };

                    ApplyTransform(go.Transform, FindComponent(goData, nameof(Transform))?.Data);
                    ApplyComponents(scene, go, goData.Components);
                    scene.AddGameObject(go);
                    map[go.Id] = go;
                }

                foreach (var goData in data.GameObjects)
                {
                    if (string.IsNullOrWhiteSpace(goData.ParentId))
                        continue;

                    if (map.TryGetValue(goData.Id, out var child) && map.TryGetValue(goData.ParentId, out var parent))
                        child.Transform.Parent = parent.Transform;
                }
            }

            if (scene.LightCount == 0 && Scene.FindObjectOfType<Camera>() == null)
                scene.EnsureEditorBootstrapObjects();

            return scene;
        }

        private static void CaptureOptionalComponents(GameObject gameObject, List<ComponentData> components)
        {
            var camera = gameObject.GetComponent<Camera>();
            if (camera != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(Camera),
                    Data = new JObject
                    {
                        ["ClearColor"] = JArray.FromObject(ToArray(camera.ClearColor.ToVector3())),
                        ["Depth"] = camera.Depth,
                        ["FieldOfView"] = camera.FieldOfView,
                        ["Near"] = camera.Near,
                        ["Far"] = camera.Far,
                        ["ProjectionType"] = (int)camera.ProjectionType
                    }
                });
            }

            var light = gameObject.GetComponent<Light>();
            if (light != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(Light),
                    Data = new JObject
                    {
                        ["Type"] = (int)light.Type,
                        ["Color"] = JArray.FromObject(ToArray(light.Color.ToVector3())),
                        ["Intensity"] = light.Intensity,
                        ["Radius"] = light.Radius,
                        ["FallOf"] = light.FallOf,
                        ["Angle"] = light.Angle,
                        ["IsSun"] = light.IsSun,
                        ["ShadowEnabled"] = light.ShadowEnabled
                    }
                });
            }

            var meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                var materialColor = meshRenderer.Material?.DiffuseColor.ToVector3() ?? Color.White.ToVector3();
                components.Add(new ComponentData
                {
                    Type = nameof(MeshRenderer),
                    Data = new JObject
                    {
                        ["CastShadow"] = meshRenderer.CastShadow,
                        ["ReceiveShadow"] = meshRenderer.ReceiveShadow,
                        ["MeshType"] = meshRenderer.Mesh?.GetType().Name ?? string.Empty,
                        ["MaterialColor"] = JArray.FromObject(ToArray(materialColor))
                    }
                });
            }

            var terrain = gameObject.GetComponent<Terrain>();
            if (terrain != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(Terrain),
                    Data = new JObject
                    {
                        ["HeightmapSize"] = terrain.Geometry.HeightmapSize,
                        ["MeshSize"] = JArray.FromObject(ToArray(terrain.Geometry.Size)),
                        ["WeightData"] = new JObject
                        {
                            ["Sand"] = terrain.WeightData.SandLayer,
                            ["Ground"] = terrain.WeightData.GroundLayer,
                            ["Rock"] = terrain.WeightData.RockLayer,
                            ["Snow"] = terrain.WeightData.SnowLayer
                        },
                        ["Heights"] = terrain.Geometry.Data != null ? JArray.FromObject(Flatten(terrain.Geometry.Data)) : null
                    }
                });
            }

            var boxCollider = gameObject.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(BoxCollider),
                    Data = new JObject
                    {
                        ["Center"] = JArray.FromObject(ToArray(boxCollider.Center)),
                        ["Size"] = JArray.FromObject(ToArray(boxCollider.Size)),
                        ["IsPickable"] = boxCollider.IsPickable,
                        ["IsTrigger"] = boxCollider.IsTrigger
                    }
                });
            }

            var sphereCollider = gameObject.GetComponent<SphereCollider>();
            if (sphereCollider != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(SphereCollider),
                    Data = new JObject
                    {
                        ["Center"] = JArray.FromObject(ToArray(sphereCollider.Center)),
                        ["Radius"] = sphereCollider.Sphere.Radius,
                        ["IsPickable"] = sphereCollider.IsPickable,
                        ["IsTrigger"] = sphereCollider.IsTrigger
                    }
                });
            }

            var rigidbody = gameObject.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                components.Add(new ComponentData
                {
                    Type = nameof(Rigidbody),
                    Data = new JObject
                    {
                        ["IsStatic"] = rigidbody.IsStatic,
                        ["Gravity"] = rigidbody.Gravity,
                        ["Velocity"] = JArray.FromObject(ToArray(rigidbody.Velocity)),
                        ["AngularVelocity"] = JArray.FromObject(ToArray(rigidbody.AngularVelocity)),
                        ["IsKinematic"] = rigidbody.IsKinematic
                    }
                });
            }
        }

        private static void ApplyComponents(EditorScene scene, GameObject gameObject, List<ComponentData> components)
        {
            if (components == null)
                return;

            foreach (var componentData in components)
            {
                if (componentData.Type == nameof(Transform))
                    continue;

                switch (componentData.Type)
                {
                    case nameof(Camera):
                        ApplyCamera(gameObject.GetComponent<Camera>() ?? gameObject.AddComponent<Camera>(), componentData.Data);
                        break;
                    case nameof(Light):
                        ApplyLight(gameObject.GetComponent<Light>() ?? gameObject.AddComponent<Light>(), componentData.Data);
                        break;
                    case nameof(MeshRenderer):
                        ApplyMeshRenderer(gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>(), componentData.Data);
                        break;
                    case nameof(Terrain):
                        ApplyTerrain(gameObject.GetComponent<Terrain>() ?? gameObject.AddComponent<Terrain>(), componentData.Data);
                        break;
                    case nameof(BoxCollider):
                        ApplyBoxCollider(gameObject.GetComponent<BoxCollider>() ?? gameObject.AddComponent<BoxCollider>(), componentData.Data);
                        break;
                    case nameof(SphereCollider):
                        ApplySphereCollider(gameObject.GetComponent<SphereCollider>() ?? gameObject.AddComponent<SphereCollider>(), componentData.Data);
                        break;
                    case nameof(Rigidbody):
                        ApplyRigidbody(gameObject.GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>(), componentData.Data);
                        break;
                }
            }
        }

        private static void ApplyTransform(Transform transform, JObject data)
        {
            if (transform == null || data == null)
                return;

            transform.LocalPosition = ReadVector3(data["LocalPosition"]);
            transform.LocalRotation = ReadVector3(data["LocalRotation"]);
            transform.LocalScale = ReadVector3(data["LocalScale"], Vector3.One);
        }

        private static void ApplyCamera(Camera camera, JObject data)
        {
            camera.ClearColor = new Color(ReadVector3(data["ClearColor"], Color.Black.ToVector3()));
            camera.Depth = data.Value<short?>("Depth") ?? 0;
            camera.FieldOfView = data.Value<float?>("FieldOfView") ?? 60.0f;
            camera.Near = data.Value<float?>("Near") ?? 0.5f;
            camera.Far = data.Value<float?>("Far") ?? 1500.0f;
            camera.ProjectionType = (CameraProjectionType)(data.Value<int?>("ProjectionType") ?? 0);
        }

        private static void ApplyLight(Light light, JObject data)
        {
            light.Type = (LightType)(data.Value<int?>("Type") ?? 1);
            light.Color = new Color(ReadVector3(data["Color"], Color.White.ToVector3()));
            light.Intensity = data.Value<float?>("Intensity") ?? 1.0f;
            light.Radius = data.Value<float?>("Radius") ?? 25.0f;
            light.FallOf = data.Value<float?>("FallOf") ?? 2.0f;
            light.Angle = data.Value<float?>("Angle") ?? MathHelper.PiOver4;
            light.IsSun = data.Value<bool?>("IsSun") ?? false;
            light.ShadowEnabled = data.Value<bool?>("ShadowEnabled") ?? false;
        }

        private static void ApplyMeshRenderer(MeshRenderer renderer, JObject data)
        {
            renderer.CastShadow = data.Value<bool?>("CastShadow") ?? true;
            renderer.ReceiveShadow = data.Value<bool?>("ReceiveShadow") ?? true;
            renderer.Mesh = CreateMesh(data.Value<string>("MeshType"));
            renderer.Mesh?.Build();

            var material = CreateMaterial(ReadVector3(data["MaterialColor"], Color.White.ToVector3()));
            renderer.Material = material;
        }

        private static void ApplyTerrain(Terrain terrain, JObject data)
        {
            var meshSize = ReadVector3(data["MeshSize"], new Vector3(1.0f, 1.0f, 1.0f));
            terrain.Geometry.Size = meshSize;
            terrain.Geometry.HeightmapSize = data.Value<int?>("HeightmapSize") ?? terrain.Geometry.HeightmapSize;

            var heights = data["Heights"] as JArray;
            if (heights != null && terrain.Geometry.HeightmapSize > 0)
                terrain.Geometry.Data = Inflate(heights.ToObject<float[]>(), terrain.Geometry.HeightmapSize);

            var weightData = data["WeightData"] as JObject;
            if (weightData != null)
            {
                terrain.SetWeightData(
                    weightData.Value<float?>("Sand") ?? 9.0f,
                    weightData.Value<float?>("Ground") ?? 18.0f,
                    weightData.Value<float?>("Rock") ?? 23.0f,
                    weightData.Value<float?>("Snow") ?? 27.0f);
            }

            terrain.Build();
            terrain.Renderer.Material = CreateMaterial(Color.WhiteSmoke.ToVector3());
        }

        private static void ApplyBoxCollider(BoxCollider collider, JObject data)
        {
            collider.SetCenter(
                data["Center"]?[0]?.Value<float>(),
                data["Center"]?[1]?.Value<float>(),
                data["Center"]?[2]?.Value<float>());
            var size = ReadVector3(data["Size"]);
            collider.SetSize(size.X, size.Y, size.Z);
            collider.IsPickable = data.Value<bool?>("IsPickable") ?? true;
            collider.IsTrigger = data.Value<bool?>("IsTrigger") ?? false;
        }

        private static void ApplySphereCollider(SphereCollider collider, JObject data)
        {
            collider.SetCenter(
                data["Center"]?[0]?.Value<float>(),
                data["Center"]?[1]?.Value<float>(),
                data["Center"]?[2]?.Value<float>());
            var radius = data.Value<float?>("Radius") ?? 0.5f;
            collider.Sphere = new BoundingSphere(Vector3.Zero, radius);
            collider.IsPickable = data.Value<bool?>("IsPickable") ?? true;
            collider.IsTrigger = data.Value<bool?>("IsTrigger") ?? false;
        }

        private static void ApplyRigidbody(Rigidbody rigidbody, JObject data)
        {
            rigidbody.IsKinematic = data.Value<bool?>("IsKinematic") ?? false;
            rigidbody.Start();
            rigidbody.Gravity = data.Value<bool?>("Gravity") ?? true;
            rigidbody.Velocity = ReadVector3(data["Velocity"]);
            rigidbody.AngularVelocity = ReadVector3(data["AngularVelocity"]);
            rigidbody.IsStatic = data.Value<bool?>("IsStatic") ?? false;
        }

        private static ComponentData FindComponent(GameObjectData gameObject, string type)
        {
            if (gameObject?.Components == null)
                return null;

            for (var i = 0; i < gameObject.Components.Count; i++)
            {
                if (gameObject.Components[i].Type == type)
                    return gameObject.Components[i];
            }

            return null;
        }

        private static string GetParentId(GameObject gameObject)
        {
            var parent = gameObject.Transform.Parent;
            if (parent == null || parent.GameObject is Scene)
                return null;

            if (parent.GameObject.Tag == EditorGame.EditorTag)
                return null;

            return parent.GameObject.Id;
        }

        private static float[] ToArray(Vector3 value) => new[] { value.X, value.Y, value.Z };

        private static Vector3 ReadVector3(JToken token, Vector3? fallback = null)
        {
            var value = fallback ?? Vector3.Zero;
            if (!(token is JArray array) || array.Count < 3)
                return value;

            value.X = array[0].Value<float>();
            value.Y = array[1].Value<float>();
            value.Z = array[2].Value<float>();
            return value;
        }

        private static float[] Flatten(float[,] data)
        {
            var width = data.GetLength(0);
            var height = data.GetLength(1);
            var flattened = new float[width * height];
            var index = 0;

            for (var x = 0; x < width; x++)
                for (var y = 0; y < height; y++)
                    flattened[index++] = data[x, y];

            return flattened;
        }

        private static float[,] Inflate(float[] data, int size)
        {
            var result = new float[size, size];
            if (data == null)
                return result;

            var index = 0;
            for (var x = 0; x < size; x++)
            {
                for (var y = 0; y < size; y++)
                {
                    if (index < data.Length)
                        result[x, y] = data[index++];
                }
            }

            return result;
        }

        private static Mesh CreateMesh(string meshType)
        {
            switch (meshType)
            {
                case nameof(CubeMesh): return new CubeMesh();
                case nameof(CylinderMesh): return new CylinderMesh();
                case nameof(PlaneMesh): return new PlaneMesh();
                case nameof(PyramidMesh): return new PyramidMesh();
                case nameof(QuadMesh): return new QuadMesh();
                case nameof(SphereMesh): return new SphereMesh();
                case nameof(TorusMesh): return new TorusMesh();
                case nameof(TerrainMesh): return new TerrainMesh();
                default: return new CubeMesh();
            }
        }

        private static StandardMaterial CreateMaterial(Vector3 diffuse)
        {
            var material = new StandardMaterial
            {
                DiffuseColor = new Color(diffuse),
                MainTexture = TextureFactory.CreateColor(Color.WhiteSmoke, 1, 1)
            };

            material.LoadContent(Application.Content);
            return material;
        }

        private static RenderSettingsData Capture(RenderSettings settings)
        {
            return new RenderSettingsData
            {
                FogEnabled = settings.FogEnabled,
                FogMode = (int)settings.FogMode,
                FogDensity = settings.FogDensity,
                FogStart = settings.FogStart,
                FogEnd = settings.FogEnd,
                FogColor = ToArray(settings.FogColor.ToVector3()),
                AmbientColor = ToArray(settings.AmbientColor.ToVector3()),
                SkyboxEnabled = settings.Skybox.Enabled,
                PostProcessing = Capture(settings.PostProcessing)
            };
        }

        private static PostProcessingData Capture(PostProcessingSettings settings)
        {
            return new PostProcessingData
            {
                Enabled = settings.Enabled,
                DebugView = (int)settings.DebugView,
                TonemappingEnabled = settings.Tonemapping.Enabled,
                Exposure = settings.Tonemapping.Exposure,
                ColorAdjustmentsEnabled = settings.ColorAdjustments.Enabled,
                Contrast = settings.ColorAdjustments.Contrast,
                Saturation = settings.ColorAdjustments.Saturation,
                Temperature = settings.ColorAdjustments.Temperature,
                Tint = settings.ColorAdjustments.Tint,
                Lift = ToArray(settings.ColorAdjustments.Lift),
                Gamma = ToArray(settings.ColorAdjustments.Gamma),
                Gain = ToArray(settings.ColorAdjustments.Gain),
                BloomEnabled = settings.Bloom.Enabled,
                BloomThreshold = settings.Bloom.Threshold,
                BloomSoftKnee = settings.Bloom.SoftKnee,
                BloomIntensity = settings.Bloom.Intensity,
                BloomBlurSize = settings.Bloom.BlurSize,
                BloomBlurIterations = settings.Bloom.BlurIterations,
                BloomResolution = (int)settings.Bloom.Resolution,
                AmbientOcclusionEnabled = settings.AmbientOcclusion.Enabled,
                AmbientOcclusionIntensity = settings.AmbientOcclusion.Intensity,
                AmbientOcclusionRadius = settings.AmbientOcclusion.Radius,
                AmbientOcclusionBias = settings.AmbientOcclusion.Bias,
                AmbientOcclusionBlurSharpness = settings.AmbientOcclusion.BlurSharpness,
                SharpenEnabled = settings.Sharpen.Enabled,
                SharpenIntensity = settings.Sharpen.Intensity,
                AntiAliasingEnabled = settings.AntiAliasing.Enabled,
                FxaaSpanMax = settings.AntiAliasing.FxaaSpanMax,
                FxaaReduceMin = settings.AntiAliasing.FxaaReduceMin,
                FxaaReduceMul = settings.AntiAliasing.FxaaReduceMul,
                VignetteEnabled = settings.Vignette.Enabled,
                VignetteIntensity = settings.Vignette.Intensity,
                VignetteSmoothness = settings.Vignette.Smoothness,
                VignetteRoundness = settings.Vignette.Roundness
            };
        }

        private static void Apply(RenderSettings settings, RenderSettingsData data)
        {
            if (data == null)
            {
                settings.Skybox.Generate(Application.GraphicsDevice, DefaultSkybox);
                return;
            }

            settings.FogEnabled = data.FogEnabled;
            settings.FogMode = (FogMode)data.FogMode;
            settings.FogDensity = data.FogDensity;
            settings.FogStart = data.FogStart;
            settings.FogEnd = data.FogEnd;
            settings.FogColor = new Color(ReadVector3(JArray.FromObject(data.FogColor), Color.White.ToVector3()));
            settings.AmbientColor = new Color(ReadVector3(JArray.FromObject(data.AmbientColor), new Vector3(0.1f, 0.1f, 0.1f)));

            settings.Skybox.Generate(Application.GraphicsDevice, DefaultSkybox);
            settings.Skybox.Enabled = data.SkyboxEnabled;

            Apply(settings.PostProcessing, data.PostProcessing);
        }

        private static void Apply(PostProcessingSettings settings, PostProcessingData data)
        {
            if (data == null)
                return;

            settings.Enabled = data.Enabled;
            settings.DebugView = (PostProcessDebugView)data.DebugView;
            settings.Tonemapping.Enabled = data.TonemappingEnabled;
            settings.Tonemapping.Exposure = data.Exposure;
            settings.ColorAdjustments.Enabled = data.ColorAdjustmentsEnabled;
            settings.ColorAdjustments.Contrast = data.Contrast;
            settings.ColorAdjustments.Saturation = data.Saturation;
            settings.ColorAdjustments.Temperature = data.Temperature;
            settings.ColorAdjustments.Tint = data.Tint;
            settings.ColorAdjustments.Lift = new Vector3(data.Lift[0], data.Lift[1], data.Lift[2]);
            settings.ColorAdjustments.Gamma = new Vector3(data.Gamma[0], data.Gamma[1], data.Gamma[2]);
            settings.ColorAdjustments.Gain = new Vector3(data.Gain[0], data.Gain[1], data.Gain[2]);
            settings.Bloom.Enabled = data.BloomEnabled;
            settings.Bloom.Threshold = data.BloomThreshold;
            settings.Bloom.SoftKnee = data.BloomSoftKnee;
            settings.Bloom.Intensity = data.BloomIntensity;
            settings.Bloom.BlurSize = data.BloomBlurSize;
            settings.Bloom.BlurIterations = data.BloomBlurIterations;
            settings.Bloom.Resolution = (PostProcessBloomResolution)data.BloomResolution;
            settings.AmbientOcclusion.Enabled = data.AmbientOcclusionEnabled;
            settings.AmbientOcclusion.Intensity = data.AmbientOcclusionIntensity;
            settings.AmbientOcclusion.Radius = data.AmbientOcclusionRadius;
            settings.AmbientOcclusion.Bias = data.AmbientOcclusionBias;
            settings.AmbientOcclusion.BlurSharpness = data.AmbientOcclusionBlurSharpness;
            settings.Sharpen.Enabled = data.SharpenEnabled;
            settings.Sharpen.Intensity = data.SharpenIntensity;
            settings.AntiAliasing.Enabled = data.AntiAliasingEnabled;
            settings.AntiAliasing.FxaaSpanMax = data.FxaaSpanMax;
            settings.AntiAliasing.FxaaReduceMin = data.FxaaReduceMin;
            settings.AntiAliasing.FxaaReduceMul = data.FxaaReduceMul;
            settings.Vignette.Enabled = data.VignetteEnabled;
            settings.Vignette.Intensity = data.VignetteIntensity;
            settings.Vignette.Smoothness = data.VignetteSmoothness;
            settings.Vignette.Roundness = data.VignetteRoundness;
        }
    }
}
