using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Geometries
{
    public enum VertexType
    {
        Position = 0, Normal
    }

    public class Geometry : IDisposable
    {
        private VertexPositionNormalTexture[] _vertices;
        private ushort[] _indices;
        private VertexBuffer _vertexBuffer;
        private IndexBuffer _indexBuffer;
        private DynamicVertexBuffer _dVertexBuffer;
        private DynamicIndexBuffer _dIndexBuffer;
        private bool _built;
        protected Vector3 size = Vector3.One;
        protected Vector2 repeatTexture = Vector2.One;
        protected bool invertFaces = false;
        protected bool useDynamicBuffers = false;

        public VertexPositionNormalTexture[] Vertices
        {
            get { return _vertices; }
            internal protected set { _vertices = value; }
        }

        public ushort[] Indices
        {
            get { return _indices; }
            internal protected set { _indices = value; }
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

        public DynamicVertexBuffer DynamicVertexBuffer
        {
            get { return _dVertexBuffer; }
        }

        public DynamicIndexBuffer DynamicIndexBuffer
        {
            get { return _dIndexBuffer; }
        }

        public bool UseDynamicBuffers
        {
            get { return useDynamicBuffers; }
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

        public Action ConstructionDone = null;

        public void NotifyConstructionDone()
        {
            if (ConstructionDone != null)
                ConstructionDone();
        }

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
            if (useDynamicBuffers)
            {
                _vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(_vertices);

                _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
                _indexBuffer.SetData(_indices);
            }
            else
            {
                _vertexBuffer = new VertexBuffer(device, typeof(VertexPositionNormalTexture), _vertices.Length, BufferUsage.WriteOnly);
                _vertexBuffer.SetData(_vertices);

                _indexBuffer = new IndexBuffer(device, IndexElementSize.SixteenBits, _indices.Length, BufferUsage.WriteOnly);
                _indexBuffer.SetData(_indices);
            }
        }

        public void Generate()
        {
            Dispose();
            CreateGeometry();
            ApplyParameters();
            CreateBuffers(Application.GraphicsDevice);
            Built = true;
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

        public Vector3[] GetVertices(VertexType type)
        {
            int size = Vertices.Length;

            Vector3[] vertices = new Vector3[size];

            for (int i = 0; i < size ; i++)
            {
                if (type == VertexType.Normal)
                    vertices[i] = Vertices[i].Normal;
                else
                    vertices[i] = Vertices[i].Position;
            }

            return vertices;
        }

        public void Dispose()
        {
            if (Built)
            {
                if (useDynamicBuffers)
                {
                    _dVertexBuffer.Dispose();
                    _dIndexBuffer.Dispose();
                }
                else
                {
                    _vertexBuffer.Dispose();
                    _indexBuffer.Dispose();
                }
            }
        }
    }
}
