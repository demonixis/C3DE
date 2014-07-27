using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            set { model = value; }
        }

        public ModelRenderer()
            : this(null)
        {
        }

        public ModelRenderer(SceneObject sceneObject)
            : base(sceneObject)
        {
        }

        public override BoundingSphere GetBoundingSphere()
        {
            boundingSphere = model.Meshes[0].BoundingSphere;
            boundingSphere.Radius += 0.1f;
            boundingSphere.Center = sceneObject.Transform.LocalPosition;
            boundingSphere.Transform(sceneObject.Transform.world);
            return boundingSphere;
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
