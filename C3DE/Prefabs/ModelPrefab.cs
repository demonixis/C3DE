using C3DE.Components.Colliders;
using C3DE.Components.Renderers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Prefabs
{
    public class ModelPrefab : SceneObject
    {
        protected ModelRenderer renderer;
        protected Collider collider;

        public Model Model
        {
            get { return renderer.Model; }
        }

        public ModelRenderer Renderer
        {
            get { return renderer; }
        }

        public Collider Collider
        {
            get { return collider; }
        }

        public ModelPrefab(string name)
            : base(name)
        {
            renderer = AddComponent<ModelRenderer>();
            collider = AddComponent<SphereCollider>();
        }

        public void LoadModel(string modelPath)
        {
            renderer.Model = Application.Content.Load<Model>(modelPath);

            BoundingSphere sphere = new BoundingSphere();

            foreach (ModelMesh mesh in renderer.Model.Meshes)
               sphere = BoundingSphere.CreateMerged(sphere, mesh.BoundingSphere);

            sphere.Center = transform.Position;

            (collider as SphereCollider).Sphere = sphere;
        }
    }
}
