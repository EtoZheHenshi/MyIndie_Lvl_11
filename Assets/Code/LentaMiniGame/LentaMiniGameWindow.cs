using Code.MiniGames;
using UnityEngine;

namespace Code.LentaMiniGame
{
    public class LentaMiniGameWindow : MiniGameWindow
    {
        [SerializeField] private ContentController _content;

        protected override void OnEnable()
        {
            base.OnEnable();
            _content.GameFinished += OnGameFinished;
        }

        protected override void OnDisable()
        {
            if (_content != null)
                _content.GameFinished -= OnGameFinished;
            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            _content.ResetGame();
        }

        private void OnGameFinished()
        {
            CompleteWindow();
        }
    }
}
