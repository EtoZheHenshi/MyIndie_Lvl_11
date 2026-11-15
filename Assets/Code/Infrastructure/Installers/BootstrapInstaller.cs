using Zenject;

namespace Code.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller, IInitializable
    {
        private SceneNavigationService _navigation;

        [Inject]
        public void Construct(SceneNavigationService navigation)
        {
            _navigation = navigation;
        }

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<BootstrapInstaller>()
                .FromInstance(this)
                .AsSingle();
        }

        public void Initialize()
        {
            _navigation.OpenMenu();
        }
    }
}
