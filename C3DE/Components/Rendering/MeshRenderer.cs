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
        private bool m_HaveListener;
        protected Mesh m_Geometry;

        [DataMember]
        public Mesh Geometry
        {
            get { return m_Geometry; }
            set
            {
                if (value != m_Geometry && value != null)
                {
                    if (m_Geometry != null && m_HaveListener)
                    {
                        m_Geometry.ConstructionDone -= ComputeBoundingInfos;
                        m_HaveListener = false;
                    }

                    m_Geometry = value;

                    m_Geometry.ConstructionDone += ComputeBoundingInfos;
                    m_HaveListener = true;
                }
            }
        }

        public MeshRenderer()
            : base()
        {
        }

        public override void ComputeBoundingInfos()
        {
            if (m_Geometry == null)
                return;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var vertices = m_Geometry.GetVertices(VertexType.Position);

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
            boundingSphere.Center = transform.LocalPosition;

            UpdateColliders();
        }

        public override void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(m_Geometry.VertexBuffer);
            device.Indices = m_Geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, m_Geometry.IndexBuffer.IndexCount / 3);
        }

        public override void Dispose()
        {
            if (m_Geometry != null)
            {
                m_Geometry.Dispose();
                m_Geometry = null;
            }
        }

        public override void PostDeserialize()
        {
            if (m_Geometry != null && m_Geometry.Built)
                m_Geometry.Build();
        }

        public override object Clone()
        {
            var clone = (MeshRenderer)base.Clone();

            if (m_Geometry != null)
            {
                clone.m_Geometry = (Mesh)Activator.CreateInstance(m_Geometry.GetType());
                clone.m_Geometry.Size = m_Geometry.Size;
                clone.m_Geometry.TextureRepeat = m_Geometry.TextureRepeat;

                if (m_Geometry.Built)
                    clone.m_Geometry.Build();
            }
              
            return clone;
        }
    }
}
