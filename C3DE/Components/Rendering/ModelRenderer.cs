using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Components.Rendering
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
    [DataContract]
    public class ModelRenderer : Renderer
    {
        protected Model model;
        protected Matrix[] boneTransforms;

        public bool DrawWithBasicEffect { get; set; } = false;

        public Model Model
        {
            get { return model; }
            set
            {
                if (value != model)
                {
                    model = value;
                    
                    if (model != null)
                    {
                        boneTransforms = new Matrix[model.Bones.Count];
                        model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                        ComputeBoundingInfos();
                    }
                }
            }
        }

        public ModelRenderer()
            : base()
        {
        }

        public override void ComputeBoundingInfos()
        {
            if (model != null)
            {
                foreach (ModelMesh mesh in model.Meshes)
                    boundingSphere = BoundingSphere.CreateMerged(boundingSphere, mesh.BoundingSphere);

                boundingSphere.Center = sceneObject.Transform.LocalPosition;
                boundingSphere.Transform(sceneObject.Transform.world);
                boundingSphere.Radius *= Math.Max(Math.Max(transform.LocalScale.X, transform.LocalScale.Y), transform.LocalScale.Z);

                UpdateColliders();
            }
        }

        public override void Draw(GraphicsDevice device)
        {
            if (DrawWithBasicEffect)
                DrawBasicEffect(Camera.Main, device);
            else
                DrawNative(device);
        }

        public void DrawNative(GraphicsDevice device)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    device.SetVertexBuffer(meshPart.VertexBuffer);
                    device.Indices = meshPart.IndexBuffer;
                    device.DrawIndexedPrimitives(PrimitiveType.TriangleList, meshPart.VertexOffset, meshPart.StartIndex, meshPart.PrimitiveCount);
                }
            }
        }

        public void DrawBasicEffect(Camera camera, GraphicsDevice device)
        {
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * transform.world;
                    effect.View = camera.view;
                    effect.Projection = camera.projection;
                    effect.EnableDefaultLighting();
                    effect.CurrentTechnique.Passes[0].Apply();
                }
                mesh.Draw();
            }
        }
    }
}
