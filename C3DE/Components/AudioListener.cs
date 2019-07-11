namespace C3DE.Components
{
    using Microsoft.Xna.Framework;
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

    public class AudioListener : Component
    {
        public XNAAudioListener Listener { get; private set; }

        public AudioListener()
            : base()
        {
            Listener = new XNAAudioListener();
        }

        public override void Update()
        {
            base.Update();

            Listener.Forward = _transform.Forward;
            Listener.Position = _transform.LocalPosition;
            Listener.Up = Vector3.Up;
        }
    }
}
