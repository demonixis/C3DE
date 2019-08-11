using C3DE.Components.Rendering;
using C3DE.Graphics.Primitives;
using C3DE.Graphics.Materials;
using C3DE;
using C3DE.Graphics;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework.Graphics
{
    public static class ModelExtensions
    {
        private static Dictionary<string, Material> MaterialsCache = new Dictionary<string, Material>();

        public static GameObject ToMeshRenderers(this Model model, bool pbrMaterial = false)
        {
            var scene = Scene.current;

            var boneTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            var gameObject = new GameObject("Model");
            scene.Add(gameObject);

            foreach (ModelMesh mesh in model.Meshes)
            {
                var meshPartIndex = 0;

                var parent = new GameObject(mesh.Name);
                scene.Add(parent);
                parent.Transform.Parent = gameObject.Transform;

                var matrix = boneTransforms[mesh.ParentBone.Index];
                matrix.Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 position);

                parent.Transform.LocalPosition = position;
                parent.Transform.LocalRotation = rotation.ToEuler();
                parent.Transform.LocalScale = scale;

                foreach (var part in mesh.MeshParts)
                {
                    var effect = (BasicEffect)part.Effect;
                    var material = TryGetMaterial(effect);

                    if (material == null)
                        material = CreateMaterial(effect, pbrMaterial);

                    var child = new GameObject($"{mesh.Name}_{meshPartIndex}");
                    scene.Add(child);
                    var renderer = child.AddComponent<MeshRenderer>();
                    renderer.Material = material;
                    renderer.CastShadow = true;
                    renderer.ReceiveShadow = true;

                    var vertexData = new VertexPositionNormalTexture[part.VertexBuffer.VertexCount];
                    part.VertexBuffer.GetData(vertexData);
                    var indexData = new ushort[part.IndexBuffer.IndexCount];
                    part.IndexBuffer.GetData(indexData);

                    var geometry = new Mesh();
                    geometry.Vertices = vertexData;
                    geometry.Indices = indexData;
                    geometry.Build();

                    renderer.Mesh = geometry;

                    child.Transform.Parent = parent.Transform;
                }
            }

            return gameObject;
        }

        private static Material CreateMaterial(BasicEffect effect, bool pbr)
        {
            Material material = null;

            if (pbr)
            {
                material = new PBRMaterial
                {
                    MainTexture = effect.Texture,
                    DiffuseColor = new Color(effect.DiffuseColor.X, effect.DiffuseColor.Y, effect.DiffuseColor.Z),
                };

                ((PBRMaterial)(material)).CreateRoughnessMetallicAO();
            }
            else
            {
                material = new StandardMaterial
                {
                    MainTexture = effect.Texture,
                    DiffuseColor = new Color(effect.DiffuseColor.X, effect.DiffuseColor.Y, effect.DiffuseColor.Z),
                    SpecularMap = TextureFactory.CreateColor(new Color(effect.SpecularColor.X, effect.SpecularColor.Y, effect.SpecularColor.Z), 1, 1),
                    SpecularPower = (int)effect.SpecularPower,
                    EmissiveColor = new Color(effect.EmissiveColor.X, effect.EmissiveColor.Y, effect.EmissiveColor.Z)
                };
            }

            if (!string.IsNullOrEmpty(effect?.Texture?.Name))
                MaterialsCache.Add(effect.Texture.Name, material);

            return material;
        }

        private static Material TryGetMaterial(BasicEffect effect)
        {
            var name = effect?.Texture?.Name;
            var hasValidName = !string.IsNullOrEmpty(name);

            if (hasValidName && MaterialsCache.ContainsKey(name))
            {
#if DEBUG
                Debug.Log($"Reusing an existing material: {name}");
#endif
                return MaterialsCache[name];
            }

            return null;
        }
    }
}
