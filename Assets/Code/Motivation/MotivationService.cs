using System;
using Code.Pause;
using Code.TypingLogic;
using UnityEngine;

namespace Code.Motivation
{
    public class MotivationService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private TypingMiniGameWindow _typingWindow;
        [SerializeField] private DisplayText _displayText;
        [SerializeField] private PauseService _pauseService;

        [Header("Motivation")]
        [SerializeField, Range(1f, 100f)] private float _startValue = 100f;
        [SerializeField, Min(0f)] private float _gainPerSecond = 2f;
        [SerializeField, Min(0f)] private float _gainPerCorrectSymbol = 0.1f;
        [SerializeField, Min(0f)] private float _lossPerSecond = 1f;

        [Header("Text visibility")]
        [SerializeField, Range(0f, 100f)] private float _minimumVisibility = 20f;

        private int _lastVisibility = -1;

        public float Value { get; private set; }
        public float NormalizedValue => Value / 100f;
        public bool IsDepleted { get; private set; }
        public bool IsTypingCompleted { get; private set; }
        public float AdditionalLossPerSecond { get; private set; }

        public event Action<float> ValueChanged;
        public event Action Depleted;

        private void Awake()
        {
            Value = Mathf.Clamp(_startValue, 1f, 100f);
            RefreshVisibility();
        }

        private void Update()
        {
            if (IsDepleted || IsTypingCompleted || _pauseService.IsPaused)
                return;

            float changePerSecond = _typingWindow.IsOpen
                ? _gainPerSecond
                : -_lossPerSecond;

            SetValue(Value + (changePerSecond - AdditionalLossPerSecond) * Time.deltaTime);
            // Also apply Inspector changes to the visibility limit in Play Mode.
            RefreshVisibility();
        }

        public void RewardCorrectSymbol()
        {
            if (IsDepleted || IsTypingCompleted || _pauseService.IsPaused || !_typingWindow.IsOpen)
                return;

            SetValue(Value + Mathf.Max(0f, _gainPerCorrectSymbol));
        }

        public void CompleteTyping()
        {
            IsTypingCompleted = true;
        }

        public void SetAdditionalLoss(float lossPerSecond)
        {
            AdditionalLossPerSecond = Mathf.Max(0f, lossPerSecond);
        }

        private void SetValue(float value)
        {
            float nextValue = Mathf.Clamp(value, 0f, 100f);
            if (nextValue == Value)
                return;

            Value = nextValue;
            RefreshVisibility();

            if (Value <= 0f)
            {
                IsDepleted = true;
                _pauseService.EndSession();
            }

            ValueChanged?.Invoke(Value);
            if (IsDepleted)
                Depleted?.Invoke();
        }

        private void RefreshVisibility()
        {
            float visibility = Mathf.Lerp(_minimumVisibility, 100f, NormalizedValue);
            int roundedVisibility = Mathf.RoundToInt(visibility);
            if (_lastVisibility == roundedVisibility)
                return;

            _lastVisibility = roundedVisibility;
            _displayText.SetVisibility(roundedVisibility);
        }
    }
}
