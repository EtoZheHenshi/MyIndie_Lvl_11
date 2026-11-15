using Code.Motivation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI
{
    public class MotivationView : MonoBehaviour
    {
        [SerializeField] private Image _fill;
        [SerializeField] private TMP_Text _valueText;

        private MotivationService _motivationService;
        private bool _isSubscribed;

        [Inject]
        public void Construct(MotivationService motivationService)
        {
            _motivationService = motivationService;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Start()
        {
            // Synchronize after every scene object's Awake has run.
            Refresh(_motivationService.Value);
        }

        private void OnDisable()
        {
            if (!_isSubscribed)
                return;

            if (_motivationService != null)
                _motivationService.ValueChanged -= Refresh;
            _isSubscribed = false;
        }

        private void Subscribe()
        {
            // Injection and OnEnable can occur in either order.
            if (_motivationService == null || _isSubscribed)
                return;

            _motivationService.ValueChanged += Refresh;
            _isSubscribed = true;
            Refresh(_motivationService.Value);
        }

        private void Refresh(float value)
        {
            _fill.fillAmount = Mathf.Clamp01(value / 100f);
            _valueText.SetText("Мотивация: {0}%", Mathf.CeilToInt(value));
        }
    }
}
