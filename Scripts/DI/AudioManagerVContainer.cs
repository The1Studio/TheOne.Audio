#if THEONE_VCONTAINER
#nullable enable
namespace TheOne.Audio.DI
{
    using TheOne.Logging.DI;
    using TheOne.ResourceManagement.DI;
    using VContainer;

    public static class AudioManagerVContainer
    {
        public static void RegisterAudioManager(this IContainerBuilder builder)
        {
            if (builder.Exists(typeof(IAudioManager), true)) return;
            builder.RegisterLoggerManager();
            builder.RegisterAssetsManager();
            builder.Register<AudioManager>(Lifetime.Singleton).AsImplementedInterfaces();
        }
    }
}
#endif