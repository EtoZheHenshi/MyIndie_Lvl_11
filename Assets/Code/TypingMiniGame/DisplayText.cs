using System;
using TMPro;
using UnityEngine;

namespace Code.TypingLogic
{
    public class DisplayText : MonoBehaviour
    {
        public TMP_Text[] Rows;

        private TMP_Text _currentRow;
        private int _typedCharacterCount;

        public int RowCapacity => Rows.Length;

        public void ValidateRows()
        {
            if (Rows == null || Rows.Length < 2)
                throw new InvalidOperationException("DisplayText requires at least two Rows.");

            for (int i = 0; i < Rows.Length; i++)
            {
                if (Rows[i] == null)
                    throw new InvalidOperationException("Assign a text component to every DisplayText row.");
                for (int j = 0; j < i; j++)
                    if (Rows[i] == Rows[j])
                        throw new InvalidOperationException("DisplayText rows must use separate text components.");
            }
        }

        public void SetTypedCharacterCount(int count)
        {
            _typedCharacterCount = count;
            if (_currentRow == null || !_currentRow.isActiveAndEnabled || _currentRow.mesh == null)
                return;

            // Also refresh pending text/layout changes before accessing the mesh.
            if (_currentRow.havePropertiesChanged)
                _currentRow.ForceMeshUpdate();

            ApplyCharacterColors(_currentRow.textInfo);
            _currentRow.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        public void Render(TextReader reader, int firstRow, int currentRow, bool completed)
        {
            DetachCurrentRow();
            _typedCharacterCount = 0;
            int visibleCount = Math.Min(Rows.Length, Math.Min(reader.RowCount - firstRow,
                currentRow - firstRow + 2));

            for (int i = 0; i < Rows.Length; i++)
            {
                Rows[i].text = i < visibleCount ? reader.GetRow(firstRow + i) : string.Empty;
                Rows[i].color = completed || firstRow + i < currentRow ? Color.black : Color.white;
                Rows[i].enableVertexGradient = false;
                Rows[i].maxVisibleCharacters = int.MaxValue;
                Rows[i].richText = false;
            }

            if (completed)
                return;

            _currentRow = Rows[currentRow - firstRow];
            // TMP regenerates vertex colors when layout, font size or text changes.
            _currentRow.OnPreRenderText += ApplyCharacterColors;
        }

        private void ApplyCharacterColors(TMP_TextInfo textInfo)
        {
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo character = textInfo.characterInfo[i];
                if (!character.isVisible)
                    continue;

                // Character indices refer to the original UTF-16 string, including spaces.
                Color32 color = character.index + character.stringLength <= _typedCharacterCount
                    ? Color.black : Color.white;
                Color32[] colors = textInfo.meshInfo[character.materialReferenceIndex].colors32;
                for (int vertex = 0; vertex < 4; vertex++)
                    colors[character.vertexIndex + vertex] = color;
            }
        }

        private void DetachCurrentRow()
        {
            if (_currentRow != null)
                _currentRow.OnPreRenderText -= ApplyCharacterColors;
            _currentRow = null;
        }

        private void OnDestroy()
        {
            DetachCurrentRow();
        }
    }
}
