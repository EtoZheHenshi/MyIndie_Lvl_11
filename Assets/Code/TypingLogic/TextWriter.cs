using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Code.TypingLogic
{
    public class TextWriter : MonoBehaviour
    {
        private TextComparer _textComparer;
        private Keyboard _keyboard;

        [Inject]
        public void Construct(TextComparer textComparer)
        {
            _textComparer = textComparer;
        }

        private void OnEnable()
        {
            _keyboard = Keyboard.current;

            if (_keyboard != null)
                _keyboard.onTextInput += CheckInputSymbol;
        }

        private void OnDisable()
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
