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

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            SetInputEnabled(false);
            _panel.SetActive(false);
        }

        protected virtual void OnEnable()
        {
            _openButton.onClick.AddListener(RequestOpen);
            _closeButton.onClick.AddListener(RequestClose);
        }

        protected virtual void OnDisable()
        {
            if (_openButton != null)
                _openButton.onClick.RemoveListener(RequestOpen);

            if (_closeButton != null)
                _closeButton.onClick.RemoveListener(RequestClose);

            if (IsOpen && _coordinator != null)
                _coordinator.CloseCurrent();
        }

        public void Show()
        {
            if (IsOpen)
                return;

            _panel.SetActive(true);
            IsOpen = true;
            SetInputEnabled(true);
        }

        public void Hide()
        {
            SetInputEnabled(false);
            IsOpen = false;
            if (_panel != null)
                _panel.SetActive(false);
        }

        public void SetInputEnabled(bool enabled)
        {
            if (_input != null)
                _input.enabled = IsOpen && enabled;
        }

        private void RequestOpen()
        {
            _coordinator.Open(this);
        }

        private void RequestClose()
        {
            _coordinator.CloseCurrent();
        }
    }
}
