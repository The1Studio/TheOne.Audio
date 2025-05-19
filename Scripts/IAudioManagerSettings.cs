#nullable enable
namespace TheOne.Audio
{
    using System;
    using UnityEngine;

    public interface IAudioManagerSettings
    {
        public event Action SoundVolumeOutputChanged;

        public event Action MuteSoundOutputChanged;

        public event Action MusicVolumeOutputChanged;

        public event Action MuteMusicOutputChanged;

        public event Action SoundVolumeChanged;

        public event Action MuteSoundChanged;

        public event Action MusicVolumeChanged;

        public event Action MuteMusicChanged;

        public event Action MasterVolumeChanged;

        public event Action MuteMasterChanged;

        public float SoundVolumeOutput { get; }

        public bool MuteSoundOutput { get; }

        public float MusicVolumeOutput { get; }

        public bool MuteMusicOutput { get; }

        public float SoundVolume { get; set; }

        public bool MuteSound { get; set; }

        public float MusicVolume { get; set; }

        public bool MuteMusic { get; set; }

        public float MasterVolume { get; set; }

        public bool MuteMaster { get; set; }

        public void RegisterSound(AudioSource source);

        public void UnregisterSound(AudioSource source);
    }
}