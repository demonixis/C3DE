using C3DE.Components.Rendering;
using C3DE.Graphics.Materials;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
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

                    for (var i = 0; i < vertexArray.Length; i++)
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

        public static void ComputeNormals(Vector3[] positions, ushort[] _indices, out Vector3[] normals)
        {
            normals = new Vector3[positions.Length];

            for (int i = 0; i < _indices.Length / 3; i++)
            {
                int index1 = _indices[i * 3];
                int index2 = _indices[i * 3 + 1];
                int index3 = _indices[i * 3 + 2];

                // Select the face
                var side1 = positions[index1] - positions[index3];
                var side2 = positions[index1] - positions[index2];
                var normal = Vector3.Cross(side1, side2);

                normals[index1] += normal;
                normals[index2] += normal;
                normals[index3] += normal;
            }

            for (int i = 0; i < positions.Length; i++)
            {
                var normal = normals[i];
                var len = normal.Length();
                if (len > 0.0f)
                    normals[i] = normal / len;
                else
                    normals[i] = Vector3.Zero;
            }
        }

        // Adapted from MeshHelper.cs in the MonoGame repository
        // https://github.com/labnation/MonoGame/blob/master/MonoGame.Framework.Content.Pipeline/Graphics/MeshHelper.cs
        public static void CalculateTangentFrames(Vector3[] positions, ushort[] indices, Vector3[] normals, Vector2[] textureCoords, out Vector3[] tangents, out Vector3[] bitangents)
        {
            var numVerts = positions.Length;
            var numIndices = indices.Length;

            var tan1 = new Vector3[numVerts];
            var tan2 = new Vector3[numVerts];

            for (var index = 0; index < numIndices; index += 3)
            {
                var i1 = indices[index + 0];
                var i2 = indices[index + 1];
                var i3 = indices[index + 2];

                var w1 = textureCoords[i1];
                var w2 = textureCoords[i2];
                var w3 = textureCoords[i3];

                var s1 = w2.X - w1.X;
                var s2 = w3.X - w1.X;
                var t1 = w2.Y - w1.Y;
                var t2 = w3.Y - w1.Y;

                var denom = s1 * t2 - s2 * t1;
                if (Math.Abs(denom) < float.Epsilon)
                    continue;

                var r = 1.0f / denom;
                var v1 = positions[i1];
                var v2 = positions[i2];
                var v3 = positions[i3];

                var x1 = v2.X - v1.X;
                var x2 = v3.X - v1.X;
                var y1 = v2.Y - v1.Y;
                var y2 = v3.Y - v1.Y;
                var z1 = v2.Z - v1.Z;
                var z2 = v3.Z - v1.Z;

                var sdir = new Vector3()
                {
                    X = (t2 * x1 - t1 * x2) * r,
                    Y = (t2 * y1 - t1 * y2) * r,
                    Z = (t2 * z1 - t1 * z2) * r,
                };

                var tdir = new Vector3()
                {
                    X = (s1 * x2 - s2 * x1) * r,
                    Y = (s1 * y2 - s2 * y1) * r,
                    Z = (s1 * z2 - s2 * z1) * r,
                };

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }

            tangents = new Vector3[numVerts];
            bitangents = new Vector3[numVerts];

            for (var i = 0; i < numVerts; i++)
            {
                var n = normals[i];
                var t = tan1[i];

                if (t.LengthSquared() < float.Epsilon)
                {
                    t = Vector3.Cross(n, Vector3.UnitX);
                    if (t.LengthSquared() < float.Epsilon)
                        t = Vector3.Cross(n, Vector3.UnitY);

                    tangents[i] = Vector3.Normalize(t);
                    bitangents[i] = Vector3.Cross(n, tangents[i]);
                    continue;
                }

                var tangent = t - n * Vector3.Dot(n, t);
                tangent = Vector3.Normalize(tangent);
                tangents[i] = tangent;

                // Calculate handedness
                var w = (Vector3.Dot(Vector3.Cross(n, t), tan2[i]) < 0.0F) ? -1.0F : 1.0F;

                // Calculate the bitangent
                var bitangent = Vector3.Cross(n, tangent) * w;
                bitangents[i] = bitangent;
            }
        }
    }
}
