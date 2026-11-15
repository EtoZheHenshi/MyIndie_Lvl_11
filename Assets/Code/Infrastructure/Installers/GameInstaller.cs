using Code.Audio;
using Zenject;

namespace Code.Infrastructure.Installers
{
    public class GameInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            BindAudioService();
        }

        private void BindAudioService()
        {
            Container
                .Bind<AudioService>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("AudioService")
                .UnderTransform(transform)
                .AsSingle()
                .NonLazy();
        }
    }
}
