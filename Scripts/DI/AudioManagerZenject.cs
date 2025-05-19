#if THEONE_ZENJECT
#nullable enable
namespace TheOne.Audio.DI
{
    using TheOne.Logging.DI;
    using TheOne.ResourceManagement.DI;
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