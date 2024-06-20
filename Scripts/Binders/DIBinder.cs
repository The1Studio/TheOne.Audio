#if UNIT_DI
#nullable enable
namespace UniT.Audio
{
    using UniT.DI;

    public static class DIBinder
    {
        public static void AddAudioManager(this DependencyContainer container)
        {
            container.AddInterfaces<AudioManager>();
        }
    }
}
#endif