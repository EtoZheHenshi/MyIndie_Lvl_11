using Code.Pause;
using UnityEngine;

namespace Code.MiniGames
{
    public class MiniGameCoordinator : MonoBehaviour
    {
        [SerializeField] private GameObject _inputBlocker;
        [SerializeField] private PauseService _pauseService;
        private MiniGameWindow _currentWindow;

        public bool HasOpenWindow => _currentWindow != null;

        public bool CanInteractWithApartment =>
            !HasOpenWindow && !_pauseService.IsPaused;

        private void OnEnable()
        {
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        public void Open(MiniGameWindow window)
        {
            if (_pauseService.IsPaused || window == null ||
                !window.isActiveAndEnabled || HasOpenWindow)
                return;

            _currentWindow = window;
            RefreshInputBlocker();
            _currentWindow.Show();
        }

        public void CloseCurrent()
        {
            if (_currentWindow != null)
                _currentWindow.Hide();

            _currentWindow = null;
            RefreshInputBlocker();
        }

        private void OnDisable()
        {
            _pauseService.PauseChanged -= OnPauseChanged;
            CloseCurrent();
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (_currentWindow != null)
                _currentWindow.SetInputEnabled(!isPaused);

            RefreshInputBlocker();
        }

        private void RefreshInputBlocker()
        {
            if (_inputBlocker != null)
                _inputBlocker.SetActive(HasOpenWindow || _pauseService.IsPaused);
        }
    }
}
