﻿using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering
{
    public class MeshRenderer : Renderer
    {
        private static VertexDeclaration instanceVertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 0),
            new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 1),
            new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 2),
            new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 3)
        );

        private bool _haveListener;
        protected internal Mesh _mesh;
        private DynamicVertexBuffer _instanceVertexBuffer;
        private Transform[] _transforms;
        private Matrix[] _instances;

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

        public override bool InstancedEnabled => (_transforms?.Length ?? 0) > 1;

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

        public override void Draw(GraphicsDevice graphics)
        {
            if (_mesh == null || _mesh?.VertexBuffer == null)
                return;

            if (_mesh.VertexBuffer.IsDisposed || _mesh.IndexBuffer.IsDisposed)
            {
                Debug.Log($"The Renderer: {GameObject.Name} is disposed but want to be drawn.");
                return;
            }

            var size = _instances?.Length ?? 0;

            if (size > 1)
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
            else
            {
                graphics.SetVertexBuffer(_mesh.VertexBuffer);
                graphics.Indices = _mesh.IndexBuffer;
                graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _mesh.IndexBuffer.IndexCount / 3);
            }
        }

        public void AddInstance(MeshRenderer renderer)
        {
            if (renderer._mesh != _mesh)
                return;

            var index = 0;

            if (_instances != null)
            {
                index = _instances.Length;
                var newSize = _instances.Length + 1;

                Array.Resize(ref _instances, newSize);
                Array.Resize(ref _transforms, newSize);
            }
            else
            {
                _instances = new Matrix[2];
                _transforms = new Transform[2];
                _transforms[0] = _transform;
                index = 1;
            }

            _transforms[index] = renderer._transform;

            renderer.Enabled = false;
        }

        public override void Dispose()
        {
            _mesh?.Dispose();
        }

        public override void PostDeserialize()
        {
            if (_mesh != null && _mesh.Built)
                _mesh.Build();
        }

        public override object Clone()
        {
            var clone = new MeshRenderer();

            if (_mesh != null)
            {
                clone._mesh = _mesh;
                clone.Material = Material;

                if (_mesh.Built)
                    clone._mesh.Build();

                //clone.ComputeBoundingInfos();
            }

            return clone;
        }
    }
}
