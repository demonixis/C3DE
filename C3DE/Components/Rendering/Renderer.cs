using C3DE.Components.Physics;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace C3DE.Components.Rendering
{
    /// <summary>
    /// An abstract class to define a renderable object.
    /// </summary>
    public abstract class Renderer : Component
    {
        internal protected BoundingSphere boundingSphere;
        internal protected BoundingBox boundingBox;
        internal protected int materialIndex = 0;
        internal protected Material material;

        /// <summary>
        /// Indicates whether the object can cast shadow. 
        /// </summary>
        public bool CastShadow { get; set; } = true;

        /// <summary>
        /// Indicates whether the object can receive shadow.
        /// </summary>
        public bool ReceiveShadow { get; set; } = true;

        public BoundingSphere BoundingSphere => boundingSphere;

        public BoundingBox BoundingBox =>  boundingBox;

        public int RenderQueue { get; protected set; } = 1;

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
            if (!_gameObject.IsStatic)
                boundingSphere.Center = _transform.LocalPosition;
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

            if (Material._hasAlpha == material._hasAlpha)
                return 0;
            else if (Material._hasAlpha && !material._hasAlpha)
                return 1;
            else
                return -1;
        }
    }
}
