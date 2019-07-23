using System;
using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    class InstancedMeshRenderer : Renderer
    {
        private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        private Mesh _mesh;
        private DynamicVertexBuffer _instanceVertexBuffer;
        private Transform[] _transforms;
        private Matrix[] _instances;

        public void AddInstance(MeshRenderer renderer)
        {
            if (_mesh == null)
                _mesh = renderer._mesh;
            else if (renderer._mesh != _mesh)
                return;

            var index = 0;

            if (_instances != null)
            {
                index = _instances.Length;
                Array.Resize(ref _instances, index + 1);
                Array.Resize(ref _transforms, index + 1);
            }
            else
            {
                _instances = new Matrix[2];
                _transforms = new Transform[2];
            }

            _transforms[index] = renderer._transform;
            renderer.Enabled = false;
        }

        public void AddInstances(MeshRenderer[] renderers)
        {
            if (_mesh == null)
                _mesh = renderers[0]._mesh;

            var size = renderers.Length;

            if (_instances == null)
            {
                _instances = new Matrix[size];
                _transforms = new Transform[size];
            }

            for(var i = 0; i < size; i++)
            {
                if (!_mesh.Equals(renderers[i]._mesh))
                    throw new Exception("This is not the same mesh");

                _transforms[i] = renderers[i]._transform;
                renderers[i].Enabled = false;
            }
        }

        public override void ComputeBoundingInfos()
        {
        }

        public override void Draw(GraphicsDevice graphics)
        {
            var size = _instances?.Length ?? 0;

            if (size > 0)
            {
                for (var i = 0; i < size; i++)
                    _instances[i] = _transforms[i]._worldMatrix;

                if ((_instanceVertexBuffer == null) || (_instances.Length != _instanceVertexBuffer.VertexCount))
                {
                    _instanceVertexBuffer?.Dispose();
                    _instanceVertexBuffer = new DynamicVertexBuffer(graphics, instanceVertexDeclaration, _instances.Length, BufferUsage.WriteOnly);
                }
                
                _instanceVertexBuffer.SetData(_instances, 0, _instances.Length, SetDataOptions.Discard);

                graphics.SetVertexBuffers(new VertexBufferBinding(_mesh.VertexBuffer, 0, 0), new VertexBufferBinding(_instanceVertexBuffer, 0, 1));
                graphics.Indices = _mesh.IndexBuffer;
                graphics.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, _mesh.Indices.Length / 3, _instances.Length);
            }
        }
    }
}
