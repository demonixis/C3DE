using C3DE.Components;
using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using System;
using System.Text;

namespace C3DE.Editor.Exporters
{
    public sealed class STLExporter
    {
        public static float Precision = 100.0f;

        public static string MergeExport(MeshRenderer[] renderers)
        {
            var geometry = MeshMerger.Merge(renderers);
            var renderer = new MeshRenderer();
            renderer.Transform = new Transform();
            renderer.Geometry = geometry;
            return Export(renderer);
        }

        public static string Export(MeshRenderer renderer)
        {
            if (renderer != null && renderer.Geometry != null)
            {
                var geometry = renderer.Geometry;
                var positions = geometry.GetVertices(VertexType.Position);
                var normals = geometry.GetVertices(VertexType.Normal);
                var indices = geometry.Indices;
                var world = renderer.Transform.WorldMatrix;
                var stringBuilder = new StringBuilder();
                // Caches
                var v1 = Vector3.Zero;
                var v2 = Vector3.Zero;
                var v3 = Vector3.Zero;
                var normal = Vector3.Zero;

                for (int i = 0, l = indices.Length; i < l; i += 3)
                {
                    v1 = Vector3.Transform(positions[i], world);
                    v2 = Vector3.Transform(positions[i + 1], world);
                    v3 = Vector3.Transform(positions[i + 2], world);
                    normal = Vector3.TransformNormal(normals[i], world);

                    stringBuilder.AppendFormat("\tfacet normal {0} {1} {2}\n", RoundF(normal.X), RoundF(normal.Y), RoundF(normal.Z));
                    stringBuilder.Append("\t\touter loop\n");
                    stringBuilder.AppendFormat("\t\t\tvertex {0} {1} {2}\n", RoundF(v1.X), RoundF(v1.Y), RoundF(v1.Z));
                    stringBuilder.AppendFormat("\t\t\tvertex {0} {1} {2}\n", RoundF(v2.X), RoundF(v2.Y), RoundF(v2.Z));
                    stringBuilder.AppendFormat("\t\t\tvertex {0} {1} {2}\n", RoundF(v3.X), RoundF(v3.Y), RoundF(v3.Z));
                    stringBuilder.Append("\t\tendloop\n");
                    stringBuilder.Append("\tendfacet\n");
                }

                return stringBuilder.ToString();
            }

            return string.Empty;
        }

        public static float RoundF(float value)
        {
            return (float)Math.Round((double)value * Precision) / Precision;
        }
    }
}
