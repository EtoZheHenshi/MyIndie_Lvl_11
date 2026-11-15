using System;
using Code.Motivation;
using Code.Audio;

namespace Code.TypingLogic
{
    public class TextComparer
    {
        private readonly TextForPlayer _textForPlayer;
        private readonly MotivationService _motivationService;
        private readonly DisplayText _displayText;
        private readonly AudioService _audio;

        public event Action CorrectSymbolTyped;

        public int TypedCharacterCount => _textForPlayer.TypedCharacterCount;
        public int TotalCharacterCount => _textForPlayer.TotalCharacterCount;
        public float NormalizedProgress => _textForPlayer.NormalizedProgress;

        public TextComparer(DisplayText displayText, int rowMaxLength,
            MotivationService motivationService, AudioService audio)
        {
            _motivationService = motivationService;
            _displayText = displayText;
            _audio = audio;
            _textForPlayer = new TextForPlayer(displayText, rowMaxLength);
            if (_textForPlayer.IsCompleted)
                _motivationService.CompleteTyping();
        }

        public void Compare(char symbol)
        {
            if (_textForPlayer.IsCompleted || _motivationService.IsDepleted)
                return;

            _audio.Play("SFX/TypingAudioCue");
            if (symbol == _textForPlayer.GetCurrentSymbol())
            {
                _textForPlayer.MoveToNextSymbol();
                _motivationService.RewardCorrectSymbol();
                if (_textForPlayer.IsCompleted)
                    _motivationService.CompleteTyping();
                CorrectSymbolTyped?.Invoke();
            }
            else
            {
                _audio.Play("SFX/OnTypeError");
                _displayText.ShowInputError();
            }
        }
    }
}
