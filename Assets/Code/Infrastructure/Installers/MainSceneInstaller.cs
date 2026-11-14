using Code.TypingLogic;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Installers
{
    public class MainSceneInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private DisplayText _displayText;
        [SerializeField] private TypingMiniGameInput _typingMiniGameInput;
        [SerializeField, Min(1)] private int _rowMaxLength = 18;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<MainSceneInstaller>()
                .FromInstance(this)
                .AsSingle();

            BindTypingMiniGame();
        }

        private void BindTypingMiniGame()
        {
            Container
                .Bind<DisplayText>()
                .FromInstance(_displayText)
                .AsSingle();

            Container
                .Bind<TextComparer>()
                .ToSelf()
                .AsSingle()
                .WithArguments(_rowMaxLength);

            Container
                .Bind<TypingMiniGameInput>()
                .FromInstance(_typingMiniGameInput)
                .AsSingle();
        }

        public void Initialize()
        {
        }
    }
}
