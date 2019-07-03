using C3DE.UI;
using System.Collections;
using System.Runtime.Serialization;

namespace C3DE.Components
{
    [DataContract]
    public class Behaviour : Component
    {
        private CoroutineManager _coroutineManager = new CoroutineManager();

        public Behaviour() : base() { }

        public override void Update()
        {
            _coroutineManager.Update();
        }

        public virtual void OnGUI(GUI ui) { }

        public virtual void OnDestroy() { }

        public virtual void OnServerInitialized() { }

        public virtual void OnConnectedToServer() { }

        public virtual void OnCollisionEnter() { }

        public virtual void OnTriggerEnter() { }

        public void StartCoroutine(IEnumerator coroutine)
        {
            _coroutineManager.Start(coroutine);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            _coroutineManager.Stop(coroutine);
        }

        public void StopAllCoroutine()
        {
            _coroutineManager.StopAll();
        }
    }
}
