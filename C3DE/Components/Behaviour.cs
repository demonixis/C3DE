using C3DE.UI;

namespace C3DE.Components
{
    public class Behaviour : Component
    {
        public Behaviour() : base() { }

        public virtual void OnGUI(GUI gui) { }

        public virtual void OnDestroy() { }

        public virtual void OnServerInitialized() { }

        public virtual void OnConnectedToServer() { }
    }
}
