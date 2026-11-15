using Code.MiniGames;
using UnityEngine;

namespace Code.LentaMiniGame
{
    public class LentaMiniGameInput : MiniGameInput
    {
        [SerializeField] private ContentController _content;

        protected override void Subscribe()
        {
            _content.SetInputEnabled(true);
        }

        protected override void Unsubscribe()
        {
            if (_content != null)
                _content.SetInputEnabled(false);
        }
    }
}
