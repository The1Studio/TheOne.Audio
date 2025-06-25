#nullable enable
namespace UniT.Audio
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using UniT.Extensions;
    using UniT.Logging;
    using UniT.ResourceManagement;
    using UnityEngine;
    using UnityEngine.Scripting;
    using ILogger = UniT.Logging.ILogger;
    #if UNIT_UNITASK
    using System.Threading;
    using Cysharp.Threading.Tasks;
    #else
    using System.Collections;
    #endif

    public sealed class AudioManager : IAudioManagerSettings, IAudioManager
    {
        #region Constructor

        private readonly IAssetsManager assetsManager;
        private readonly ILogger        logger;

        private readonly AudioSettings      settings         = new AudioSettings();
        private readonly GameObject         sourcesContainer = new GameObject(nameof(AudioManager)).DontDestroyOnLoad();
        private readonly Queue<AudioSource> sourcePool       = new Queue<AudioSource>();

        private readonly AudioPool soundPool;
        private readonly AudioPool musicPool;

        [Preserve]
        public AudioManager(IAssetsManager assetsManager, ILoggerManager loggerManager)
        {
            this.assetsManager = assetsManager;
            this.logger        = loggerManager.GetLogger(this);
            this.soundPool     = new AudioPool(this);
            this.musicPool     = new AudioPool(this);
            this.logger.Debug("Constructed");
        }

        #endregion

        #region Settings

        event Action IAudioManagerSettings.SoundVolumeOutputChanged { add => this.soundPool.VolumeOutputChanged += value; remove => this.soundPool.VolumeOutputChanged -= value; }

        event Action IAudioManagerSettings.MuteSoundOutputChanged { add => this.soundPool.MuteOutputChanged += value; remove => this.soundPool.MuteOutputChanged -= value; }

        event Action IAudioManagerSettings.MusicVolumeOutputChanged { add => this.musicPool.VolumeOutputChanged += value; remove => this.musicPool.VolumeOutputChanged -= value; }

        event Action IAudioManagerSettings.MuteMusicOutputChanged { add => this.musicPool.MuteOutputChanged += value; remove => this.musicPool.MuteOutputChanged -= value; }

        event Action IAudioManagerSettings.SoundVolumeChanged { add => this.soundPool.Settings.VolumeChanged += value; remove => this.soundPool.Settings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteSoundChanged { add => this.soundPool.Settings.MuteChanged += value; remove => this.soundPool.Settings.MuteChanged -= value; }

        event Action IAudioManagerSettings.MusicVolumeChanged { add => this.musicPool.Settings.VolumeChanged += value; remove => this.musicPool.Settings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteMusicChanged { add => this.musicPool.Settings.MuteChanged += value; remove => this.musicPool.Settings.MuteChanged -= value; }

        event Action IAudioManagerSettings.MasterVolumeChanged { add => this.settings.VolumeChanged += value; remove => this.settings.VolumeChanged -= value; }

        event Action IAudioManagerSettings.MuteMasterChanged { add => this.settings.MuteChanged += value; remove => this.settings.MuteChanged -= value; }

        float IAudioManagerSettings.SoundVolumeOutput => this.soundPool.VolumeOutput;

        bool IAudioManagerSettings.MuteSoundOutput => this.soundPool.MuteOutput;

        float IAudioManagerSettings.MusicVolumeOutput => this.musicPool.VolumeOutput;

        bool IAudioManagerSettings.MuteMusicOutput => this.musicPool.MuteOutput;

        float IAudioManagerSettings.SoundVolume { get => this.soundPool.Settings.Volume; set => this.soundPool.Settings.Volume = value; }

        bool IAudioManagerSettings.MuteSound { get => this.soundPool.Settings.Mute; set => this.soundPool.Settings.Mute = value; }

        float IAudioManagerSettings.MusicVolume { get => this.musicPool.Settings.Volume; set => this.musicPool.Settings.Volume = value; }

        bool IAudioManagerSettings.MuteMusic { get => this.musicPool.Settings.Mute; set => this.musicPool.Settings.Mute = value; }

        float IAudioManagerSettings.MasterVolume { get => this.settings.Volume; set => this.settings.Volume = value; }

        bool IAudioManagerSettings.MuteMaster { get => this.settings.Mute; set => this.settings.Mute = value; }

        void IAudioManagerSettings.RegisterSound(AudioSource source) => this.soundPool.Register(source);

        void IAudioManagerSettings.UnregisterSound(AudioSource soundSource) => this.soundPool.Unregister(soundSource);

        #endregion

        #region Sound

        void IAudioManager.LoadSound(AudioClip clip) => this.soundPool.Load(clip);

        void IAudioManager.LoadSound(string name) => this.soundPool.Load(name);

        #if UNIT_UNITASK
        UniTask IAudioManager.LoadSoundAsync(string name, IProgress<float>? progress, CancellationToken cancellationToken) => this.soundPool.LoadAsync(name, progress, cancellationToken);
        #else
        IEnumerator IAudioManager.LoadSoundAsync(string name, Action? callback, IProgress<float>? progress) => this.soundPool.LoadAsync(name, callback, progress);
        #endif

        void IAudioManager.PlaySoundOneShot(AudioClip clip) => this.soundPool.PlayOneShot(clip);

        void IAudioManager.PlaySoundOneShot(string name) => this.soundPool.PlayOneShot(name);

        void IAudioManager.PlaySound(AudioClip clip, bool loop, bool force) => this.soundPool.Play(clip, loop, force);

        void IAudioManager.PlaySound(string name, bool loop, bool force) => this.soundPool.Play(name, loop, force);

        void IAudioManager.PauseSound(AudioClip clip) => this.soundPool.Pause(clip);

        void IAudioManager.PauseSound(string name) => this.soundPool.Pause(name);

        void IAudioManager.PauseAllSounds() => this.soundPool.PauseAll();

        void IAudioManager.ResumeSound(AudioClip clip) => this.soundPool.Resume(clip);

        void IAudioManager.ResumeSound(string name) => this.soundPool.Resume(name);

        void IAudioManager.ResumeAllSounds() => this.soundPool.ResumeAll();

        void IAudioManager.StopSound(AudioClip clip) => this.soundPool.Stop(clip);

        void IAudioManager.StopSound(string name) => this.soundPool.Stop(name);

        void IAudioManager.StopAllSounds() => this.soundPool.StopAll();

        void IAudioManager.UnloadSound(AudioClip clip) => this.soundPool.Unload(clip);

        void IAudioManager.UnloadSound(string name) => this.soundPool.Unload(name);

        void IAudioManager.UnloadAllSounds() => this.soundPool.UnloadAll();

        #endregion

        #region Music

        private AudioClip? currentMusicClip;
        private string?    currentMusicName;

        string? IAudioManager.CurrentMusic => this.currentMusicClip?.name ?? this.currentMusicName;

        float IAudioManager.MusicTime
        {
            get
            {
                if (this.currentMusicClip is { })
                {
                    return this.musicPool.GetTime(this.currentMusicClip);
                }
                if (this.currentMusicName is { })
                {
                    return this.musicPool.GetTime(this.currentMusicName);
                }
                return 0;
            }
            set
            {
                if (this.currentMusicClip is { })
                {
                    this.musicPool.SetTime(this.currentMusicClip, value);
                }
                if (this.currentMusicName is { })
                {
                    this.musicPool.SetTime(this.currentMusicName, value);
                }
            }
        }

        void IAudioManager.LoadMusic(AudioClip clip) => this.musicPool.Load(clip);

        void IAudioManager.LoadMusic(string name) => this.musicPool.Load(name);

        #if UNIT_UNITASK
        UniTask IAudioManager.LoadMusicAsync(string name, IProgress<float>? progress, CancellationToken cancellationToken) => this.musicPool.LoadAsync(name, progress, cancellationToken);
        #else
        IEnumerator IAudioManager.LoadMusicAsync(string name, Action? callback, IProgress<float>? progress) => this.musicPool.LoadAsync(name, callback, progress);
        #endif

        void IAudioManager.PlayMusic(AudioClip clip, bool loop, bool force)
        {
            if (this.currentMusicClip is { } && this.currentMusicClip != clip)
            {
                this.musicPool.Stop(this.currentMusicClip);
                this.currentMusicClip = null;
            }
            if (this.currentMusicName is { })
            {
                this.musicPool.Stop(this.currentMusicName);
                this.currentMusicName = null;
            }
            this.musicPool.Play(clip, loop, force);
            this.currentMusicClip = clip;
        }

        void IAudioManager.PlayMusic(string name, bool loop, bool force)
        {
            if (this.currentMusicClip is { })
            {
                this.musicPool.Stop(this.currentMusicClip);
                this.currentMusicClip = null;
            }
            if (this.currentMusicName is { } && this.currentMusicName != name)
            {
                this.musicPool.Stop(this.currentMusicName);
                this.currentMusicName = null;
            }
            this.musicPool.Play(name, loop, force);
            this.currentMusicName = name;
        }

        void IAudioManager.PauseMusic()
        {
            if (this.currentMusicClip is { } clip)
            {
                this.musicPool.Pause(clip);
            }
            if (this.currentMusicName is { } name)
            {
                this.musicPool.Pause(name);
            }
        }

        void IAudioManager.ResumeMusic()
        {
            if (this.currentMusicClip is { } clip)
            {
                this.musicPool.Resume(clip);
            }
            if (this.currentMusicName is { } name)
            {
                this.musicPool.Resume(name);
            }
        }

        void IAudioManager.StopMusic()
        {
            if (this.currentMusicClip is { } clip)
            {
                this.musicPool.Stop(clip);
            }
            if (this.currentMusicName is { } name)
            {
                this.musicPool.Stop(name);
            }
        }

        void IAudioManager.UnloadMusic(AudioClip clip)
        {
            this.musicPool.Unload(clip);
            if (this.currentMusicClip == clip)
            {
                this.currentMusicClip = null;
            }
        }

        void IAudioManager.UnloadMusic(string name)
        {
            this.musicPool.Unload(name);
            if (this.currentMusicName == name)
            {
                this.currentMusicName = null;
            }
        }

        void IAudioManager.UnloadAllMusics()
        {
            this.musicPool.UnloadAll();
            this.currentMusicClip = null;
            this.currentMusicName = null;
        }

        #endregion

        private sealed class AudioPool
        {
            private readonly AudioManager manager;

            private readonly HashSet<AudioSource>               registeredSources = new HashSet<AudioSource>();
            private readonly Dictionary<string, AudioClip>      nameToClip        = new Dictionary<string, AudioClip>();
            private readonly Dictionary<AudioClip, AudioSource> clipToSource      = new Dictionary<AudioClip, AudioSource>();

            public AudioPool(AudioManager manager)
            {
                this.manager = manager;

                manager.settings.VolumeChanged += this.OnVolumeOutputChanged;
                manager.settings.MuteChanged   += this.OnMuteOutputChanged;

                this.Settings.VolumeChanged += this.OnVolumeOutputChanged;
                this.Settings.MuteChanged   += this.OnMuteOutputChanged;
            }

            public AudioSettings Settings { get; } = new AudioSettings();

            #region Public

            public event Action? VolumeOutputChanged;

            public event Action? MuteOutputChanged;

            public float VolumeOutput => this.Settings.Volume * this.manager.settings.Volume;

            public bool MuteOutput => this.Settings.Mute || this.manager.settings.Mute;

            public void Register(AudioSource source)
            {
                this.Configure(source);
                this.registeredSources.Add(source);
            }

            public void Unregister(AudioSource source)
            {
                this.registeredSources.Remove(source);
            }

            public void Load(AudioClip clip)
            {
                this.clipToSource.TryAdd(clip, () =>
                {
                    var source = this.manager.sourcePool.DequeueOrDefault(() => this.manager.sourcesContainer.AddComponent<AudioSource>());
                    this.Configure(source);
                    source.clip = clip;
                    return source;
                });
            }

            public void Load(string name)
            {
                var clip = this.nameToClip.GetOrAdd(name, () => this.manager.assetsManager.Load<AudioClip>(name));
                this.Load(clip);
            }

            #if UNIT_UNITASK
            public async UniTask LoadAsync(string name, IProgress<float>? progress, CancellationToken cancellationToken)
            {
                var clip = await this.nameToClip.GetOrAddAsync(name, () => this.manager.assetsManager.LoadAsync<AudioClip>(name, progress, cancellationToken));
                this.Load(clip);
            }
            #else
            public IEnumerator LoadAsync(string name, Action? callback, IProgress<float>? progress)
            {
                var clip = default(AudioClip)!;
                yield return this.nameToClip.GetOrAddAsync(
                    name,
                    callback => this.assetsManager.LoadAsync(name, callback, progress),
                    result => clip = result
                );
                this.Load(clip);
                callback?.Invoke();
            }
            #endif

            public void PlayOneShot(AudioClip clip)
            {
                var source = this.GetOrLoadSource(clip);
                source.PlayOneShot(source.clip);
                this.manager.logger.Debug($"Playing one shot {clip.name}");
            }

            public void PlayOneShot(string name)
            {
                var clip = this.nameToClip.GetOrAdd(name, () => this.manager.assetsManager.Load<AudioClip>(name));
                this.PlayOneShot(clip);
            }

            public void Play(AudioClip clip, bool loop, bool force)
            {
                var source = this.GetOrLoadSource(clip);
                source.loop = loop;
                if (!force && source.isPlaying) return;
                source.Play();
                this.manager.logger.Debug($"Playing {clip.name}, loop: {loop}");
            }

            public void Play(string name, bool loop, bool force)
            {
                var clip = this.nameToClip.GetOrAdd(name, () => this.manager.assetsManager.Load<AudioClip>(name));
                this.Play(clip, loop, force);
            }

            public float GetTime(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return 0;
                return source.time;
            }

            public float GetTime(string name)
            {
                if (!this.TryGetClip(name, out var clip)) return 0;
                return this.GetTime(clip);
            }

            public void SetTime(AudioClip clip, float time)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.time = time;
                this.manager.logger.Debug($"Set {clip.name} time to {time}");
            }

            public void SetTime(string name, float time)
            {
                if (!this.TryGetClip(name, out var clip)) return;
                this.SetTime(clip, time);
            }

            public void Pause(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Pause();
                this.manager.logger.Debug($"Paused {clip.name}");
            }

            public void Pause(string name)
            {
                if (!this.TryGetClip(name, out var clip)) return;
                this.Pause(clip);
            }

            public void PauseAll()
            {
                this.clipToSource.Keys.ForEach(this.Pause);
            }

            public void Resume(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.UnPause();
                this.manager.logger.Debug($"Resumed {clip.name}");
            }

            public void Resume(string name)
            {
                if (!this.TryGetClip(name, out var clip)) return;
                this.Resume(clip);
            }

            public void ResumeAll()
            {
                this.clipToSource.Keys.ForEach(this.Resume);
            }

            public void Stop(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Stop();
                this.manager.logger.Debug($"Stopped {clip.name}");
            }

            public void Stop(string name)
            {
                if (!this.TryGetClip(name, out var clip)) return;
                this.Stop(clip);
            }

            public void StopAll()
            {
                this.clipToSource.Keys.ForEach(this.Stop);
            }

            public void Unload(AudioClip clip)
            {
                if (!this.TryGetSource(clip, out var source)) return;
                source.Stop();
                source.clip = null;
                this.manager.sourcePool.Enqueue(source);
                this.clipToSource.Remove(clip);
                this.manager.logger.Debug($"Unloaded {clip.name}");
            }

            public void Unload(string name)
            {
                if (!this.TryGetClip(name, out var clip)) return;
                this.Unload(clip);
                this.manager.assetsManager.Unload(name);
                this.nameToClip.Remove(name);
            }

            public void UnloadAll()
            {
                this.nameToClip.Keys.SafeForEach(this.Unload);
                this.clipToSource.Keys.SafeForEach(this.Unload);
            }

            #endregion

            #region Private

            private void OnVolumeOutputChanged()
            {
                this.clipToSource.ForEach(this.ConfigureVolume);
                this.registeredSources.ForEach(this.ConfigureVolume);
                this.VolumeOutputChanged?.Invoke();
            }

            private void OnMuteOutputChanged()
            {
                this.clipToSource.ForEach(this.ConfigureMute);
                this.registeredSources.ForEach(this.ConfigureMute);
                this.MuteOutputChanged?.Invoke();
            }

            private void Configure(AudioSource source)
            {
                this.ConfigureVolume(source);
                this.ConfigureMute(source);
            }

            private void ConfigureVolume(AudioSource source)
            {
                source.volume = this.VolumeOutput;
            }

            private void ConfigureMute(AudioSource source)
            {
                source.mute = this.MuteOutput;
            }

            private AudioSource GetOrLoadSource(AudioClip clip)
            {
                if (!this.clipToSource.ContainsKey(clip))
                {
                    this.Load(clip);
                    this.manager.logger.Warning($"Auto loaded {clip.name}. Consider preload it with `Load` or `LoadAsync` for better performance.");
                }
                return this.clipToSource[clip];
            }

            private bool TryGetSource(AudioClip clip, [MaybeNullWhen(false)] out AudioSource source)
            {
                if (this.clipToSource.TryGetValue(clip, out source)) return true;
                this.manager.logger.Warning($"{clip.name} not loaded");
                return false;
            }

            private bool TryGetClip(string name, [MaybeNullWhen(false)] out AudioClip clip)
            {
                if (this.nameToClip.TryGetValue(name, out clip)) return true;
                this.manager.logger.Warning($"{name} not loaded");
                return false;
            }

            #endregion
        }

        private sealed class AudioSettings
        {
            public event Action? VolumeChanged;

            public event Action? MuteChanged;

            public float Volume
            {
                get => this.volume;
                set
                {
                    if (value is < 0 or > 1)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value), "Volume must be between 0 and 1");
                    }
                    this.volume = value;
                    this.VolumeChanged?.Invoke();
                }
            }

            public bool Mute
            {
                get => this.mute;
                set
                {
                    this.mute = value;
                    this.MuteChanged?.Invoke();
                }
            }

            private float volume = 1;
            private bool  mute   = false;
        }
    }
}