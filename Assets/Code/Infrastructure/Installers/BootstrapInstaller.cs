using UnityEngine.SceneManagement;
using Zenject;

namespace Code.Infrastructure.Installers
{
    public class BootstrapInstaller : MonoInstaller, IInitializable
    {
        public string FirstSceneName;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<BootstrapInstaller>()
                .FromInstance(this)
                .AsSingle();
        }

        public void Initialize()
        {
            SceneManager.LoadScene(FirstSceneName);
        }
    }
}