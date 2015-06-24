using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace C3DE.Editor.Exporters
{
    public sealed class MeshMerger
    {
        public static Geometry Merge(MeshRenderer[] renderers)
        {
            var positions = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var indices = new List<ushort>();

            Geometry current = null;
            Vector3[] p = null;
            int j = 0;
            int k = 0;

            for (int i = 0, l = renderers.Length; i < l; i++)
            {
                current = renderers[i].Geometry;

                p = current.GetVertices(VertexType.Position);
                for (j = 0, k = p.Length; j < k; j++)
                    p[j] = Vector3.Transform(p[j], renderers[i].Transform.WorldMatrix);

                positions.AddRange(p);

                p = current.GetVertices(VertexType.Normal);
                for (j = 0, k = p.Length; j < k; j++)
                    p[j] = Vector3.TransformNormal(p[j], renderers[i].Transform.WorldMatrix);
                
                normals.AddRange(p);

                uvs.AddRange(current.GetUVs());
                indices.AddRange(current.Indices);
            }

            var size = positions.Count;
            current = new Geometry();
            current.Vertices = new VertexPositionNormalTexture[size];
            
            for (int i = 0; i < size; i++)
            {
                current.Vertices[i].Position = positions[i];
                current.Vertices[i].Normal = normals[i];
                current.Vertices[i].TextureCoordinate = uvs[i];
            }

            current.Indices = indices.ToArray();

            return current;
        }
    }
}
