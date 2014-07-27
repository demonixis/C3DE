using C3DE.Components;

namespace C3DE.Prefabs
{
    public class ModelPrefab : SceneObject
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

        public ModelPrefab()
            : this(null)
        {
        }

        public ModelPrefab(string name)
            : base()
        {
            if (!string.IsNullOrEmpty(name))
                Name = name;

            renderer = AddComponent<ModelRenderer>();
            collider = AddComponent<BoxCollider>();
        }
    }
}
