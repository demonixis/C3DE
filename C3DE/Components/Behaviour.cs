using C3DE.UI;

namespace C3DE.Components
{
    public class Behaviour : Component
    {
        protected Transform transform;

        public new bool Enabled
        {
            get { return enabled; }
            set
            {
                if (value != enabled)
                {
                    enabled = value;

                    if (enabled)
                        OnEnabled();
                    else
                        OnDisabled();
                }
            }
        }

        public Behaviour()
            : this(null)
        {
        }

        public Behaviour(SceneObject sceneObject)
            : base(sceneObject)
        {
        }

        public virtual void OnEnabled()
        {
        }

        public virtual void OnDisabled()
        {
        }

        public virtual void Awake()
        {
            transform = GetComponent<Transform>();
        }

        public virtual void Start()
        {
        }

        public virtual void OnGUI(GUI gui)
        {
        }
    }
}
