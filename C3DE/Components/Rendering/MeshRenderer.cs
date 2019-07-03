using C3DE.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Components.Rendering
{
    [DataContract]
    public class MeshRenderer : Renderer
    {
        private bool _haveListener;
        protected Mesh _geometry;

        [DataMember]
        public Mesh Mesh
        {
            get => _geometry;
            set
            {
                if (value != _geometry && value != null)
                {
                    if (_geometry != null && _haveListener)
                    {
                        _geometry.ConstructionDone -= ComputeBoundingInfos;
                        _haveListener = false;
                    }

                    _geometry = value;

                    _geometry.ConstructionDone += ComputeBoundingInfos;
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
            if (_geometry == null)
                return;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var vertices = _geometry.GetVertices(VertexType.Position);

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
            device.SetVertexBuffer(_geometry.VertexBuffer);
            device.Indices = _geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _geometry.IndexBuffer.IndexCount / 3);
        }

        public override void Dispose()
        {
            if (_geometry != null)
            {
                _geometry.Dispose();
                _geometry = null;
            }
        }

        public override void PostDeserialize()
        {
            if (_geometry != null && _geometry.Built)
                _geometry.Build();
        }

        public override object Clone()
        {
            var clone = (MeshRenderer)base.Clone();

            if (_geometry != null)
            {
                clone._geometry = (Mesh)Activator.CreateInstance(_geometry.GetType());
                clone._geometry.Size = _geometry.Size;
                clone._geometry.TextureRepeat = _geometry.TextureRepeat;

                if (_geometry.Built)
                    clone._geometry.Build();
            }
              
            return clone;
        }
    }
}
