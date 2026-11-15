using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.MotherCallMiniGame
{
    public class CallButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _visual;
        [SerializeField] private CanvasGroup _brightness;
        [SerializeField, Range(0.1f, 1f)] private float _minimumBrightness = 0.35f;
        [SerializeField, Min(0.05f)] private float _blinkDuration = 0.45f;
        [SerializeField, Min(1f)] private float _hoverScale = 1.12f;
        [SerializeField, Min(0f)] private float _shakeAngle = 4f;

        private Vector3 _baseScale;
        private Quaternion _baseRotation;
        private Tween _blink;
        private Tween _scale;
        private Tween _shake;

        private void Awake()
        {
            _baseScale = _visual.localScale;
            _baseRotation = _visual.localRotation;
        }

        private void OnEnable()
        {
            StartBlink();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!isActiveAndEnabled || !_button.IsInteractable())
                return;
            _blink?.Kill();
            _brightness.alpha = 1f;
            _scale?.Kill();
            _shake?.Kill();
            _visual.localRotation = _baseRotation;
            _scale = _visual.DOScale(_baseScale * _hoverScale, 0.15f).SetEase(Ease.OutQuad);
            _shake = _visual.DOShakeRotation(0.6f, new Vector3(0f, 0f, _shakeAngle),
                12, 90f, false).SetLoops(-1, LoopType.Restart);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!isActiveAndEnabled)
                return;
            _scale?.Kill();
            _shake?.Kill();
            _visual.localRotation = _baseRotation;
            _scale = _visual.DOScale(_baseScale, 0.12f).SetEase(Ease.OutQuad);
            StartBlink();
        }

        private void StartBlink()
        {
            _blink?.Kill();
            _brightness.alpha = 1f;
            _blink = _brightness.DOFade(_minimumBrightness, _blinkDuration)
                .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        }

        private void OnDisable()
        {
            _blink?.Kill();
            _scale?.Kill();
            _shake?.Kill();
            _brightness.alpha = 1f;
            _visual.localScale = _baseScale;
            _visual.localRotation = _baseRotation;
        }
    }
}
