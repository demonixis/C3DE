using C3DE.Geometries;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Renderers
{
    public class MeshRenderer : RenderableComponent
    {
        protected Geometry geometry;
        protected bool _dirty;

        public Geometry Geometry
        {
            get { return geometry; }
            set
            {
                if (value != geometry)
                {
                    geometry = value;
                }
            }
        }

        public MeshRenderer()
            : this(null)
        {
        }

        public MeshRenderer(SceneObject sceneObject)
            : base(sceneObject)
        {
            _dirty = true;
        }

        public void ComputeBoundingSphere()
        {
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
            boundingSphere.Center = sceneObject.Transform.LocalPosition;
            boundingSphere.Transform(sceneObject.Transform.world);
        }

        public override BoundingSphere GetBoundingSphere()
        {
            return boundingSphere;
        }

        public override void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(geometry.VertexBuffer);
            device.Indices = geometry.IndexBuffer;
            device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, geometry.Vertices.Length, 0, geometry.Indices.Length / 3);
        }
    }
}
