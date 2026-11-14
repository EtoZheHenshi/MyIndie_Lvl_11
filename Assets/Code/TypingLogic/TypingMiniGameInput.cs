using Code.MiniGames;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Code.TypingLogic
{
    public class TypingMiniGameInput : MiniGameInput
    {
        private TextComparer _textComparer;
        private Keyboard _keyboard;

        [Inject]
        public void Construct(TextComparer textComparer)
        {
            _textComparer = textComparer;
        }

        protected override void Subscribe()
        {
            if (_keyboard != null)
                return;

            _keyboard = Keyboard.current;

            if (_keyboard != null)
                _keyboard.onTextInput += CheckInputSymbol;
        }

        protected override void Unsubscribe()
        {
            if (_keyboard != null)
                _keyboard.onTextInput -= CheckInputSymbol;

            _keyboard = null;
        }

        private void CheckInputSymbol(char symbol)
        {
            if (!Application.isFocused || char.IsControl(symbol))
                return;

            _textComparer.Compare(symbol);
        }
    }
}
