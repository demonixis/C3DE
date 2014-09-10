using C3DE.Components.Colliders;
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
        protected internal BoundingSphere boundingSphere;
        protected internal List<int> materials;
        protected internal int materialCount;
        protected int materialIndex;

        /// <summary>
        /// Indicates whether the object can cast shadow. 
        /// </summary>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Indicates whether the object can receive shadow.
        /// </summary>
        public bool ReceiveShadow { get; set; }

        protected internal List<int> MaterialIndices
        {
            get { return materials; }
        }

        public Material MainMaterial
        {
            get { return Material; }
            set
            {
                var index = AddMaterial(value);

                if (index > -1 && index < materials.Count)
                    materialIndex = index;
            }
        }

        /// <summary>
        /// Gets the main material.
        /// </summary>
        public Material Material
        {
            get { return materialCount > 0 ? sceneObject.Scene.Materials[materials[materialIndex]] : null; }
            set { AddMaterial(value); }
        }

        /// <summary>
        /// Gets the number of materials.
        /// </summary>
        public int MaterialCount
        {
            get { return materialCount; }
        }

        /// <summary>
        /// Create a renderable component which can cast and receive shadow. No material is assigned.
        /// </summary>
        public RenderableComponent()
            : base()
        {
            CastShadow = true;
            ReceiveShadow = true;
            boundingSphere = new BoundingSphere();
            materials = new List<int>(1);
            materialCount = 0;
            materialIndex = 0;
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
                boundingSphere.Center = transform.Position;
        }

        /// <summary>
        /// Add a material, if the component have not a material it is used by default.
        /// If the material is already added it is used by default.
        /// </summary>
        /// <param name="material"></param>
        public int AddMaterial(Material material)
        {
            if (sceneObject.Scene == null)
                sceneObject.Scene = material.scene;

            var index = material.Index;
            var matIndex = materials.IndexOf(index);

            if (matIndex == -1)
            {
                materials.Add(material.Index);
                matIndex = materialCount++;
            }

            return matIndex;
        }

        /// <summary>
        /// Remove a material.
        /// </summary>
        /// <param name="material">The material to remove.</param>
        public void RemoveMaterial(Material material)
        {
            var index = material.Index;

            if (materials.IndexOf(index) > -1)
            {
                materials.Remove(index);
                materialCount--;
            }
        }

        /// <summary>
        /// Compute colliders size.
        /// </summary>
        protected void UpdateColliders()
        {
            var colliders = GetComponents<Collider>();
            var size = colliders.Length;

            if (size > 0)
            {
                for (var i = 0; i < size; i++)
                    colliders[i].Compute();
            }
        }

        /// <summary>
        /// Compute the internal bouding sphere of the collider, it's required by the shadow generator.
        /// </summary>
        protected abstract void ComputeBoundingSphere();

        /// <summary>
        /// Draw the content of the component.
        /// </summary>
        /// <param name="device"></param>
        public abstract void Draw(GraphicsDevice device);
    }
}
