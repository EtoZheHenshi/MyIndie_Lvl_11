using UnityEngine;
using UnityEngine.UI;

namespace Code.MiniGames
{
    public abstract class MiniGameWindow : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private MiniGameInput _input;
        [SerializeField] private MiniGameCoordinator _coordinator;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject[] _activityVisuals = new GameObject[0];

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            SetInputEnabled(false);
            _panel.SetActive(false);
            SetActivityVisuals(false);
        }

        protected virtual void OnEnable()
        {
            _openButton.onClick.AddListener(RequestOpen);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(RequestClose);
        }

        protected virtual void OnDisable()
        {
            if (_openButton != null)
                _openButton.onClick.RemoveListener(RequestOpen);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(RequestClose);

            CloseWindow();
        }

        public void Show()
        {
            if (IsOpen)
                return;

            OnBeforeShow();

            _panel.SetActive(true);
            SetActivityVisuals(true);
            IsOpen = true;
            SetInputEnabled(true);
        }

        public void Hide()
        {
            SetInputEnabled(false);
            IsOpen = false;
            SetActivityVisuals(false);
            if (_panel != null)
                _panel.SetActive(false);
        }

        public void SetInputEnabled(bool enabled)
        {
            if (_input != null)
                _input.enabled = IsOpen && enabled;
        }

        private void SetActivityVisuals(bool visible)
        {
            foreach (GameObject visual in _activityVisuals)
                if (visual != null)
                    visual.SetActive(visible);
        }

        private void RequestOpen()
        {
            _coordinator.Open(this);
        }

        private void RequestClose()
        {
            CloseWindow();
        }

        protected virtual void OnBeforeShow()
        {
        }

        protected void CloseWindow()
        {
            if (IsOpen && _coordinator != null)
                _coordinator.CloseCurrent();
        }

        protected void CompleteWindow()
        {
            if (IsOpen && _coordinator != null)
                _coordinator.CompleteCurrent(this);
        }
    }
}
