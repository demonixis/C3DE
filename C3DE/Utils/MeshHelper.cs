using C3DE.Components.Renderers;
using C3DE.Geometries;
using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Utils
{
    public class MeshHelper
    {
        public static MeshRenderer Merge(SceneObject firstSO, SceneObject secondSO, Material material = null)
        {
            MeshRenderer first = firstSO.GetComponent<MeshRenderer>();
            MeshRenderer second = secondSO.GetComponent<MeshRenderer>();

            Geometry geometry = new Geometry();

            int i = 0;
            int vertexCount = first.Geometry.Vertices.Length + second.Geometry.Vertices.Length;
            int indexCount = first.Geometry.Indices.Length + second.Geometry.Indices.Length;
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[vertexCount];
            ushort[] indices = new ushort[indexCount];

            for (i = 0; i < first.Geometry.Vertices.Length; i++)
            {
                vertices[i].Position = Vector3.Transform(first.Geometry.Vertices[i].Position, first.Transform.world);
                vertices[i].Normal = Vector3.TransformNormal(first.Geometry.Vertices[i].Normal, first.Transform.world);
                vertices[i].TextureCoordinate = first.Geometry.Vertices[i].TextureCoordinate;
            }

            for (i = first.Geometry.Vertices.Length; i < second.Geometry.Vertices.Length; i++)
            {
                vertices[i].Position = Vector3.Transform(second.Geometry.Vertices[i].Position, second.Transform.world);
                vertices[i].Normal = Vector3.TransformNormal(second.Geometry.Vertices[i].Normal, second.Transform.world);
                vertices[i].TextureCoordinate = second.Geometry.Vertices[i].TextureCoordinate;
            }

            for (i = 0; i < first.Geometry.Indices.Length; i++)
                indices[i] = first.Geometry.Indices[i];

            for (i = first.Geometry.Indices.Length; i < second.Geometry.Indices.Length; i++)
                indices[i] = second.Geometry.Indices[i];

            geometry.Vertices = vertices;
            geometry.Indices = indices;
            geometry.Generate();

            MeshRenderer renderer = new MeshRenderer();
            renderer.Geometry = geometry;
            renderer.MainMaterial = material != null ? material : first.MainMaterial;

            return renderer;
        }
    }
}
