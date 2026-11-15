using Code.MiniGames;
using UnityEngine;

namespace Code.SandwichMiniGame
{
    public class SandwichMiniGameWindow : MiniGameWindow
    {
        [SerializeField] private DropArea _dropArea;
        [SerializeField] private Draggable[] _ingredients;

        protected override void OnEnable()
        {
            base.OnEnable();
            _dropArea.OnGameFinished += OnGameFinished;
        }

        protected override void OnDisable()
        {
            if (_dropArea != null)
                _dropArea.OnGameFinished -= OnGameFinished;

            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            _dropArea.ResetGame();

            foreach (Draggable ingredient in _ingredients)
                ingredient.ResetIngredient();
        }

        private void OnGameFinished()
        {
            CompleteWindow();
        }
    }
}
