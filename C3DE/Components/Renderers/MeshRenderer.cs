using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Renderers
{
    public class MeshRenderer : Renderer
    {
        private bool _haveListener;
        protected Geometry geometry;

        public Geometry Geometry
        {
            get { return geometry; }
            set
            {
                if (value != geometry && value != null)
                {
                    if (geometry != null && _haveListener)
                    {
                        geometry.ConstructionDone -= ComputeBoundingInfos;
                        _haveListener = false;
                    }

                    geometry = value;

                    geometry.ConstructionDone += ComputeBoundingInfos;
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
            if (geometry == null)
                return;

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            var vertices = geometry.GetVertices(VertexType.Position);

            for (int i = 0, l = vertices.Length; i < l; i++)
            {
                min.X = Math.Min(vertices[i].X, min.X);
                min.Y = Math.Min(vertices[i].Y, min.Y);
                min.Z = Math.Min(vertices[i].Z, min.Z);
                max.X = Math.Max(vertices[i].X, max.X);
                max.Y = Math.Max(vertices[i].Y, max.Y);
                max.Z = Math.Max(vertices[i].Z, max.Z);
            }

            min *= transform.LocalScale;
            max *= transform.LocalScale;

            boundingBox = new BoundingBox(min, max);

            var mx = max.X - min.X;
            var my = max.Y - min.Y;
            var mz = max.Z - min.Z;
            boundingSphere.Radius = (float)Math.Max(Math.Max(mx, my), mz) / 2.0f;
            boundingSphere.Center = transform.Position;

            UpdateColliders();
        }

        public override void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(geometry.VertexBuffer);
            device.Indices = geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, geometry.Vertices.Length, 0, geometry.Indices.Length / 3);
        }

        public override void Dispose()
        {
            if (geometry != null)
            {
                geometry.Dispose();
                geometry = null;
            }
        }

        public override SerializedCollection Serialize()
        {
            var data = base.Serialize();
            data.IncreaseCapacity(1);
            data.Add("Geometry", geometry != null ? geometry.Serialize() : null);
            return data;
        }

        public override void Deserialize(SerializedCollection data)
        {
            base.Deserialize(data);

            var geoStr = data["Geometry"];
                     
            if (!string.IsNullOrEmpty(geoStr))
            {
                var tmp = geoStr.Split('_');
                geometry = Activator.CreateInstance(Type.GetType(tmp[0])) as Geometry;
                if (geometry != null)
                    geometry.Deserialize(geoStr);
            }
        }
    }
}
