using System;
using Code.Motivation;
using UnityEngine;

namespace Code.TypingLogic
{
    public class TextComparer
    {
        private readonly TextForPlayer _textForPlayer;
        private readonly MotivationService _motivationService;

        public event Action CorrectSymbolTyped;

        public TextComparer(DisplayText displayText, int rowMaxLength,
            MotivationService motivationService)
        {
            _motivationService = motivationService;
            _textForPlayer = new TextForPlayer(displayText, rowMaxLength);
            if (_textForPlayer.IsCompleted)
                _motivationService.CompleteTyping();
        }

        public void Compare(char symbol)
        {
            if (_textForPlayer.IsCompleted || _motivationService.IsDepleted)
                return;

            if (symbol == _textForPlayer.GetCurrentSymbol())
            {
                _textForPlayer.MoveToNextSymbol();
                if (_textForPlayer.IsCompleted)
                    _motivationService.CompleteTyping();
                CorrectSymbolTyped?.Invoke();
            }
            else
            {
                Debug.Log("Not comparing symbol");
            }
        }
    }
}
