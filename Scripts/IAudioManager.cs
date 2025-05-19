#nullable enable
namespace TheOne.Audio
{
    using System;
    using UnityEngine;
    #if THEONE_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public interface IAudioManager
    {
        #region Sound

        public void LoadSound(AudioClip clip);

        public void LoadSound(object key);

        public void PlaySoundOneShot(AudioClip clip);

        public void PlaySoundOneShot(object key);

        public void PlaySound(AudioClip clip, bool loop = false, bool force = false);

        public void PlaySound(object key, bool loop = false, bool force = false);

        public void PauseSound(AudioClip clip);

        public void PauseSound(object key);

        public void PauseAllSounds();

        public void ResumeSound(AudioClip clip);

        public void ResumeSound(object key);

        public void ResumeAllSounds();

        public void StopSound(AudioClip clip);

        public void StopSound(object key);

        public void StopAllSounds();

        public void UnloadSound(AudioClip clip);

        public void UnloadSound(object key);

        public void UnloadAllSounds();

        #endregion

        #region Music

        public object? CurrentMusic { get; }

        public float MusicTime { get; set; }

        public void LoadMusic(AudioClip clip);

        public void LoadMusic(object key);

        public void PlayMusic(AudioClip clip, bool loop = true, bool force = false);

        public void PlayMusic(object key, bool loop = true, bool force = false);

        public void PauseMusic();

        public void ResumeMusic();

        public void StopMusic();

        public void UnloadMusic(AudioClip clip);

        public void UnloadMusic(object key);

        public void UnloadAllMusics();

        #endregion

        #region Async

        #if THEONE_UNITASK
        public UniTask LoadSoundAsync(object key, IProgress<float>? progress = null, CancellationToken cancellationToken = default);

        public UniTask LoadMusicAsync(object key, IProgress<float>? progress = null, CancellationToken cancellationToken = default);
        #else
        public IEnumerator LoadSoundAsync(object key, Action? callback = null, IProgress<float>? progress = null);

        public IEnumerator LoadMusicAsync(object key, Action? callback = null, IProgress<float>? progress = null);
        #endif

        #endregion
    }
}