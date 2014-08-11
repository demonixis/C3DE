using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Prefabs
{
    public class ModelPrefab : Prefab
    {
        protected ModelRenderer renderer;
        protected BoxCollider collider;

        public ModelRenderer Renderer
        {
            get { return renderer; }
        }

        public BoxCollider Collider
        {
            get { return collider; }
        }

        public ModelPrefab(string name, Scene scene)
            : base(name, scene)
        {
            renderer = AddComponent<ModelRenderer>();
            collider = AddComponent<BoxCollider>();
        }

        public void LoadModel(string modelPath)
        {
            renderer.Model = Application.Content.Load<Model>(modelPath);

            Vector3 transformedPosition;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            Matrix[] bonesTransforms = new Matrix[renderer.Model.Bones.Count];
            
            renderer.Model.CopyAbsoluteBoneTransformsTo(bonesTransforms);

            foreach (ModelMesh mesh in renderer.Model.Meshes)
            {
                foreach (ModelMeshPart meshPart in mesh.MeshParts)
                {
                    // Vertex buffer parameters
                    int vertexStride = meshPart.VertexBuffer.VertexDeclaration.VertexStride;
                    int vertexBufferSize = meshPart.NumVertices * vertexStride;

                    // Get vertex data as float
                    float[] vertexData = new float[vertexBufferSize / sizeof(float)];
                    meshPart.VertexBuffer.GetData<float>(vertexData);

                    // Iterate through vertices (possibly) growing bounding box, all calculations are done in world space
                    for (int i = 0; i < vertexBufferSize / sizeof(float); i += vertexStride / sizeof(float))
                    {
                        transformedPosition = Vector3.Transform(new Vector3(vertexData[i], vertexData[i + 1], vertexData[i + 2]), bonesTransforms[mesh.ParentBone.Index] * transform.world);
                        min = Vector3.Min(min, transformedPosition);
                        max = Vector3.Max(max, transformedPosition);
                    }
                }
            }

            collider.Box = new BoundingBox(min, max);
        }
    }
}
