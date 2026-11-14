using Code.TypingLogic;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Installers
{
    public class MainSceneInstaller : MonoInstaller, IInitializable
    {
        [SerializeField] private DisplayText _displayText;
        [SerializeField] private TextWriter _textWriter;
        [SerializeField, Min(1)] private int _rowMaxLength = 18;

        public override void InstallBindings()
        {
            Container
                .BindInterfacesTo<MainSceneInstaller>()
                .FromInstance(this)
                .AsSingle();

            BindTypingComponent();
        }

        private void BindTypingComponent()
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
                .Bind<TextWriter>()
                .FromInstance(_textWriter)
                .AsSingle();
        }

        public void Initialize()
        {
        }
    }
}
