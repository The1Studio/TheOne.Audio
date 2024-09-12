#if UNIT_VCONTAINER
#nullable enable
namespace UniT.Audio.DI
{
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
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