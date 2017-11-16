namespace C3DE.Components
{
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

    public class AudioListener : Component
    {
        private XNAAudioListener m_AudioListener;

        public XNAAudioListener Listener
        {
            get => m_AudioListener;
        }

        public AudioListener()
            : base()
        {
            m_AudioListener = new XNAAudioListener();
        }

        public override void Update()
        {
            base.Update();

            m_AudioListener.Forward = transform.Forward;
            m_AudioListener.Position = transform.LocalPosition;
            m_AudioListener.Up = transform.Up;
        }
    }
}
