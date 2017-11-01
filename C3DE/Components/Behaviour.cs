using C3DE.UI;
using System.Collections;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    [DataContract]
    public class Behaviour : Component
    {
        public Behaviour() : base() { }

        public virtual void OnGUI(GUI ui) { }

        public virtual void OnDestroy() { }

        public virtual void OnServerInitialized() { }

        public virtual void OnConnectedToServer() { }

        public virtual void OnCollisionEnter() { }

        public virtual void OnTriggerEnter() { }

        public void StartCoroutine(IEnumerator coroutine)
        {
            Application.CoroutineManager.Start(coroutine);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            Application.CoroutineManager.Stop(coroutine);
        }

        public void StopAllCoroutine()
        {
            Application.CoroutineManager.StopAll();
        }
    }
}
