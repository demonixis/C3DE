using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;

namespace C3DE
{
    public class AudioManager
    {
        private bool _soundEnabled;
        private bool _musicEnabled;
        private float _maxSoundVolume;
        private float _maxMusicVolume;

        public bool SoundEnabled
        {
            get { return _soundEnabled; }
            set { _soundEnabled = value; }
        }

        public float SoundVolume
        {
            get { return _maxSoundVolume; }
            set { _maxSoundVolume = value; }
        }

        public bool MusicEnabled
        {
            get { return _musicEnabled; }
            set
            {
                _musicEnabled = value;

                if (!_musicEnabled)
                    Stop();
            }
        }

        public float MusicVolume
        {
            get { return _maxMusicVolume; }
            set 
            {
                _maxMusicVolume = value;
                MediaPlayer.Volume = Math.Min(_maxMusicVolume, MediaPlayer.Volume);
            }
        }

        public AudioManager()
        {
            _soundEnabled = true;
            _musicEnabled = true;
            _maxSoundVolume = 1.0f;
            _maxMusicVolume = 1.0f;
        }

        public void PlayOneShot(SoundEffect sound, float volume = 1.0f, float pitch = 1.0f, float pan = 0.0f)
        {
            if (_soundEnabled)
                sound.Play(Math.Min(volume, _maxSoundVolume), pitch, pan);
        }

        public void Play(Song music, float volume = 1.0f, bool repeat = true)
        {
            if (_musicEnabled)
            {
                Stop();
                MediaPlayer.Volume = Math.Min(volume, _maxMusicVolume);
                MediaPlayer.IsRepeating = repeat;
                MediaPlayer.Play(music);
            }
        }

        public void Stop()
        {
             if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();
        }

        public void Pause()
        {
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Pause();
        }

        public void Resume()
        {
            if (_musicEnabled && MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Resume();
        }
    }
}