using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Code.TypingLogic
{
    public class DisplayText : MonoBehaviour
    {
        public TMP_Text[] Rows;

        [Header("Text visibility")]
        [SerializeField, Range(0f, 100f)] private float _visiblePercent = 100f;

        [Header("Animated placeholder")]
        [SerializeField] private TMP_SpriteAsset _maskSpriteAsset;
        [SerializeField, Min(0)] private int _firstFrame;
        [SerializeField, Min(0)] private int _lastFrame = 5;
        [SerializeField, Min(1)] private int _framesPerSecond = 12;

        [Header("Character layout")]
        [SerializeField, Min(0.1f)] private float _characterWidth = 0.65f;

        private readonly TypingTextMask _mask = new TypingTextMask();
        private TextReader _reader;
        private int _firstVisibleRow;
        private int _currentRow;
        private int _typedCharacterCount;
        private int _visibleRowCount;
        private bool _completed;
        private float _lastVisiblePercent;

        public int RowCapacity => Rows.Length;

        public void ValidateRows()
        {
            if (Rows == null || Rows.Length < 2)
                throw new InvalidOperationException("DisplayText requires at least two Rows.");
            var assignedRows = new HashSet<TMP_Text>();
            foreach (TMP_Text row in Rows)
                if (row == null || !assignedRows.Add(row))
                    throw new InvalidOperationException("Assign a separate text component to every row.");

            if (_maskSpriteAsset == null)
                throw new InvalidOperationException("Assign a Mask Sprite Asset to DisplayText.");
            int frameCount = _maskSpriteAsset.spriteCharacterTable.Count;
            if (_firstFrame < 0 || _lastFrame < _firstFrame || _lastFrame >= frameCount)
                throw new InvalidOperationException("The placeholder frame range is invalid.");
        }

        public void Render(TextReader reader, int firstRow, int currentRow, bool completed)
        {
            _reader = reader;
            _firstVisibleRow = firstRow;
            _currentRow = currentRow;
            _completed = completed;
            _typedCharacterCount = 0;
            _visibleRowCount = Math.Min(Rows.Length, Math.Min(reader.RowCount - firstRow, currentRow - firstRow + 2));
            RefreshRows();
        }

        public void SetTypedCharacterCount(int count)
        {
            _typedCharacterCount = count;
            if (_reader == null || _completed)
                return;
            RenderRow(_currentRow - _firstVisibleRow);
        }

        public void SetVisibility(float percent)
        {
            _visiblePercent = Mathf.Clamp(percent, 0f, 100f);
            RefreshRows();
        }

        private void OnEnable()
        {
            RefreshRows();
        }

        private void LateUpdate()
        {
            if (_reader != null && _lastVisiblePercent != _visiblePercent)
                RefreshRows();
        }

        private void OnDisable()
        {
            if (Rows == null)
                return;
            foreach (TMP_Text row in Rows)
                if (row != null)
                    StopSpriteAnimations(row);
        }

        private void RefreshRows()
        {
            if (_reader == null)
                return;
            _lastVisiblePercent = _visiblePercent;
            for (int i = 0; i < Rows.Length; i++)
                RenderRow(i);
        }

        private void RenderRow(int slot)
        {
            TMP_Text label = Rows[slot];
            StopSpriteAnimations(label);
            label.color = Color.white;
            label.enableVertexGradient = false;
            label.richText = true;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Overflow;
            label.spriteAsset = _maskSpriteAsset;
            label.maxVisibleCharacters = int.MaxValue;
            if (slot >= _visibleRowCount)
                label.text = string.Empty;
            else
            {
                int sourceRow = _firstVisibleRow + slot;
                string original = _reader.GetRow(sourceRow);
                bool finishedRow = _completed || sourceRow < _currentRow;
                int typedCount = finishedRow ? original.Length : sourceRow == _currentRow ? _typedCharacterCount : 0;
                string spriteTag = $"<sprite anim=\"{_firstFrame},{_lastFrame},{_framesPerSecond}\">";
                label.text = _mask.Build(original, sourceRow, typedCount, _visiblePercent, spriteTag, _characterWidth);
            }
            // Regenerate even if the string is unchanged: animations were stopped above.
            label.havePropertiesChanged = true;
            label.SetAllDirty();
        }

        private void StopSpriteAnimations(TMP_Text label)
        {
            TMP_SpriteAnimator animator = label.GetComponent<TMP_SpriteAnimator>();
            if (animator != null)
                animator.StopAllAnimations();
        }
    }
}
