using C3DE.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace C3DE.Components.Renderers
{
    /// <summary>
    /// An abstract class to define a renderable object.
    /// </summary>
    public abstract class RenderableComponent : Component
    {
        protected BoundingSphere boundingSphere; // FIXME
        protected internal List<int> materials;
        protected internal int materialCount;

        public bool CastShadow { get; set; }
        public bool RecieveShadow { get; set; }

        protected internal List<int> MaterialIndices
        {
            get { return materials; }
        }

        public Material Material
        {
            get { return materialCount > 0 ? sceneObject.Scene.Materials[materials[0]] : null; }
            set { AddMaterial(value); }
        }

        public int MaterialCount
        {
            get { return materialCount; }
        }

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
            protected set { boundingSphere = value; }
        }

        public RenderableComponent()
            : this(null)
        {
        }

        public RenderableComponent(SceneObject sceneObject)
            : base(sceneObject)
        {
            CastShadow = true;
            RecieveShadow = true;
            boundingSphere = new BoundingSphere();
            materials = new List<int>(1);
            materialCount = 0;
        }

        public void AddMaterial(Material material)
        {
            if (sceneObject.Scene == null)
                sceneObject.Scene = material.scene;

            var index = material.Index;

            if (materials.IndexOf(index) == -1)
            {
                materials.Add(material.Index);
                materialCount++;
            }
        }

        public void RemoveMaterial(Material material)
        {
            var index = material.Index;

            if (materials.IndexOf(index) > -1)
            {
                materials.Remove(index);
                materialCount--;
            }
        }

        public abstract BoundingSphere GetBoundingSphere();

        public abstract void Draw(GraphicsDevice device);
    }
}
