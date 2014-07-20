using Microsoft.Xna.Framework.Content;

namespace C3DE.Components
{
    /// <summary>
    /// A component is a part of a scene object.
    /// </summary>
    public abstract class Component
    {
        protected bool enabled = true;
        protected uint order = 1;
        protected SceneObject sceneObject;

        /// <summary>
        /// Determine if the component is enabled of disabled.
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        /// <summary>
        /// Gets the scene object of this component.
        /// </summary>
        public SceneObject SceneObject
        {
            get { return sceneObject; }
            internal set { sceneObject = value; }
        }

        public uint Order
        {
            get { return order; }
            protected set { order = value; }
        }

        /// <summary>
        /// Create an empty component.
        /// </summary>
        public Component()
        {
        }

        public Component(SceneObject sceneObj)
            : this()
        {
            sceneObject = sceneObj;
        }

        public virtual void LoadContent(ContentManager content)
        {
        }

        /// <summary>
        /// Update the logic.
        /// </summary>
        public virtual void Update()
        {
        }

        public static int CompareTo(Component c1, Component c2)
        {
            return c1.CompareTo(c2);
        }

        public int CompareTo(object obj)
        {
            if (obj is Component)
                return CompareTo(obj as Component);
            else
                return -1;
        }
    }
}