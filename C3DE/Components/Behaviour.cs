using C3DE.UI;
using System.Collections;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    [DataContract]
    public class Behaviour : Component
    {
        private CoroutineManager m_CoroutineManager = new CoroutineManager();

        public Behaviour() : base() { }

        public override void Update()
        {
            m_CoroutineManager.Update();
        }

        public virtual void OnGUI(GUI ui) { }

        public virtual void OnDestroy() { }

        public virtual void OnServerInitialized() { }

        public virtual void OnConnectedToServer() { }

        public virtual void OnCollisionEnter() { }

        public virtual void OnTriggerEnter() { }

        public void StartCoroutine(IEnumerator coroutine)
        {
            m_CoroutineManager.Start(coroutine);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            m_CoroutineManager.Stop(coroutine);
        }

        public void StopAllCoroutine()
        {
            m_CoroutineManager.StopAll();
        }
    }
}
