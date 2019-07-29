using C3DE.Graphics.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Graphics.Primitives
{
    public enum PrimitiveTypes
    {
        Cube, Cylinder, Quad, Plane, Pyramid, Sphere, Torus
    }

    public enum VertexType
    {
        Position = 0, Normal
    }

    public class Mesh : IDisposable, ICloneable
    {
        protected VertexPositionNormalTexture[] _vertices;
        protected VertexPositionNormalTextureTB[] _verticesN;
        private ushort[] _indices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private bool _built;
        protected Vector3 size = Vector3.One;
        protected Vector2 repeatTexture = Vector2.One;
        protected bool invertFaces = false;

        public VertexPositionNormalTexture[] Vertices
        {
            get { return _vertices; }
            set { _vertices = value; }
        }

        public ushort[] Indices
        {
            get { return _indices; }
            set { _indices = value; }
        }

        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
            internal protected set { _vertexBuffer = value; }
        }

        public IndexBuffer IndexBuffer
        {
            get { return _indexBuffer; }
            internal protected set { _indexBuffer = value; }
        }

        public Vector3 Size
        {
            get { return size; }
            set { size = value; }
        }

        public Vector2 TextureRepeat
        {
            get { return repeatTexture; }
            set { repeatTexture = value; }
        }

        public bool Built
        {
            get { return _built; }
            internal protected set
            {
                _built = value;
                NotifyConstructionDone();
            }
        }

        public event Action ConstructionDone = null;

        public void NotifyConstructionDone() => ConstructionDone?.Invoke();

        protected virtual void CreateGeometry() { }

        protected virtual void ApplyParameters()
        {
            for (int i = 0, l = Vertices.Length; i < l; i++)
            {
                Vertices[i].Position *= size;
                Vertices[i].TextureCoordinate *= repeatTexture;
            }
        }

        protected virtual void CreateBuffers(GraphicsDevice device)
        {
            _vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(_vertices);

            _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_indices);
        }

        public void Build()
        {
            Dispose();
            CreateGeometry();
            ApplyParameters();
            CreateBuffers(Application.GraphicsDevice);
            Built = true;
        }

        public void ComputeNormals()
        {
            if (_vertices == null)
            {
                _vertices = new VertexPositionNormalTexture[_vertexBuffer.VertexCount];
                _vertexBuffer.GetData<VertexPositionNormalTexture>(_vertices);

                _indices = new ushort[_indexBuffer.IndexCount];
                _indexBuffer.GetData<ushort>(_indices);
            }

            for (int i = 0; i < _vertices.Length; i++)
                _vertices[i].Normal = Vector3.Zero;

            for (int i = 0; i < _indices.Length / 3; i++)
            {
                int index1 = _indices[i * 3];
                int index2 = _indices[i * 3 + 1];
                int index3 = _indices[i * 3 + 2];

                // Select the face
                Vector3 side1 = _vertices[index1].Position - _vertices[index3].Position;
                Vector3 side2 = _vertices[index1].Position - _vertices[index2].Position;
                Vector3 normal = Vector3.Cross(side1, side2);

                _vertices[index1].Normal += normal;
                _vertices[index2].Normal += normal;
                _vertices[index3].Normal += normal;
            }

            for (int i = 0; i < _vertices.Length; i++)
            {
                var normal = _vertices[i].Normal;
                var len = normal.Length();
                if (len > 0.0f)
                    _vertices[i].Normal = normal / len;
                else
                    _vertices[i].Normal = Vector3.Zero;
            }
        }

        public void ComputeNormalTangentBiNormal(GraphicsDevice graphics)
        {
            var size = _verticesN.Length;
            var positions = new Vector3[size];
            var uv = new Vector2[size];

            for (var i = 0; i < size; i++)
            {
                positions[i] = _verticesN[i].Position;
                uv[i] = _verticesN[i].UV;
            }

            MeshUtils.ComputeNormals(positions, _indices, out Vector3[] normals);
            MeshUtils.CalculateTangentFrames(positions, _indices, normals, uv, out Vector3[] tangents, out Vector3[] biNormals);

            for (var i = 0; i < size; i++)
            {
                _verticesN[i].Normal = normals[i];
                _verticesN[i].Tangent = tangents[i];
                _verticesN[i].BiNormal = biNormals[i];
            }

            _vertexBuffer?.Dispose();
            _vertexBuffer = new VertexBuffer(graphics, typeof(VertexPositionNormalTextureTB), _verticesN.Length, BufferUsage.None);

            _indexBuffer?.Dispose();
            _indexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.None);
        }

        public void SetVertices(VertexType type, Vector3[] vertices)
        {
            for (int i = 0, l = vertices.Length; i < l; i++)
            {
                if (type == VertexType.Normal)
                    Vertices[i].Normal = vertices[i];
                else
                    Vertices[i].Position = vertices[i];
            }
        }

        public Vector3[] GetVertices(VertexType type)
        {
            if (Vertices == null)
                PopulateVerticesFromVertexBuffer();

            var size = Vertices.Length;
            var vertices = new Vector3[size];

            for (int i = 0; i < size; i++)
            {
                if (type == VertexType.Normal)
                    vertices[i] = Vertices[i].Normal;
                else
                    vertices[i] = Vertices[i].Position;
            }

            return vertices;
        }

        public Vector2[] GetUVs()
        {
            if (Vertices == null)
                PopulateVerticesFromVertexBuffer();

            int size = Vertices.Length;

            Vector2[] uvs = new Vector2[Vertices.Length];

            for (int i = 0; i < size; i++)
                uvs[i] = Vertices[i].TextureCoordinate;

            return uvs;
        }

        public void SetSize(float? x, float? y, float? z)
        {
            if (x.HasValue)
                size.X = x.Value;

            if (y.HasValue)
                size.Y = y.Value;

            if (z.HasValue)
                size.Z = z.Value;
        }

        public void SetTextureRepeat(float? x, float? y)
        {
            if (x.HasValue)
                repeatTexture.X = x.Value;

            if (y.HasValue)
                repeatTexture.Y = y.Value;
        }

        protected void PopulateVerticesFromVertexBuffer()
        {
            Vertices = new VertexPositionNormalTexture[VertexBuffer.VertexCount];
            VertexBuffer.GetData<VertexPositionNormalTexture>(Vertices);

            Indices = new ushort[IndexBuffer.IndexCount];
            IndexBuffer.GetData<ushort>(Indices);
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }

        public object Clone()
        {
            var clone = (Mesh)MemberwiseClone();

            if (Built)
                Build();

            return clone;
        }

        public override bool Equals(object obj)
        {
            var mesh = obj as Mesh;

            if (mesh != null)
            {
                if (mesh == this)
                    return true;

                if (_vertices.Length == mesh._vertices.Length && _indices.Length == mesh._indices.Length)
                {
                    return _vertices[0].Equals(mesh._vertices) &&
                        _vertices[_vertices.Length - 1].Equals(mesh._vertices[_vertices.Length - 1]);
                }
            }

            return base.Equals(obj);
        }
    }
}
