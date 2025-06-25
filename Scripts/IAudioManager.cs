#nullable enable
namespace UniT.Audio
{
    using System;
    using UnityEngine;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public interface IAudioManager
    {
        #region Sound

        public void LoadSound(AudioClip clip);

        public void LoadSound(string name);

        public void PlaySoundOneShot(AudioClip clip);

        public void PlaySoundOneShot(string name);

        public void PlaySound(AudioClip clip, bool loop = false, bool force = false);

        public void PlaySound(string name, bool loop = false, bool force = false);

        public void PauseSound(AudioClip clip);

        public void PauseSound(string name);

        public void PauseAllSounds();

        public void ResumeSound(AudioClip clip);

        public void ResumeSound(string name);

        public void ResumeAllSounds();

        public void StopSound(AudioClip clip);

        public void StopSound(string name);

        public void StopAllSounds();

        public void UnloadSound(AudioClip clip);

        public void UnloadSound(string name);

        public void UnloadAllSounds();

        #endregion

        #region Music

        public string? CurrentMusic { get; }

        public float MusicTime { get; set; }

        public void LoadMusic(AudioClip clip);

        public void LoadMusic(string name);

        public void PlayMusic(AudioClip clip, bool loop = true, bool force = false);

        public void PlayMusic(string name, bool loop = true, bool force = false);

        public void PauseMusic();

        public void ResumeMusic();

        public void StopMusic();

        public void UnloadMusic(AudioClip clip);

        public void UnloadMusic(string name);

        public void UnloadAllMusics();

        #endregion

        #region Async

        #if UNIT_UNITASK
        public UniTask LoadSoundAsync(string name, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public UniTask LoadMusicAsync(string name, IProgress<float>? progress = null, CancellationToken cancellationToken = default);
        #else
        public IEnumerator LoadSoundAsync(string name, Action? callback = null, IProgress<float>? progress = null);

        public IEnumerator LoadMusicAsync(string name, Action? callback = null, IProgress<float>? progress = null);
        #endif

        #endregion
    }
}