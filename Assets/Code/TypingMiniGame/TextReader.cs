using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Code.TypingLogic
{
    public class TextReader
    {
        private const string TextPath = "TypingMiniGame/Textes/text900";
        private readonly List<string> _rows = new List<string>();

        public int RowCount => _rows.Count;
        public int CharacterCount { get; }

        public TextReader(int rowMaxLength) : this(LoadText(), rowMaxLength)
        {
        }

        public TextReader(string originalText, int rowMaxLength)
        {
            if (rowMaxLength < 1)
                throw new ArgumentOutOfRangeException(nameof(rowMaxLength));

            string text = Regex.Replace(originalText ?? string.Empty, @"\s+", " ").Trim();
            CharacterCount = text.Length > 0 ? text.Length + 1 : 0;
            if (text.Length > 0)
                SplitTextOnRows(text + " ", rowMaxLength);
        }

        public string GetRow(int rowIndex)
        {
            return rowIndex >= 0 && rowIndex < _rows.Count ? _rows[rowIndex] : string.Empty;
        }

        private static string LoadText()
        {
            TextAsset asset = Resources.Load<TextAsset>(TextPath);
            if (asset == null)
                throw new InvalidOperationException("Typing text resource not found: " + TextPath);
            return asset.text;
        }

        private void SplitTextOnRows(string text, int rowMaxLength)
        {
            int start = 0;
            while (start < text.Length)
            {
                int end = Math.Min(start + rowMaxLength - 1, text.Length - 1);
                int boundary = end;
                while (boundary >= start && text[boundary] != ' ')
                    boundary--;

                // An indivisible long word occupies a row of its own.
                if (boundary < start)
                {
                    boundary = end;
                    while (text[boundary] != ' ')
                        boundary++;
                }

                _rows.Add(text.Substring(start, boundary - start + 1));
                start = boundary + 1;
            }
        }
    }
}
