using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace C3DE.Components.Rendering
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
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

                boundingSphere.Center = _gameObject.Transform.LocalPosition;
                boundingSphere.Transform(_gameObject.Transform._worldMatrix);
                boundingSphere.Radius *= Math.Max(Math.Max(_transform.LocalScale.X, _transform.LocalScale.Y), _transform.LocalScale.Z);

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
                    effect.World = boneTransforms[mesh.ParentBone.Index] * _transform._worldMatrix;
                    effect.View = camera._viewMatrix;
                    effect.Projection = camera._projectionMatrix;
                    effect.EnableDefaultLighting();
                    effect.CurrentTechnique.Passes[0].Apply();
                }
                mesh.Draw();
            }
        }
    }
}
