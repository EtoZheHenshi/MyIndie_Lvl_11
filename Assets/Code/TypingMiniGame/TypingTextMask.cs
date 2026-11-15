using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Code.TypingLogic
{
    public class TypingTextMask
    {
        private readonly int _seed = Environment.TickCount;
        private readonly Dictionary<int, RowOrder> _orders = new Dictionary<int, RowOrder>();

        public string Build(string original, int rowIndex, int typedCount,
            float visiblePercent, string spriteTag, float characterWidth,
            Color typedColor, Color untypedColor, int errorIndex, Color errorColor)
        {
            RowOrder order = GetOrder(original, rowIndex);
            int visibleCount = Mathf.CeilToInt(order.CharacterCount * Mathf.Clamp01(visiblePercent / 100f));
            typedCount = Mathf.Clamp(typedCount, 0, original.Length);
            var result = new StringBuilder();
            result.Append("<mspace=");
            result.Append(characterWidth.ToString(CultureInfo.InvariantCulture));
            result.Append("em><color=#");
            result.Append(ColorUtility.ToHtmlStringRGBA(typedColor));
            result.Append('>');
            bool untypedPartStarted = false;

            for (int i = 0; i < original.Length; i++)
            {
                int symbolLength = 1;
                if (char.IsHighSurrogate(original[i]) && i + 1 < original.Length && char.IsLowSurrogate(original[i + 1]))
                    symbolLength = 2;

                bool isTyped = i + symbolLength <= typedCount;
                if (!isTyped && !untypedPartStarted)
                {
                    result.Append("</color><color=#");
                    result.Append(ColorUtility.ToHtmlStringRGBA(untypedColor));
                    result.Append('>');
                    untypedPartStarted = true;
                }

                bool isHidden = !isTyped && order.Ranks[i] >= visibleCount;
                bool isError = !isTyped && i == errorIndex && char.IsLetter(original, i);
                if (isError)
                    result.Append("<color=#").Append(ColorUtility.ToHtmlStringRGBA(errorColor)).Append('>');
                if (isHidden)
                    result.Append(isError ? spriteTag.Replace("<sprite ",
                        "<sprite tint=1 color=#" + ColorUtility.ToHtmlStringRGBA(errorColor) + " ") : spriteTag);
                else
                {
                    // Treat original text as literal text, including angle brackets.
                    result.Append("<noparse>");
                    result.Append(original, i, symbolLength);
                    result.Append("</noparse>");
                }
                if (isError)
                    result.Append("</color>");
                i += symbolLength - 1;
            }
            result.Append("</color></mspace>");
            return result.ToString();
        }

        private RowOrder GetOrder(string original, int rowIndex)
        {
            if (_orders.TryGetValue(rowIndex, out RowOrder existing) && existing.Original == original)
                return existing;

            var positions = new List<int>();
            var ranks = new int[original.Length];
            for (int i = 0; i < original.Length; i++)
            {
                ranks[i] = -1;
                if (char.IsLetterOrDigit(original[i]))
                    positions.Add(i);
            }
            var random = new System.Random(unchecked(_seed ^ rowIndex * 397));
            for (int i = positions.Count - 1; i > 0; i--)
            {
                int other = random.Next(i + 1);
                int temporary = positions[i];
                positions[i] = positions[other];
                positions[other] = temporary;
            }
            for (int rank = 0; rank < positions.Count; rank++)
                ranks[positions[rank]] = rank;

            var order = new RowOrder(original, ranks, positions.Count);
            _orders[rowIndex] = order;
            return order;
        }

        private class RowOrder
        {
            public readonly string Original;
            public readonly int[] Ranks;
            public readonly int CharacterCount;

            public RowOrder(string original, int[] ranks, int characterCount)
            {
                Original = original;
                Ranks = ranks;
                CharacterCount = characterCount;
            }
        }
    }
}
