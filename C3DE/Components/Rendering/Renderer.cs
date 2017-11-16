using C3DE.Components.Physics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.Serialization;

namespace C3DE.Components.Rendering
{
    /// <summary>
    /// An abstract class to define a renderable object.
    /// </summary>
    [DataContract]
    public abstract class Renderer : Component
    {
        [DataMember]
        internal protected BoundingSphere boundingSphere;
        [DataMember]
        internal protected BoundingBox boundingBox;
        [DataMember]
        internal protected int materialIndex = 0;
        [DataMember]
        internal protected Material material;

        /// <summary>
        /// Indicates whether the object can cast shadow. 
        /// </summary>
        [DataMember]
        public bool CastShadow { get; set; } = true;

        /// <summary>
        /// Indicates whether the object can receive shadow.
        /// </summary>
        [DataMember]
        public bool ReceiveShadow { get; set; } = true;

        public BoundingSphere BoundingSphere => boundingSphere;

        public BoundingBox BoundingBox =>  boundingBox;

        /// <summary>
        /// Gets the main material.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set { material = value; }
        }

        /// <summary>
        /// Create a renderable component which can cast and receive shadow. No material is assigned.
        /// </summary>
        public Renderer()
            : base()
        {
            boundingSphere = new BoundingSphere();
            boundingBox = new BoundingBox();
        }

        public override void Update()
        {
            if (!sceneObject.IsStatic)
                boundingSphere.Center = transform.LocalPosition;
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
        public abstract void ComputeBoundingInfos();

        /// <summary>
        /// Draw the content of the component.
        /// </summary>
        /// <param name="device"></param>
        public abstract void Draw(GraphicsDevice device);

        public override int CompareTo(object obj)
        {
            var renderer = obj as Renderer;
            var material = renderer != null ? renderer.Material : null;

            if (renderer == null)
                return 1;

            if (material == null || Material == null)
                return base.CompareTo(obj);

            if (Material.hasAlpha == material.hasAlpha)
                return 0;
            else if (Material.hasAlpha && !material.hasAlpha)
                return 1;
            else
                return -1;
        }
    }
}
