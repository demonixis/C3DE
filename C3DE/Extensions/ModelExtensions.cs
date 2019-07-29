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
        private static Dictionary<string, StandardMaterial> MaterialsCache = new Dictionary<string, StandardMaterial>();

        public static GameObject ToMeshRenderers(this Model model, Scene scene = null)
        {
            if (scene == null)
                scene = Scene.current;

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
                Vector3 position;
                Quaternion rotation;
                Vector3 scale;

                matrix.Decompose(out scale, out rotation, out position);

                parent.Transform.LocalPosition = position;
                parent.Transform.LocalRotation = rotation.ToEuler();
                parent.Transform.LocalScale = scale;

                foreach (var part in mesh.MeshParts)
                {
                    var effect = (BasicEffect)part.Effect;
                    var material = TryGetMaterial(effect);

                    if (material == null)
                    {
                        material = new StandardMaterial();
                        material.MainTexture = effect.Texture;
                        material.DiffuseColor = new Color(effect.DiffuseColor.X, effect.DiffuseColor.Y, effect.DiffuseColor.Z);
                        material.SpecularMap = TextureFactory.CreateColor(new Color(effect.SpecularColor.X, effect.SpecularColor.Y, effect.SpecularColor.Z), 1, 1);
                        material.SpecularPower = (int)effect.SpecularPower;
                        material.EmissiveColor = new Color(effect.EmissiveColor.X, effect.EmissiveColor.Y, effect.EmissiveColor.Z);

                        if (!string.IsNullOrEmpty(effect?.Texture?.Name))
                            MaterialsCache.Add(effect.Texture.Name, material);
                    }

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

        private static StandardMaterial TryGetMaterial(BasicEffect effect)
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
