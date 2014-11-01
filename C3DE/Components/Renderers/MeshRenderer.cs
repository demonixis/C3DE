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

                    geometry.ConstructionDone += OnGeometryConstructionDone;
                    _haveListener = true;
                }
            }
        }

        private void OnGeometryConstructionDone(object sender, EventArgs e)
        {
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

            boundingSphere = BoundingSphere.CreateFromPoints(geometry.GetVertices(VertexType.Position));
            boundingSphere.Center = sceneObject.Transform.Position;
      
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
