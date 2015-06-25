using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;
using System;

namespace C3DE
{
    [Serializable]
    public static class AudioManager
    {
        private static bool _soundEnabled = true;
        private static bool _musicEnabled = true;
        private static float _maxSoundVolume = 1.0f;
        private static float _maxMusicVolume = 1.0f;

        public static bool SoundEnabled
        {
            get { return _soundEnabled; }
            set { _soundEnabled = value; }
        }

        public static float SoundVolume
        {
            get { return _maxSoundVolume; }
            set { _maxSoundVolume = value; }
        }

        public static bool MusicEnabled
        {
            get { return _musicEnabled; }
            set
            {
                _musicEnabled = value;

                if (!_musicEnabled)
                    Stop();
            }
        }

        public static float MusicVolume
        {
            get { return _maxMusicVolume; }
            set 
            {
                _maxMusicVolume = value;
                MediaPlayer.Volume = Math.Min(_maxMusicVolume, MediaPlayer.Volume);
            }
        }

        public static void PlayOneShot(SoundEffect sound, float volume = 1.0f, float pitch = 1.0f, float pan = 0.0f)
        {
            if (_soundEnabled)
                sound.Play(Math.Min(volume, _maxSoundVolume), pitch, pan);
        }

        public static void Play(Song music, float volume = 1.0f, bool repeat = true)
        {
            if (_musicEnabled)
            {
                Stop();
                MediaPlayer.Volume = Math.Min(volume, _maxMusicVolume);
                MediaPlayer.IsRepeating = repeat;
                MediaPlayer.Play(music);
            }
        }

        public static void Stop()
        {
             if (MediaPlayer.State != MediaState.Stopped)
                MediaPlayer.Stop();
        }

        public static void Pause()
        {
            if (MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Pause();
        }

        public static void Resume()
        {
            if (_musicEnabled && MediaPlayer.State == MediaState.Playing)
                MediaPlayer.Resume();
        }
    }
}