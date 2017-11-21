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

            m_AudioListener.Forward = m_Transform.Forward;
            m_AudioListener.Position = m_Transform.LocalPosition;
            m_AudioListener.Up = m_Transform.Up;
        }
    }
}
