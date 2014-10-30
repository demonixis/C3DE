using C3DE.Components.Colliders;
using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Renderers
{
    public class MeshRenderer : RenderableComponent
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
                        geometry.ConstructionDone -= OnGeometryConstructionDone;
                        _haveListener = false;
                    }

                    geometry = value;

                    if (!geometry.Constructed)
                    {
                        geometry.ConstructionDone += OnGeometryConstructionDone;
                        _haveListener = true;
                    }
                }
            }
        }

        private void OnGeometryConstructionDone(object sender, EventArgs e)
        {
            if (geometry != null)
                geometry.ConstructionDone -= OnGeometryConstructionDone;

            _haveListener = false;

            ComputeBoundingSphere();
        }

        public MeshRenderer()
            : base()
        {
        }

        public override void ComputeBoundingSphere()
        {
            if (geometry == null)
                return;

            var box = new BoundingBox(new Vector3(float.MaxValue), new Vector3(float.MinValue));

            for (var i = 0; i < geometry.Vertices.Length; i++)
            {
                box.Min.X = Math.Min(box.Min.X, geometry.Vertices[i].Position.X);
                box.Min.Y = Math.Min(box.Min.Y, geometry.Vertices[i].Position.Y);
                box.Min.Z = Math.Min(box.Min.Z, geometry.Vertices[i].Position.Z);

                box.Max.X = Math.Max(box.Max.X, geometry.Vertices[i].Position.X);
                box.Max.Y = Math.Max(box.Max.Y, geometry.Vertices[i].Position.Y);
                box.Max.Z = Math.Max(box.Max.Z, geometry.Vertices[i].Position.Z);
            }

            var width = box.Max.X - box.Min.X;
            var height = box.Max.Y - box.Min.Y;
            var depth = box.Max.Z - box.Min.Z;

            boundingSphere = new BoundingSphere();
            boundingSphere.Radius = (int)Math.Max(Math.Max(width, height), depth);
            boundingSphere.Center = sceneObject.Transform.Position;
            boundingSphere.Transform(sceneObject.Transform.world);

            UpdateColliders();
        }

        public override void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(geometry.VertexBuffer);
            device.Indices = geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, geometry.Vertices.Length, 0, geometry.Indices.Length / 3);
        }
    }
}
