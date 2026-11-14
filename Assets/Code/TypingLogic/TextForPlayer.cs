using System;

namespace Code.TypingLogic
{
    public class TextForPlayer
    {
        private readonly DisplayText _displayText;
        private readonly TextReader _textReader;
        private int _currentRowIndex;
        private int _currentSymbolIndex;
        private int _firstVisibleRowIndex;

        public bool IsCompleted => _currentRowIndex >= _textReader.RowCount;

        public TextForPlayer(DisplayText displayText, int rowMaxLength)
            : this(displayText, new TextReader(rowMaxLength))
        {
        }

        public TextForPlayer(DisplayText displayText, TextReader textReader)
        {
            _displayText = displayText;
            _textReader = textReader;
            _displayText.ValidateRows();
            RefreshDisplay();
        }

        public char GetCurrentSymbol()
        {
            if (IsCompleted)
                throw new InvalidOperationException("The typing text is complete.");

            return _textReader.GetRow(_currentRowIndex)[_currentSymbolIndex];
        }

        public void MoveToNextSymbol()
        {
            if (IsCompleted)
                return;

            _currentSymbolIndex++;
            _displayText.SetTypedCharacterCount(_currentSymbolIndex);
            if (_currentSymbolIndex < _textReader.GetRow(_currentRowIndex).Length)
                return;

            _currentRowIndex++;
            _currentSymbolIndex = 0;

            // Keep one preview row below the current row, until the source ends.
            if (!IsCompleted)
                _firstVisibleRowIndex = Math.Min(
                    Math.Max(0, _currentRowIndex - _displayText.RowCapacity + 2),
                    Math.Max(0, _textReader.RowCount - _displayText.RowCapacity));

            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            _displayText.Render(_textReader, _firstVisibleRowIndex, _currentRowIndex, IsCompleted);
        }
    }
}
