using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
    public class ModelRenderer : Component
    {
        public Model Model;
        public Texture2D Texture;
        protected BoundingSphere boundingSphere;
        public bool CastShadow { get; set; }
        public bool RecieveShadow { get; set; } 

        public ModelRenderer()
            : base()
        {
            CastShadow = true;
            RecieveShadow = true;
        }

        public void DrawMesh(GraphicsDevice device)
        {
            foreach (ModelMesh mesh in Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.SetVertexBuffer(meshPart.VertexBuffer);
                    device.Indices = meshPart.IndexBuffer;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        public BoundingSphere GetBoundingSphere()
        {
            boundingSphere = Model.Meshes[0].BoundingSphere;
            boundingSphere.Radius += 0.1f;
            boundingSphere.Center = sceneObject.Transform.LocalPosition;
            boundingSphere.Transform(sceneObject.Transform.world);
            return boundingSphere;
        }
    }
}
