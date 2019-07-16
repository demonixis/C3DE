using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Graphics
{
    public static class MeshUtils
    {
        public static (VertexPositionNormalTexture[], uint[]) Merge(params (VertexPositionNormalTexture[], uint[])[] buffers)
        {
            var vertices = new List<VertexPositionNormalTexture>();
            var indices = new List<uint>();
            uint offset = 0;

            foreach (var buffer in buffers)
            {
                var indiceCount = buffer.Item2.Length;

                vertices.AddRange(buffer.Item1);

                for (var i = 0; i < indiceCount; i++)
                    indices.Add(buffer.Item2[i] + offset);

                offset += (uint)indiceCount;
            }

            return (vertices.ToArray(), indices.ToArray());
        }

        public static GameObject Optimize(GameObject gameObject)
        {
            var renderers = gameObject.Transform.GetComponentsInChildren<MeshRenderer>();

            if (renderers.Length < 2)
                return null;

            var materialRenderers = new Dictionary<Material, List<MeshRenderer>>();

            // 1. One merge per material
            foreach (var renderer in renderers)
            {
                if (!materialRenderers.ContainsKey(renderer.material))
                    materialRenderers.Add(renderer.material, new List<MeshRenderer>());

                materialRenderers[renderer.material].Add(renderer);
            }

            var clone = new GameObject($"{gameObject.Name}_Merged");

            GameObject subMesh = null;
            MeshRenderer meshRenderer = null;
            List<VertexPositionNormalTexture> vertices = new List<VertexPositionNormalTexture>();
            List<ushort> indices = new List<ushort>();
            Mesh meshFilder = null;
            ushort offset = 0;

            foreach (var keyValue in materialRenderers)
            {
                vertices.Clear();
                indices.Clear();
                offset = 0;

                foreach (var renderer in keyValue.Value)
                {
                    var mesh = renderer._mesh;
                    var indiceCount = (ushort)mesh.Indices.Length;

                    var vertexArray = mesh.Vertices;
                    var world = renderer._transform._worldMatrix;

                    for(var i = 0; i < vertexArray.Length; i++)
                        vertexArray[i].Position = Vector3.Transform(vertexArray[i].Position, world);

                    vertices.AddRange(mesh.Vertices);

                    for (var i = 0; i < indiceCount; i++)
                        indices.Add((ushort)(mesh.Indices[i] + offset));

                    offset += indiceCount;
                }

                subMesh = new GameObject($"{clone}_SubMesh");
                subMesh.Transform.Parent = clone.Transform;

                meshFilder = new Mesh();
                meshFilder.Vertices = vertices.ToArray();
                meshFilder.Indices = indices.ToArray();
                meshFilder.Build();

                meshRenderer = subMesh.AddComponent<MeshRenderer>();
                meshRenderer._mesh = meshFilder;
                meshRenderer.material = keyValue.Key;
            }

            return clone;
        }
    }
}
