using Code.MiniGames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.MotherCallMiniGame
{
    public class MotherCallMiniGameInput : MiniGameInput
    {
        [SerializeField] private MotherCallMiniGameWindow _window;
        [SerializeField] private BubbleController _bubbles;

        protected override void Subscribe()
        {
            _window.SetInteractionEnabled(true);
        }

        protected override void Unsubscribe()
        {
            if (_window != null)
                _window.SetInteractionEnabled(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (!Application.isFocused || keyboard == null || !_window.IsCallAccepted)
                return;

            if (keyboard.digit1Key.wasPressedThisFrame || keyboard.numpad1Key.wasPressedThisFrame)
                _bubbles.Answer(1);
            else if (keyboard.digit2Key.wasPressedThisFrame || keyboard.numpad2Key.wasPressedThisFrame)
                _bubbles.Answer(2);
            else if (keyboard.digit3Key.wasPressedThisFrame || keyboard.numpad3Key.wasPressedThisFrame)
                _bubbles.Answer(3);
        }
    }
}
