using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.TypingLogic
{
    public class DisplayText : MonoBehaviour
    {
        public TMP_Text[] Rows;

        [Header("Text visibility")]
        [SerializeField, Range(0f, 100f)] private float _visiblePercent = 100f;

        [Header("Text colors")]
        [SerializeField] private Color _typedColor = Color.black;
        [SerializeField] private Color _untypedColor = new Color32(57, 110, 172, 255);

        [Header("Typing cursor")]
        [SerializeField] private Image _cursor;
        [SerializeField] private Color _cursorColor = Color.black;
        [SerializeField, Min(0.05f)] private float _cursorBlinkDuration = 0.45f;

        [Header("Input error")]
        [SerializeField] private Image _errorHighlight;
        [SerializeField] private Image _errorUnderline;
        [SerializeField] private CanvasGroup _errorOpacity;
        [SerializeField] private Color _errorColor = new Color32(204, 16, 43, 255);
        [SerializeField, Min(0.05f)] private float _errorDuration = 0.56f / 1.5f;

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
        private Color _lastTypedColor;
        private Color _lastUntypedColor;
        private Sequence _errorTween;
        private Tween _cursorTween;
        private bool _cursorDirty;
        private int _errorSymbolIndex = -1;
        private bool _errorRestartPending;

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
            ClearInputError();
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
            ClearInputError();
            _typedCharacterCount = count;
            if (_reader == null || _completed)
                return;
            RenderRow(_currentRow - _firstVisibleRow);
            _cursorDirty = true;
        }

        public void SetVisibility(float percent)
        {
            _visiblePercent = Mathf.Clamp(percent, 0f, 100f);
            RefreshRows();
        }

        public void ShowInputError()
        {
            if (!isActiveAndEnabled || _reader == null || _completed ||
                _errorHighlight == null || _errorUnderline == null || _errorOpacity == null)
                return;
            // Do not keep postponing the pulse if key repeat is faster than the short gap.
            if (_errorRestartPending)
                return;

            if (!PositionCurrentCell(_errorHighlight.rectTransform, false))
                return;
            HideCursor();
            bool restarting = _errorTween != null && _errorTween.IsActive();
            _errorTween?.Kill();
            bool isLetter = char.IsLetter(_reader.GetRow(_currentRow), _typedCharacterCount);
            _errorHighlight.enabled = !isLetter;
            _errorHighlight.color = new Color(_errorColor.r, _errorColor.g, _errorColor.b, 0.4f);
            _errorUnderline.color = _errorColor;
            _errorTween = DOTween.Sequence();
            if (restarting)
            {
                _errorRestartPending = true;
                _errorSymbolIndex = -1;
                _errorHighlight.gameObject.SetActive(false);
                RenderRow(_currentRow - _firstVisibleRow);
                _errorTween.AppendInterval(0.04f).AppendCallback(ShowErrorPulse);
            }
            else
                ShowErrorPulse();
            _errorTween.AppendInterval(_errorDuration).OnComplete(ClearInputError);
        }

        private void ShowErrorPulse()
        {
            _errorRestartPending = false;
            _errorSymbolIndex = _typedCharacterCount;
            RenderRow(_currentRow - _firstVisibleRow);
            _errorHighlight.gameObject.SetActive(true);
            _errorOpacity.alpha = 1f;
        }

        private bool PositionCurrentCell(RectTransform marker, bool underline)
        {
            if (_reader == null || _completed || _currentRow < _firstVisibleRow ||
                _currentRow - _firstVisibleRow >= Rows.Length)
                return false;

            TMP_Text row = Rows[_currentRow - _firstVisibleRow];
            row.ForceMeshUpdate();
            // TMP has one character entry per Unicode code point, including sprite placeholders.
            string original = _reader.GetRow(_currentRow);
            int characterIndex = 0;
            for (int i = 0; i < _typedCharacterCount; i++, characterIndex++)
                if (char.IsHighSurrogate(original[i]) && i + 1 < original.Length &&
                    char.IsLowSurrogate(original[i + 1]))
                {
                    if (i + 1 >= _typedCharacterCount)
                        break;
                    i++;
                }
            if (characterIndex >= row.textInfo.characterCount)
                return false;

            TMP_CharacterInfo character = row.textInfo.characterInfo[characterIndex];
            float width = row.fontSize * _characterWidth;
            float height = Mathf.Max(row.fontSize * 1.3f, character.ascender - character.descender);
            marker.SetParent(row.rectTransform, false);
            marker.anchorMin = marker.anchorMax = row.rectTransform.pivot;
            marker.pivot = new Vector2(0.5f, 0.5f);
            marker.anchoredPosition = new Vector2(character.xAdvance - width * 0.5f,
                (character.ascender + character.descender) * 0.5f -
                (underline ? height * 0.5f - 2.5f : 0f));
            marker.sizeDelta = new Vector2(width + 4f, underline ? 5f : height);
            return true;
        }

        private void RefreshCursor()
        {
            _cursorDirty = false;
            HideCursor();
            if (_cursor == null || !isActiveAndEnabled ||
                _errorSymbolIndex >= 0 || _errorRestartPending ||
                !PositionCurrentCell(_cursor.rectTransform, true))
                return;

            _cursor.color = _cursorColor;
            _cursor.gameObject.SetActive(true);
            _cursorTween = _cursor.DOFade(0.1f, _cursorBlinkDuration)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void HideCursor()
        {
            _cursorTween?.Kill();
            _cursorTween = null;
            if (_cursor != null)
                _cursor.gameObject.SetActive(false);
        }

        private void ClearInputError()
        {
            bool hadError = _errorSymbolIndex >= 0;
            _errorSymbolIndex = -1;
            _errorRestartPending = false;
            _errorTween?.Kill();
            _errorTween = null;
            if (_errorHighlight != null)
                _errorHighlight.gameObject.SetActive(false);
            if (hadError && _reader != null && !_completed && isActiveAndEnabled)
                RenderRow(_currentRow - _firstVisibleRow);
            _cursorDirty = true;
        }

        private void OnEnable()
        {
            RefreshRows();
        }

        private void LateUpdate()
        {
            if (_reader != null && (_lastVisiblePercent != _visiblePercent ||
                _lastTypedColor != _typedColor || _lastUntypedColor != _untypedColor))
                RefreshRows();
            if (_cursorDirty)
                RefreshCursor();
        }

        private void OnDisable()
        {
            ClearInputError();
            HideCursor();
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
            _lastTypedColor = _typedColor;
            _lastUntypedColor = _untypedColor;
            for (int i = 0; i < Rows.Length; i++)
                RenderRow(i);
            _cursorDirty = true;
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
                label.text = _mask.Build(original, sourceRow, typedCount, _visiblePercent,
                    spriteTag, _characterWidth, _typedColor, _untypedColor,
                    sourceRow == _currentRow ? _errorSymbolIndex : -1, _errorColor);
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
