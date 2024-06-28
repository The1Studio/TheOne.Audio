#if UNIT_ZENJECT
#nullable enable
namespace UniT.Audio
{
    using UniT.Logging;
    using UniT.ResourceManagement;
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindAudioManager(this DiContainer container)
        {
            if (container.HasBinding<IAudioManager>()) return;
            container.BindLoggerManager();
            container.BindResourceManagers();
            container.BindInterfacesTo<AudioManager>().AsSingle();
        }
    }
}
#endif