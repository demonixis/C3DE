using C3DE.Components;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
