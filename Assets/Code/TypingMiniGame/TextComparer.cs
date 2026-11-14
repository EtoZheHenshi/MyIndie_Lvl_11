using UnityEngine;

namespace Code.TypingLogic
{
    public class TextComparer
    {
        private readonly TextForPlayer _textForPlayer;

        public TextComparer(DisplayText displayText, int rowMaxLength)
        {
            _textForPlayer = new TextForPlayer(displayText, rowMaxLength);
        }

        public void Compare(char symbol)
        {
            if (_textForPlayer.IsCompleted)
                return;

            if (symbol == _textForPlayer.GetCurrentSymbol())
            {
                _textForPlayer.MoveToNextSymbol();
            }
            else
            {
                Debug.Log("Not comparing symbol");
            }
        }
    }
}
