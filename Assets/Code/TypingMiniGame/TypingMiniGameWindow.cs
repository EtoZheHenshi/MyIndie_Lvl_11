using Code.MiniGames;

namespace Code.TypingLogic
{
    public class TypingMiniGameWindow : MiniGameWindow
    {
        public bool HasBeenOpened { get; private set; }

        protected override void OnBeforeShow()
        {
            HasBeenOpened = true;
        }
    }
}
