using TMPro;
using UnityEngine;
using Zenject;

namespace Code.TypingLogic
{
    public class TypingProgressView : MonoBehaviour
    {
        [SerializeField] private RectTransform _fillMask;
        [SerializeField] private TMP_Text _valueText;

        private TextComparer _textComparer;
        private bool _isSubscribed;

        [Inject]
        public void Construct(TextComparer textComparer)
        {
            _textComparer = textComparer;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_textComparer == null || _isSubscribed)
                return;
            _textComparer.CorrectSymbolTyped += Refresh;
            _isSubscribed = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (!_isSubscribed)
                return;
            _textComparer.CorrectSymbolTyped -= Refresh;
            _isSubscribed = false;
        }

        private void Refresh()
        {
            float progress = Mathf.Clamp01(_textComparer.NormalizedProgress);
            _fillMask.anchorMax = new Vector2(progress, 1f);
            // Show 100% only after the final required character, including the trailing space.
            _valueText.SetText("{0}%", Mathf.FloorToInt(progress * 100f));
        }
    }
}
