#if UNIT_ZENJECT
#nullable enable
namespace UniT.Audio
{
    using Zenject;

    public static class ZenjectBinder
    {
        public static void BindAudioManager(this DiContainer container)
        {
            container.BindInterfacesTo<AudioManager>().AsSingle();
        }
    }
}
#endif