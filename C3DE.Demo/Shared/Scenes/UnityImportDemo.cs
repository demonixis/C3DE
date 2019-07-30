using C3DE.Components;
using C3DE.Components.Lighting;
using C3DE.Components.Physics;
using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Demonixis.UnityJSONSceneExporter;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace C3DE.Demo.Scenes
{
    public class UnityImportDemo : SimpleDemo
    {
        public bool PBR = false;

        public UnityImportDemo() : base("Unity Import")
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            var watch = new Stopwatch();
            watch.Start();

            var content = Application.Content;

            var materials = new Dictionary<string, Material>();
            materials.Add("Sci-fi_Walll_001_basecolor", CreateMaterial(content, 
                "Textures/pbr/Wall/Sci-fi_Walll_001_basecolor", 
                "Textures/pbr/Wall/Sci-fi_Walll_001_normal",
                "Textures/pbr/Wall/Sci-fi_Walll_001_roughness", 
                "Textures/pbr/Wall/Sci-fi_Walll_001_metallic",
                "Textures/pbr/Wall/Sci-fi_Walll_001_ambientOcclusion",
                null));

            materials.Add("Metal_Plate_015_basecolor", CreateMaterial(content,
                "Textures/pbr/Metal Plate/Metal_Plate_015_basecolor",
                "Textures/pbr/Metal Plate/Metal_Plate_015_normal",
                "Textures/pbr/Metal Plate/Metal_Plate_015_roughness",
                "Textures/pbr/Metal Plate/Metal_Plate_015_metallic",
                "Textures/pbr/Metal Plate/Metal_Plate_015_ambientOcclusion",
                null));

            materials.Add("Metal01_col", CreateMaterial(content,
                "Textures/pbr/Metal/Metal01_col",
                "Textures/pbr/Metal/Metal01_nrm",
                "Textures/pbr/Metal/Metal01_rgh",
                "Textures/pbr/Metal/Metal01_met", 
                null, 
                null));


            var json = File.ReadAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "map.json"));
            var data = JsonConvert.DeserializeObject<UGameObject[]>(json);

            var objects = new Dictionary<string, Transform>();

            foreach (var item in data)
            {
                var go = new GameObject(item.Name);
                go.Id = item.ID.ToString();
                go.Name = item.Name;
                go.IsStatic = item.IsStatic;

                var tr = go.Transform;
                tr.LocalPosition = ToVector3(item.LocalPosition);
                tr.LocalRotation = ToVector3(item.LocalRotation);
                tr.LocalScale = ToVector3(item.LocalScale);

                objects.Add(go.Id, tr);

                // Mesh
                if (item.Renderer.Name != null && item.Renderer.MeshFilter.IndexStart != null)
                {
                    var uRenderer = item.Renderer;
                    var uMesh = uRenderer.MeshFilter;
                    var subMeshCount = uMesh.SubMeshCount;

                    var vertices = new VertexPositionNormalTexture[uMesh.Positions.Length / 3];
                    var offset = 0;

                    for (var i = 0; i < uMesh.Positions.Length; i += 3)
                    {
                        vertices[offset] = new VertexPositionNormalTexture
                        {
                            Position = new Vector3
                            {
                                X = uMesh.Positions[i],
                                Y = uMesh.Positions[i + 1],
                                Z = uMesh.Positions[i + 2],
                            },
                            Normal = new Vector3
                            {
                                X = uMesh.Normals[i],
                                Y = uMesh.Normals[i + 1],
                                Z = uMesh.Normals[i + 2],
                            },
                            TextureCoordinate = new Vector2
                            {
                                X = uMesh.UVs[i],
                                Y = uMesh.UVs[i + 1],
                            }
                        };

                        offset++;
                    }

                    if (subMeshCount == 0)
                    {
                        var mesh = new Mesh();
                        mesh.Vertices = vertices;
                        mesh.Indices = ToUShort(uMesh.Indices[0]);
                        mesh.Build();

                        var renderer = go.AddComponent<MeshRenderer>();
                        renderer.Mesh = mesh;
                        renderer.Material = GetMaterial(uRenderer.Materials[0]);
                    }
                    else
                    {
                        for (var i = 0; i < subMeshCount; i++)
                        {
                            var part = new GameObject($"{item.Name}_Part_{i}");
                            part.Transform.Parent = go.Transform;
                            part.Transform.Dirty = true;

                            var partMesh = new Mesh();
                            partMesh.Vertices = vertices;
                            partMesh.Indices = ToUShort(uMesh.Indices[i]);
                            partMesh.Build();

                            var partRenderer = part.AddComponent<MeshRenderer>();
                            partRenderer.Mesh = partMesh;

                            Material material = null;
                            if (uRenderer.Materials.Length != subMeshCount)
                                material = GetMaterial(uRenderer.Materials[0]);
                            else
                                material = GetMaterial(uRenderer.Materials[i]);

                            partRenderer.Material = material;
                        }
                    }
                }

                // Reflection Probe
                if (item.ReflectionProbe.Resolution > 0)
                {
                    var probe = go.AddComponent<ReflectionProbe>();
                    probe.Mode = item.ReflectionProbe.IsBacked ? ReflectionProbe.RenderingMode.Backed : ReflectionProbe.RenderingMode.Realtime;
                    probe.NearClip = item.ReflectionProbe.ClipPlanes[0];
                    probe.FarClip = item.ReflectionProbe.ClipPlanes[1];
                    probe.Resolution = item.ReflectionProbe.Resolution;
                    probe.Enabled = item.ReflectionProbe.Enabled;
                }

                // Light
                if (item.Light.Color != null)
                {
                    var light = go.AddComponent<Light>();
                    light.Enabled = item.Light.Enabled;
                    light.Color = ToColor(item.Light.Color);
                    light.Angle = item.Light.Angle;
                    light.Intensity = item.Light.Intensity;
                    light.Radius = item.Light.Radius;
                    light.ShadowEnabled = item.Light.ShadowsEnabled;

                    if (item.Light.Type == 1)
                        light.Type = LightType.Point;
                    else if (item.Light.Type == 2)
                        light.Type = LightType.Spot;
                }
            }

            // Assign Parent
            foreach (var item in data)
            {
                if (item.Parent != 0)
                {
                    var obj = objects[item.ID.ToString()];
                    obj.Parent = objects[item.Parent.ToString()];
                    obj.Dirty = true;
                }
            }

            watch.Stop();

            Debug.Log($"Unity Scene imported in {watch.ElapsedMilliseconds}ms.");

            Material GetMaterial(UMaterial uMat)
            {
                if (!string.IsNullOrEmpty(uMat.MainTexture))
                {
                    if (materials.ContainsKey(uMat.MainTexture))
                        return materials[uMat.MainTexture];
                }

                return materials["Default"];
            }
        }

        public Material CreateMaterial(ContentManager content, string albedo, string normal, string roughness, string metallic, string ao, string emissive)
        {
            if (PBR)
            {
                var mat = new PBRMaterial
                {
                    MainTexture = content.Load<Texture2D>(albedo),
                    NormalMap = content.Load<Texture2D>(normal),
                    EmissiveMap = emissive != null ? content.Load<Texture2D>(emissive) : null
                };

                return mat;
            }

            return new StandardMaterial
            {
                MainTexture = content.Load<Texture2D>(albedo),
                NormalMap = content.Load<Texture2D>(normal),
                EmissiveMap = emissive != null ? content.Load<Texture2D>(emissive) : null,
                EmissiveIntensity = emissive != null ? 1 : 0
            };
        }

        public Vector2 ToVector2(float[] f) => new Vector2(f[0], f[1]);
        public Vector3 ToVector3(float[] f) => new Vector3(f[0], f[1], f[2]);
        public Vector4 ToVector4(float[] f) => new Vector4(f[0], f[1], f[2], f[3]);
        public Color ToColor(float[] f) => new Color(f[0], f[1], f[2], f[3]);

        public ushort[] ToUShort(int[] a)
        {
            var u = new ushort[a.Length];

            for (var i = 0; i < a.Length; i++)
                u[i] = (ushort)a[i];

            return u;
        }
    }
}
