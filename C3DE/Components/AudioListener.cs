namespace C3DE.Components
{
    using Microsoft.Xna.Framework;
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

    public class AudioListener : Component
    {
        private XNAAudioListener _audioListener;

        public XNAAudioListener Listener
        {
            get => _audioListener;
        }

        public AudioListener()
            : base()
        {
            _audioListener = new XNAAudioListener();
        }

        public override void Update()
        {
            base.Update();

            _audioListener.Forward = _transform.Forward;
            _audioListener.Position = _transform.LocalPosition;
            _audioListener.Up = Vector3.Up;
        }
    }
}
