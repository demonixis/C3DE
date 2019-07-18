using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering
{
    public class MeshRenderer : Renderer
    {
        private bool _haveListener;
        protected internal Mesh _mesh;

        public Mesh Mesh
        {
            get => _mesh;
            set
            {
                if (value != _mesh && value != null)
                {
                    if (_mesh != null && _haveListener)
                    {
                        _mesh.ConstructionDone -= ComputeBoundingInfos;
                        _haveListener = false;
                    }

                    _mesh = value;

                    _mesh.ConstructionDone += ComputeBoundingInfos;
                    _haveListener = true;
                }
            }
        }

        public MeshRenderer()
            : base()
        {
        }

        public override void ComputeBoundingInfos()
        {
            if (_mesh == null)
                return;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var vertices = _mesh.GetVertices(VertexType.Position);

            for (int i = 0, l = vertices.Length; i < l; i++)
            {
                min.X = Math.Min(vertices[i].X, min.X);
                min.Y = Math.Min(vertices[i].Y, min.Y);
                min.Z = Math.Min(vertices[i].Z, min.Z);
                max.X = Math.Max(vertices[i].X, max.X);
                max.Y = Math.Max(vertices[i].Y, max.Y);
                max.Z = Math.Max(vertices[i].Z, max.Z);
            }

            boundingBox.Min = min;
            boundingBox.Max = max;

            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var dz = max.Z - min.Z;

            boundingSphere.Radius = (float)Math.Max(Math.Max(dx, dy), dz) / 2.0f;
            boundingSphere.Center = _transform.LocalPosition;

            UpdateColliders();
        }

        public override void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(_mesh.VertexBuffer);
            device.Indices = _mesh.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _mesh.IndexBuffer.IndexCount / 3);
        }

        DynamicVertexBuffer _instanceVertexBuffer;

        static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        public void DrawInstance(GraphicsDevice graphics, Matrix[] instances, ref Matrix view, ref Matrix projection)
        {
            if (instances.Length == 0)
                return;

            if ((_instanceVertexBuffer == null) || (instances.Length != _instanceVertexBuffer.VertexCount))
            {
                _instanceVertexBuffer?.Dispose();
                _instanceVertexBuffer = new DynamicVertexBuffer(graphics, instanceVertexDeclaration, instances.Length, BufferUsage.WriteOnly);
            }

            _instanceVertexBuffer.SetData(instances, 0, instances.Length, SetDataOptions.Discard);

            graphics.SetVertexBuffers(new VertexBufferBinding(_mesh.VertexBuffer, 0, 0), new VertexBufferBinding(_instanceVertexBuffer, 0, 1));
            graphics.Indices = _mesh.IndexBuffer;
        }

        public override void Dispose()
        {
            if (_mesh != null)
            {
                _mesh.Dispose();
                _mesh = null;
            }
        }

        public override void PostDeserialize()
        {
            if (_mesh != null && _mesh.Built)
                _mesh.Build();
        }

        public override object Clone()
        {
            var clone = (MeshRenderer)base.Clone();

            if (_mesh != null)
            {
                clone._mesh = (Mesh)Activator.CreateInstance(_mesh.GetType());
                clone._mesh.Size = _mesh.Size;
                clone._mesh.TextureRepeat = _mesh.TextureRepeat;

                if (_mesh.Built)
                    clone._mesh.Build();
            }

            return clone;
        }
    }
}
