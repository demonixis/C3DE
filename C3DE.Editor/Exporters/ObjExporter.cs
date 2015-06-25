using C3DE.Components.Renderers;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using System.Text;

namespace C3DE.Editor.Exporters
{
    public sealed class OBJExporter
    {
        private static int StartIndex = 0;

        public static string ExportMesh(MeshRenderer meshRenderer)
        {
            var geometry = meshRenderer.Geometry;
            var vertices = geometry.GetVertices(VertexType.Position);
            var normals = geometry.GetVertices(VertexType.Normal);
            var uvs = geometry.GetUVs();
            var verticesCount = 0;
            var stringBuilder = new StringBuilder();

            foreach (Vector3 vertice in vertices)
            {
                Vector3 verticeVector = Vector3.Transform(vertice, meshRenderer.Transform.WorldMatrix);
                verticesCount++;
                stringBuilder.Append(string.Format("v {0} {1} {2}\n", verticeVector.X, verticeVector.Y, -verticeVector.Z));
            }
            stringBuilder.Append("\n");

            foreach (Vector3 normal in normals)
            {
                var normalVector = meshRenderer.Transform.Rotation * normal;
                stringBuilder.Append(string.Format("vn {0} {1} {2}\n", -normalVector.X, -normalVector.Y, normalVector.Z));
            }
            stringBuilder.Append("\n");

            foreach (Vector2 uv in uvs)
                stringBuilder.Append(string.Format("vt {0} {1}\n", uv.X, uv.Y));

            StartIndex += verticesCount;
            return stringBuilder.ToString();
        }
    }
}
