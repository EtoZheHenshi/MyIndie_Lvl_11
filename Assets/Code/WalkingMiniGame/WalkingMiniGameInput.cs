using Code.MiniGames;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.WalkingMiniGame
{
    public class WalkingMiniGameInput : MiniGameInput
    {
        [SerializeField] private WalkingGame _game;

        protected override void Subscribe()
        {
            _game.SetInputEnabled(true);
        }

        protected override void Unsubscribe()
        {
            if (_game != null)
                _game.SetInputEnabled(false);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (!Application.isFocused || keyboard == null)
                return;
            if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame)
                _game.MoveLane(-1);
            else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame)
                _game.MoveLane(1);
        }
    }
}
