using C3DE.Components.Colliders;
using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace C3DE.Components.Renderers
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
    public class ModelRenderer : RenderableComponent
    {
        protected Model model;

        public Model Model
        {
            get { return model; }
            set 
            {
                if (value != model)
                {
                    model = value;
                    ComputeBoundingSphere();
                }
            }
        }

        public ModelRenderer()
            : base()
        {
        }

        public override void ComputeBoundingSphere()
        {
            if (model != null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);

                boundingSphere.Center = sceneObject.Transform.Position;
                boundingSphere.Transform(sceneObject.Transform.world);
                boundingSphere.Radius *= Math.Max(Math.Max(transform.LocalScale.X, transform.LocalScale.Y), transform.LocalScale.Z);

                UpdateColliders();
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.SetVertexBuffer(meshPart.VertexBuffer);
                    device.Indices = meshPart.IndexBuffer;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }
    }
}
