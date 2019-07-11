using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace C3DE.Components
{
    using XNAAudioListener = Microsoft.Xna.Framework.Audio.AudioListener;

    public class AudioSource : Component
    {
        private XNAAudioListener _audioListener;
        private AudioEmitter _audioEmitter;
        private SoundEffect _soundEffect;
        private SoundEffectInstance _soundEffectInstance;
        private float _volume = 1.0f;
        private float _pitch = 1.0f;
        private float _pan = 0.0f;
        private bool _loop = false;

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                if (_soundEffectInstance != null)
                    _soundEffectInstance.Volume = value;
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = value;
                if (_soundEffectInstance != null)
                    _soundEffectInstance.Pitch = value;
            }
        }

        public float Pan
        {
            get => _pan;
            set
            {
                _pan = value;
                if (_soundEffectInstance != null)
                    _soundEffectInstance.Pan = value;
            }
        }

        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                if (_soundEffectInstance != null)
                    _soundEffectInstance.IsLooped = value;
            }
        }

        public XNAAudioListener AudioListener
        {
            get
            {
                if (_audioListener == null)
                {
                    var camera = Camera.Main;
                    var listener = camera.GetComponent<AudioListener>();
                    _audioListener = listener?.Listener;

                    if (_audioListener == null)
                    {
                        listener = camera.AddComponent<AudioListener>();
                        _audioListener = listener.Listener;
                    }
                }

                return _audioListener;
            }
            set => _audioListener = value;
        }

        public bool PlayOnAwake { get; set; } = false;

        public SoundEffect AudioClip
        {
            get { return _soundEffect; }
            set
            {
                if (_soundEffect == value)
                    return;

                _soundEffect = value;
                _soundEffectInstance = _soundEffect.CreateInstance();
                _soundEffectInstance.Pan = _pan;
                _soundEffectInstance.Pitch = _pitch;
                _soundEffectInstance.IsLooped = _loop;
                _soundEffectInstance.Volume = _volume;
            }
        }

        public override void Awake()
        {
            base.Awake();

            SoundEffect.DistanceScale = 2000.0f;
            SoundEffect.DopplerScale = 0.1f;

            _audioEmitter = new AudioEmitter();

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
            if (_soundEffectInstance == null)
                return;

            _audioEmitter.Position = _transform.LocalPosition;
            _audioEmitter.Forward = _transform.Forward;
            _audioEmitter.Up = Vector3.Up;
            _audioEmitter.Velocity = Vector3.One;
            _soundEffectInstance.Apply3D(AudioListener, _audioEmitter);
        }

        public void Play()
        {
            if (_soundEffectInstance == null)
                return;

            UpdateSound();

            _soundEffectInstance.Play();
        }

        public void Stop()
        {
            if (_soundEffectInstance == null)
                return;

            _soundEffectInstance.Stop();
        }

        public void Pause()
        {
            if (_soundEffectInstance == null)
                return;

            _soundEffectInstance.Pause();
        }
    }
}
