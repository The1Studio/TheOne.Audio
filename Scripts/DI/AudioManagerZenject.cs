#if UNIT_ZENJECT
#nullable enable
namespace UniT.Audio.DI
{
    using UniT.Logging.DI;
    using UniT.ResourceManagement.DI;
    using Zenject;

    public static class AudioManagerZenject
    {
        public static void BindAudioManager(this DiContainer container)
        {
            if (container.HasBinding<IAudioManager>()) return;
            container.BindLoggerManager();
            container.BindAssetsManager();
            container.BindInterfacesTo<AudioManager>().AsSingle();
        }
    }
}
#endif