using Code.MiniGames;
using UnityEngine;
using UnityEngine.UI;

namespace Code.MotherCallMiniGame
{
    public class MotherCallMiniGameWindow : MiniGameWindow
    {
        [SerializeField] private BubbleController _bubbles;
        [SerializeField] private GameObject _callButtons;
        [SerializeField] private Button _acceptButton;
        [SerializeField] private Button _declineButton;
        [SerializeField] private CallButtonFeedback _acceptFeedback;
        [SerializeField] private CallButtonFeedback _declineFeedback;
        private bool _inputEnabled;

        public bool IsCallAccepted { get; private set; }

        protected override void OnEnable()
        {
            base.OnEnable();
            _bubbles.OnGameFinished += OnGameFinished;
            _acceptButton.onClick.AddListener(AcceptCall);
        }

        protected override void OnDisable()
        {
            if (_bubbles != null)
                _bubbles.OnGameFinished -= OnGameFinished;
            if (_acceptButton != null)
                _acceptButton.onClick.RemoveListener(AcceptCall);
            base.OnDisable();
        }

        protected override void OnBeforeShow()
        {
            IsCallAccepted = false;
            _inputEnabled = false;
            _bubbles.ResetGame();
            _callButtons.SetActive(true);
        }

        public void SetInteractionEnabled(bool enabled)
        {
            _inputEnabled = enabled;
            if (_bubbles != null)
                _bubbles.SetInputEnabled(enabled && IsCallAccepted);
            if (_acceptButton != null)
                _acceptButton.interactable = enabled && !IsCallAccepted;
            if (_declineButton != null)
                _declineButton.interactable = enabled && !IsCallAccepted;
            if (_acceptFeedback != null)
                _acceptFeedback.enabled = enabled && !IsCallAccepted;
            if (_declineFeedback != null)
                _declineFeedback.enabled = enabled && !IsCallAccepted;
        }

        private void AcceptCall()
        {
            if (!IsOpen || !_inputEnabled || IsCallAccepted)
                return;

            IsCallAccepted = true;
            _callButtons.SetActive(false);
            _bubbles.StartConversation();
            SetInteractionEnabled(true);
        }

        private void OnGameFinished()
        {
            CompleteWindow();
        }
    }
}
