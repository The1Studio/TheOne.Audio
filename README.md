# TheOne.Audio

Audio Manager for Unity

## Installation

### Option 1: Unity Scoped Registry (Recommended)

Add the following scoped registry to your project's `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "TheOne Studio",
      "url": "https://upm.the1studio.org/",
      "scopes": [
        "com.theone"
      ]
    }
  ],
  "dependencies": {
    "com.theone.audio": "1.1.0"
  }
}
```

### Option 2: Git URL

Add to Unity Package Manager:
```
https://github.com/The1Studio/TheOne.Audio.git
```

## Features

- Centralized audio management
- Support for multiple audio sources
- Easy-to-use API for playing sounds and music
- Volume control and audio settings management

## Dependencies

- TheOne.Extensions
- TheOne.Logging  
- TheOne.ResourceManagement

## Usage

### Basic Usage

```csharp
using TheOne.Audio;

// Play a sound effect
audioManager.PlaySoundOneShot("click");
audioManager.PlaySound("click", loop: false, force: false);

// Play background music
audioManager.PlayMusic("main-theme", loop: true, force: false);

// Load sounds and music
audioManager.LoadSound("click");
audioManager.LoadMusic("main-theme");

// Control playback
audioManager.PauseSound("click");
audioManager.ResumeSound("click");
audioManager.StopSound("click");

audioManager.PauseMusic();
audioManager.ResumeMusic();
audioManager.StopMusic();
```

### Advanced Usage

```csharp
// Load and play using AudioClip reference
[SerializeField] private AudioClip explosionClip;
audioManager.LoadSound(explosionClip);
audioManager.PlaySound(explosionClip, loop: false, force: true);

// Stop all sounds and music
audioManager.StopAllSounds();
audioManager.StopMusic();

// Unload resources
audioManager.UnloadSound("explosion");
audioManager.UnloadAllSounds();
audioManager.UnloadMusic("main-theme");
audioManager.UnloadAllMusics();

// Check current music and control time
string currentMusic = audioManager.CurrentMusic;
float currentTime = audioManager.MusicTime;
audioManager.MusicTime = 30.0f; // Jump to 30 seconds
```

### Async Loading Examples

```csharp
using TheOne.Audio;
using System;
using System.Collections;
#if THEONE_UNITASK
using Cysharp.Threading.Tasks;
using System.Threading;
#endif

public class AsyncAudioExample : MonoBehaviour
{
    private IAudioManager audioManager;
    
    #if THEONE_UNITASK
    // UniTask-based async loading
    public async UniTask LoadAudioAsync()
    {
        var progress = new Progress<float>(p => Debug.Log($"Loading: {p:P}"));
        var cancellationToken = this.GetCancellationTokenOnDestroy();
        
        try
        {
            // Load sound asynchronously with progress
            await audioManager.LoadSoundAsync("explosion", progress, cancellationToken);
            
            // Load music asynchronously
            await audioManager.LoadMusicAsync("background-music", progress, cancellationToken);
            
            // Play loaded audio
            audioManager.PlaySoundOneShot("explosion");
            audioManager.PlayMusic("background-music");
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Audio loading was cancelled");
        }
    }
    #else
    // Coroutine-based async loading
    public void LoadAudioAsync()
    {
        StartCoroutine(LoadAudioCoroutine());
    }
    
    private IEnumerator LoadAudioCoroutine()
    {
        var progress = new Progress<float>(p => Debug.Log($"Loading: {p:P}"));
        
        // Load sound asynchronously with callback
        yield return audioManager.LoadSoundAsync("explosion", 
            callback: () => Debug.Log("Sound loaded"), 
            progress: progress);
        
        // Load music asynchronously with callback
        yield return audioManager.LoadMusicAsync("background-music",
            callback: () => Debug.Log("Music loaded"),
            progress: progress);
        
        // Play loaded audio
        audioManager.PlaySoundOneShot("explosion");
        audioManager.PlayMusic("background-music");
    }
    #endif
}
```

## Architecture

### Folder Structure

```
TheOne.Audio/
├── Scripts/
│   ├── AudioManager.cs           # Main audio manager implementation
│   ├── IAudioManager.cs          # Public interface for audio operations
│   ├── IAudioManagerSettings.cs  # Settings interface
│   └── DI/                       # Dependency injection integrations
│       ├── AudioManagerDI.cs
│       ├── AudioManagerVContainer.cs
│       └── AudioManagerZenject.cs
```

### Core Classes

#### `IAudioManager`
Main interface for audio operations:

**Sound Operations:**
- `LoadSound(AudioClip clip)` / `LoadSound(string name)` - Load sound into memory
- `PlaySoundOneShot(AudioClip clip)` / `PlaySoundOneShot(string name)` - Play sound once
- `PlaySound(AudioClip clip, bool loop, bool force)` / `PlaySound(string name, bool loop, bool force)` - Play sound with options
- `PauseSound(AudioClip clip)` / `PauseSound(string name)` - Pause specific sound
- `ResumeSound(AudioClip clip)` / `ResumeSound(string name)` - Resume specific sound
- `StopSound(AudioClip clip)` / `StopSound(string name)` - Stop specific sound
- `PauseAllSounds()` / `ResumeAllSounds()` / `StopAllSounds()` - Control all sounds
- `UnloadSound(AudioClip clip)` / `UnloadSound(string name)` / `UnloadAllSounds()` - Memory management

**Music Operations:**
- `LoadMusic(AudioClip clip)` / `LoadMusic(string name)` - Load music into memory
- `PlayMusic(AudioClip clip, bool loop, bool force)` / `PlayMusic(string name, bool loop, bool force)` - Play music
- `PauseMusic()` / `ResumeMusic()` / `StopMusic()` - Control music playback
- `UnloadMusic(AudioClip clip)` / `UnloadMusic(string name)` / `UnloadAllMusics()` - Memory management
- `string? CurrentMusic` - Get currently playing music name
- `float MusicTime` - Get/set current music playback time

**Async Operations:**
```csharp
#if THEONE_UNITASK
UniTask LoadSoundAsync(string name, IProgress<float>? progress = null, CancellationToken cancellationToken = default);
UniTask LoadMusicAsync(string name, IProgress<float>? progress = null, CancellationToken cancellationToken = default);
#else
IEnumerator LoadSoundAsync(string name, Action? callback = null, IProgress<float>? progress = null);
IEnumerator LoadMusicAsync(string name, Action? callback = null, IProgress<float>? progress = null);
#endif
```

#### `IAudioManagerSettings`
Interface for audio settings:
- `float SoundVolume` - Master volume for sound effects (0-1)
- `float MusicVolume` - Master volume for music (0-1)
- `bool MuteSound` - Mute all sound effects
- `bool MuteMusic` - Mute all music
- Events for volume and mute state changes

#### `AudioManager`
Main implementation that:
- Manages audio source pooling for performance
- Handles resource loading via `IAssetsManager`
- Provides separate channels for sound and music
- Supports both synchronous and async operations (with UniTask)
- Implements audio source recycling

### Design Patterns

- **Object Pooling**: Reuses `AudioSource` components to minimize allocations
- **Dependency Injection**: Supports VContainer, Zenject, and custom DI
- **Interface Segregation**: Separates settings from operations
- **Resource Management**: Integrates with TheOne.ResourceManagement for asset loading

### Code Style & Conventions

- **Namespace**: All code under `TheOne.Audio` namespace
- **Null Safety**: Uses `#nullable enable` directive
- **Interfaces**: Prefixed with `I` (e.g., `IAudioManager`)
- **Private Fields**: Use camelCase with no prefix
- **Properties**: Use PascalCase
- **Methods**: Use PascalCase for public, camelCase for private
- **Async Support**: Conditional compilation for UniTask support

### Integration with DI Frameworks

#### VContainer
```csharp
builder.RegisterAudioManager();
```

#### Zenject
```csharp
Container.BindAudioManager();
```

#### Custom DI
```csharp
container.Register<IAudioManager, AudioManager>();
container.Register<IAudioManagerSettings>(container.Resolve<IAudioManager>());
```

## Integration with Entity System

### Managed Sound Components

```csharp
using TheOne.Audio;
using TheOne.Entities;
using UnityEngine;

// Component that automatically registers AudioSources with IAudioManagerSettings
[RequireComponent(typeof(AudioSource))]
internal sealed class ManagedSound : Component
{
    protected override void OnInstantiate()
    {
        // Register all AudioSources on this GameObject with the audio manager
        var audioManager = this.Container.Resolve<IAudioManagerSettings>();
        this.GetComponents<AudioSource>().ForEach(audioManager.RegisterSound);
    }
    
    protected override void OnCleanup()
    {
        // Unregister when entity is destroyed
        var audioManager = this.Container.Resolve<IAudioManagerSettings>();
        this.GetComponents<AudioSource>().ForEach(audioManager.UnregisterSound);
    }
}

// Usage: Add ManagedSound component to any entity with AudioSource
// The AudioSource will automatically respect global sound settings
```

### Sound Effects on Events

```csharp
public class PlaySound : Component
{
    [SerializeField] private string soundName;
    
    private IAudioManager audioManager;
    
    protected override void OnInstantiate()
    {
        this.audioManager = this.Container.Resolve<IAudioManager>();
    }
    
    public void OnCollected()
    {
        // Play sound when item is collected
        this.audioManager.PlaySound(this.soundName);
    }
    
    public void OnHit()
    {
        // Play different sound for hit event
        this.audioManager.PlaySound("hit_sound");
    }
}
```

## Performance Considerations

- Audio sources are pooled and reused to minimize garbage collection
- Resources are loaded on-demand and cached
- Separate pools for sound effects and music prevent interference
- Automatic cleanup of finished audio sources
- Use `RegisterSound`/`UnregisterSound` for managed volume control

## Best Practices

1. **Resource Organization**: Place audio files in Resources or Addressables
2. **Volume Levels**: Keep master volumes between 0.5-0.8 for headroom
3. **Memory Management**: Unload unused audio clips when switching scenes
4. **Channel Separation**: Use sounds for SFX, music for background audio
5. **Error Handling**: Always check if audio clips exist before playing
6. **Entity Integration**: Use ManagedSound component for automatic volume management
7. **Event-driven Audio**: Trigger sounds from gameplay events and component callbacks