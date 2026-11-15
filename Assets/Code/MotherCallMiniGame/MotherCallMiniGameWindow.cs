using Code.MiniGames;
using UnityEngine;

namespace Code.MotherCallMiniGame
{
    public class MotherCallMiniGameWindow : MiniGameWindow
    {
        [SerializeField] private BubbleController _bubbles;

        protected override void OnEnable()
        {
            base.OnEnable();
            _bubbles.OnGameFinished += OnGameFinished;
        }

        protected override void OnDisable()
        {
            if (_bubbles != null)
                _bubbles.OnGameFinished -= OnGameFinished;
            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            _bubbles.ResetGame();
        }

        private void OnGameFinished()
        {
            CompleteWindow();
        }
    }
}
