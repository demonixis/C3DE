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

        public bool UseBasicEffect { get; set; }

        public Model Model
        {
            get { return model; }
            set 
            {
                if (value != model)
                {
                    model = value;
                    ComputeBoundingInfos();
                }
            }
        }

        public ModelRenderer()
            : base()
        {
            UseBasicEffect = true;
        }

        public override void ComputeBoundingInfos()
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

        public void DrawWithBasicEffect(Camera camera, GraphicsDevice device)
        {
            Matrix[] bonesTransforms = new Matrix[model.Bones.Count];
            model.CopyAbsoluteBoneTransformsTo(bonesTransforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = bonesTransforms[mesh.ParentBone.Index] * transform.world;
                    effect.View = camera.view;
                    effect.Projection = camera.projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
    }
}
