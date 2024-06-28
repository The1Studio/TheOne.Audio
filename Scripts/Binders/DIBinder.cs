#if UNIT_DI
#nullable enable
namespace UniT.Audio
{
    using UniT.DI;
    using UniT.Logging;
    using UniT.ResourceManagement;

    public static class DIBinder
    {
        public static void AddAudioManager(this DependencyContainer container)
        {
            if (container.Contains<IAudioManager>()) return;
            container.AddLoggerManager();
            container.AddResourceManagers();
            container.AddInterfaces<AudioManager>();
        }
    }
}
#endif