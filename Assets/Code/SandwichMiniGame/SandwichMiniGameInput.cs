using Code.MiniGames;
using UnityEngine;

namespace Code.SandwichMiniGame
{
    public class SandwichMiniGameInput : MiniGameInput
    {
        [SerializeField] private DropArea _dropArea;
        [SerializeField] private Draggable[] _ingredients;

        protected override void Subscribe()
        {
            foreach (Draggable ingredient in _ingredients)
                ingredient.enabled = true;

            _dropArea.enabled = true;
        }

        protected override void Unsubscribe()
        {
            if (_dropArea != null)
                _dropArea.enabled = false;

            foreach (Draggable ingredient in _ingredients)
            {
                if (ingredient != null)
                    ingredient.enabled = false;
            }
        }
    }
}
