using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Geometries
{
    public abstract class Geometry
    {
        private VertexPositionNormalTexture[] _vertices;
        private short[] _indices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        protected Vector3 size = Vector3.One;
        protected Vector2 repeatTexture = Vector2.One;
        protected bool invertFaces = false;
        
        protected bool constructed;

        public VertexPositionNormalTexture[] Vertices
        {
            get { return _vertices; }
            protected set { _vertices = value; }
        }

        public short[] Indices
        {
            get { return _indices; }
            protected set { _indices = value; }
        }

        public VertexBuffer VertexBuffer
        {
            get { return _vertexBuffer; }
            protected set { _vertexBuffer = value; }
        }

        public IndexBuffer IndexBuffer
        {
            get { return _indexBuffer; }
            protected set { _indexBuffer = value; }
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

        public bool Constructed
        {
            get { return constructed; }
        }

        protected abstract void CreateGeometry();

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

        public void Generate(GraphicsDevice device)
        {
            if (constructed)
            {
                _vertexBuffer.Dispose();
                _indexBuffer.Dispose();
            }

            CreateGeometry();
                
            ApplyParameters();
            CreateBuffers(device);
            constructed = true;
        }

        public void ComputeNormals()
        {
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
                _vertices[i].Normal.Normalize();
        }
    }
}
