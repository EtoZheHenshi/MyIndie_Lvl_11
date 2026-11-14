using UnityEngine;

namespace Code.MiniGames
{
    public class MiniGameCoordinator : MonoBehaviour
    {
        [SerializeField] private GameObject _inputBlocker;
        private MiniGameWindow _currentWindow;

        public bool HasOpenWindow => _currentWindow != null;

        private void Awake()
        {
            _inputBlocker.SetActive(false);
        }

        public void Open(MiniGameWindow window)
        {
            if (window == null || !window.isActiveAndEnabled || HasOpenWindow)
                return;

            _currentWindow = window;
            _inputBlocker.SetActive(true);
            _currentWindow.Show();
        }

        public void CloseCurrent()
        {
            if (_currentWindow != null)
                _currentWindow.Hide();

            _currentWindow = null;
            if (_inputBlocker != null)
                _inputBlocker.SetActive(false);
        }

        private void OnDisable()
        {
            CloseCurrent();
        }
    }
}
