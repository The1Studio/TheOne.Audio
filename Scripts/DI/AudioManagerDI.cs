#if THEONE_DI
#nullable enable
namespace TheOne.Audio.DI
{
    using TheOne.DI;
    using TheOne.Logging.DI;
    using TheOne.ResourceManagement.DI;

    public static class AudioManagerDI
    {
        public static void AddAudioManager(this DependencyContainer container)
        {
            if (container.Contains<IAudioManager>()) return;
            container.AddLoggerManager();
            container.AddAssetsManager();
            container.AddInterfaces<AudioManager>();
        }
    }
}
#endif