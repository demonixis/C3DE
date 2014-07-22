using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Components
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
    public class ModelRenderer : Component
    {
        protected Model model;
        protected Texture2D mainTexture;
        protected List<Texture2D> textures;
        protected BoundingSphere boundingSphere;
        public bool CastShadow { get; set; }
        public bool RecieveShadow { get; set; }

        public Model Model
        {
            get { return model; }
            set { model = value; }
        }

        public Texture2D MainTexture
        {
            get { return mainTexture; }
            set
            {
                mainTexture = value;
                AddTexture(value);
            }
        }

        public List<Texture2D> Textures
        {
            get { return textures; }
        }

        public int TextureCount
        {
            get { return textures.Count; }
        }

        public ModelRenderer()
            : base()
        {
            CastShadow = true;
            RecieveShadow = true;
            textures = new List<Texture2D>(1); // Assuming we've one texture by defaut
        }

        public void DrawMesh(GraphicsDevice device)
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

        public BoundingSphere GetBoundingSphere()
        {
            boundingSphere = model.Meshes[0].BoundingSphere;
            boundingSphere.Radius += 0.1f;
            boundingSphere.Center = sceneObject.Transform.LocalPosition;
            boundingSphere.Transform(sceneObject.Transform.world);
            return boundingSphere;
        }

        public void AddTexture(Texture2D texture)
        {
            if (!textures.Contains(texture))
                textures.Add(texture);
        }

        public void RemoveTexture(Texture2D texture)
        {
            if (textures.Contains(texture))
                textures.Remove(texture);
        }
    }
}
