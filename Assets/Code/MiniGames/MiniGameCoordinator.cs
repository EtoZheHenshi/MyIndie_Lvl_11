using System;
using Code.Pause;
using UnityEngine;

namespace Code.MiniGames
{
    public class MiniGameCoordinator : MonoBehaviour
    {
        [SerializeField] private GameObject _inputBlocker;
        [SerializeField] private GameObject _transitionBlocker;
        [SerializeField] private PauseService _pauseService;
        private MiniGameWindow _currentWindow;
        private MiniGameWindow _forcedWindow;
        private float _forcedOpenRemaining;

        public bool HasOpenWindow => _currentWindow != null;
        public MiniGameWindow CurrentWindow => _currentWindow;
        public bool IsOpeningForcedWindow => _forcedWindow != null;

        public event Action<MiniGameWindow> WindowOpened;
        public event Action<MiniGameWindow> WindowCompleted;

        public bool CanInteractWithApartment =>
            !HasOpenWindow && !IsOpeningForcedWindow && !_pauseService.IsPaused;

        private void OnEnable()
        {
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        public void Open(MiniGameWindow window)
        {
            if (_pauseService.IsPaused || window == null ||
                !window.isActiveAndEnabled || HasOpenWindow || IsOpeningForcedWindow)
                return;

            _currentWindow = window;
            RefreshInputBlocker();
            _currentWindow.Show();
            if (_currentWindow == window)
                WindowOpened?.Invoke(window);
        }

        public bool OpenForced(MiniGameWindow window, float delay)
        {
            if (_pauseService.IsSessionEnded || IsOpeningForcedWindow || window == null ||
                !window.isActiveAndEnabled || window == _currentWindow)
                return false;

            _forcedWindow = window;
            _forcedOpenRemaining = Mathf.Max(0f, delay);
            if (_currentWindow != null)
                _currentWindow.SetInputEnabled(false);
            RefreshInputBlocker();
            return true;
        }

        public void CancelForcedOpen()
        {
            _forcedWindow = null;
            _forcedOpenRemaining = 0f;
            if (_currentWindow != null)
                _currentWindow.SetInputEnabled(!_pauseService.IsPaused);
            RefreshInputBlocker();
        }

        private void Update()
        {
            if (!IsOpeningForcedWindow)
                return;
            if (_pauseService.IsSessionEnded)
            {
                CancelForcedOpen();
                return;
            }
            if (_pauseService.IsPaused)
                return;

            _forcedOpenRemaining -= Time.deltaTime;
            if (_forcedOpenRemaining > 0f)
                return;

            MiniGameWindow window = _forcedWindow;
            _forcedWindow = null;
            CloseCurrent();
            Open(window);
        }

        public void CloseCurrent()
        {
            if (_currentWindow != null)
                _currentWindow.Hide();

            _currentWindow = null;
            RefreshInputBlocker();
        }

        public void CompleteCurrent(MiniGameWindow window)
        {
            if (_currentWindow == null || _currentWindow != window)
                return;

            CloseCurrent();
            WindowCompleted?.Invoke(window);
        }

        private void OnDisable()
        {
            if (_pauseService != null)
                _pauseService.PauseChanged -= OnPauseChanged;
            CancelForcedOpen();
            CloseCurrent();
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (_currentWindow != null)
                _currentWindow.SetInputEnabled(!isPaused && !IsOpeningForcedWindow);

            RefreshInputBlocker();
        }

        private void RefreshInputBlocker()
        {
            if (_inputBlocker != null)
                _inputBlocker.SetActive(HasOpenWindow || IsOpeningForcedWindow || _pauseService.IsPaused);
            if (_transitionBlocker != null)
                _transitionBlocker.SetActive(IsOpeningForcedWindow);
        }
    }
}
