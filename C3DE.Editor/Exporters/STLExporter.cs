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

        public static string Export(SceneObject sceneObject)
        {
            var renderer = sceneObject.GetComponent<MeshRenderer>();
            if (renderer != null && renderer.Geometry != null)
            {
                var geometry = renderer.Geometry;
                var positions = geometry.GetVertices(VertexType.Position);
                var normals = geometry.GetVertices(VertexType.Normal);
                var indices = geometry.Indices;
                var world = sceneObject.Transform.WorldMatrix;
                var stringBuilder = new StringBuilder();

                for (int i = 0, l = indices.Length; i < l; i += 3)
                {
                    var v1 = Vector3.Transform(positions[i], world);
                    var v2 = Vector3.Transform(positions[i + 1], world);
                    var v3 = Vector3.Transform(positions[i + 2], world);
                    var normal = Vector3.TransformNormal(normals[i], world);

                    stringBuilder.Append("\tfacet normal " + RoundF(normal.X) + " " + RoundF(normal.Y) + " " + RoundF(normal.Z) + "\n");
                    stringBuilder.Append("\t\touter loop\n");
                    stringBuilder.Append("\t\t\tvertex " + RoundF(v1.X) + " " + RoundF(v1.Y) + " " + RoundF(v1.Z) + "\n");
                    stringBuilder.Append("\t\t\tvertex " + RoundF(v2.X) + " " + RoundF(v2.Y) + " " + RoundF(v2.Z) + "\n");
                    stringBuilder.Append("\t\t\tvertex " + RoundF(v3.X) + " " + RoundF(v3.Y) + " " + RoundF(v3.Z) + "\n");
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
