using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Serialization;

namespace C3DE.Components.Renderers
{
    /// <summary>
    /// A component used to render an XNA model.
    /// </summary>
    [DataContract]
    public class ModelRenderer : Renderer
    {
        protected Model model;
        protected Matrix[] boneTransforms;

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
                var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

                model.CopyAbsoluteBoneTransformsTo(boneTransforms);

                // For each mesh of the model
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (ModelMeshPart meshPart in mesh.MeshParts)
                    {
                        // Vertex buffer parameters
                        var vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                        var vertexBufferSize = meshPart.NumVertices * vertexStride;

                        // Get vertex data as float
                        var vertexData = new float[vertexBufferSize / sizeof(float)];
                        meshPart.VertexBuffer.GetData<float>(vertexData);

                        // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                        for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                        {
                            var transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), boneTransforms[mesh.ParentBone.Index] * transform.world);
                            min = Vector3.Min(min, transformedPosition);
                            max = Vector3.Max(max, transformedPosition);
                        }
                    }
                }

                boundingBox.Min = min;
                boundingBox.Max = max;
                
                var mx = max.X - min.X;
                var my = max.Y - min.Y;
                var mz = max.Z - min.Z;
                boundingSphere.Radius = (float)Math.Max(Math.Max(mx, my), mz) / 2.0f;
                boundingSphere.Center = transform.Position;

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
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * transform.world;
                    effect.View = camera.view;
                    effect.Projection = camera.projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }
    }
}
