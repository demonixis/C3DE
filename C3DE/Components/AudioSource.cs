using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace C3DE.Components
{
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

    public class AudioSource : Component
    {
        private XNAAudioListener m_AudioListener;
        private AudioEmitter m_AudioEmitter;
        private SoundEffect m_SoundEffect;
        private SoundEffectInstance m_SoundEffectInstance;
        private float m_Volume = 1.0f;
        private float m_Pitch = 1.0f;
        private float m_Pan = 0.0f;
        private bool m_Loop = false;

        public float Volume
        {
            get => m_Volume;
            set
            {
                m_Volume = value;
                if (m_SoundEffectInstance != null)
                    m_SoundEffectInstance.Volume = value;
            }
        }

        public float Pitch
        {
            get => m_Pitch;
            set
            {
                m_Pitch = value;
                if (m_SoundEffectInstance != null)
                    m_SoundEffectInstance.Pitch = value;
            }
        }

        public float Pan
        {
            get => m_Pan;
            set
            {
                m_Pan = value;
                if (m_SoundEffectInstance != null)
                    m_SoundEffectInstance.Pan = value;
            }
        }

        public bool Loop
        {
            get => m_Loop;
            set
            {
                m_Loop = value;
                if (m_SoundEffectInstance != null)
                    m_SoundEffectInstance.IsLooped = value;
            }
        }

        public XNAAudioListener AudioListener
        {
            get
            {
                if (m_AudioListener == null)
                {
                    var camera = Camera.Main;
                    var listener = camera.GetComponent<AudioListener>();
                    m_AudioListener = listener?.Listener;

                    if (m_AudioListener == null)
                    {
                        listener = camera.AddComponent<AudioListener>();
                        m_AudioListener = listener.Listener;
                    }
                }

                return m_AudioListener;
            }
            set { m_AudioListener = value; }
        }

        public bool PlayOnAwake { get; set; } = false;

        public SoundEffect AudioClip
        {
            get { return m_SoundEffect; }
            set
            {
                if (m_SoundEffect == value)
                    return;

                m_SoundEffect = value;
                m_SoundEffectInstance = m_SoundEffect.CreateInstance();
                m_SoundEffectInstance.Pan = m_Pan;
                m_SoundEffectInstance.Pitch = m_Pitch;
                m_SoundEffectInstance.IsLooped = m_Loop;
                m_SoundEffectInstance.Volume = m_Volume;
            }
        }

        public override void Awake()
        {
            base.Awake();

            SoundEffect.DistanceScale = 2000.0f;
            SoundEffect.DopplerScale = 0.1f;

            if (PlayOnAwake)
                Play();
        }

        public override void Update()
        {
            base.Update();
            UpdateSound();
        }

        private void UpdateSound()
        {
            if (m_SoundEffectInstance == null)
                return;

            m_AudioEmitter.Position = m_Transform.LocalPosition;
            m_AudioEmitter.Forward = m_Transform.Forward;
            m_AudioEmitter.Up = m_Transform.Up;
            m_AudioEmitter.Velocity = Vector3.One;
            m_SoundEffectInstance.Apply3D(AudioListener, m_AudioEmitter);
        }

        public void Play()
        {
            if (m_SoundEffectInstance == null)
                return;

            UpdateSound();

            m_SoundEffectInstance.Play();
        }

        public void Stop()
        {
            if (m_SoundEffectInstance == null)
                return;

            m_SoundEffectInstance.Stop();
        }

        public void Pause()
        {
            if (m_SoundEffectInstance == null)
                return;

            m_SoundEffectInstance.Pause();
        }
    }
}
