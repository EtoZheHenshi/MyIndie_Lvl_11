using Code.MiniGames;
using UnityEngine;

namespace Code.WalkingMiniGame
{
    public class WalkingMiniGameWindow : MiniGameWindow
    {
        [SerializeField] private WalkingGame _game;

        protected override void OnEnable()
        {
            base.OnEnable();
            _game.GameFinished += OnGameFinished;
        }

        protected override void OnDisable()
        {
            if (_game != null)
                _game.GameFinished -= OnGameFinished;
            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            _game.ResetGame();
        }

        private void OnGameFinished()
        {
            CompleteWindow();
        }
    }
}
