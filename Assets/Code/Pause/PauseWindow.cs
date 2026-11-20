using Code.Infrastructure;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Code.Pause
{
    public class PauseWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _menuButton;
        private bool _settingsOpen;
        private SceneNavigationService _navigation;

        [Inject]
        public void Construct(SceneNavigationService navigation)
        {
            _navigation = navigation;
        }

        private bool CanUsePauseMenu => _pauseService.IsPaused &&
            _pauseService.ShowPauseMenu && !_pauseService.IsSessionEnded && !_settingsOpen;

        public void SetSettingsOpen(bool open)
        {
            _settingsOpen = open;
            if (_pauseService != null)
                OnPauseChanged(_pauseService.IsPaused);
        }

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(TogglePause);
            _resumeButton.onClick.AddListener(Resume);
            _restartButton.onClick.AddListener(Restart);
            _menuButton.onClick.AddListener(OpenMenu);
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        private void Update()
        {
            if (Application.isFocused && Keyboard.current != null &&
                Keyboard.current.escapeKey.wasPressedThisFrame)
                TogglePause();
        }

        private void OnDisable()
        {
            // UI objects can be destroyed before this component during scene teardown.
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(TogglePause);
            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(Resume);
            if (_restartButton != null)
                _restartButton.onClick.RemoveListener(Restart);
            if (_menuButton != null)
                _menuButton.onClick.RemoveListener(OpenMenu);

            if (_pauseService != null)
                _pauseService.PauseChanged -= OnPauseChanged;

            if (_panel != null)
                _panel.SetActive(false);
        }

        private void TogglePause()
        {
            if (_settingsOpen || _pauseService.IsSessionEnded ||
                (_navigation != null && _navigation.IsLoading) ||
                (_pauseService.IsPaused && !_pauseService.ShowPauseMenu))
                return;
            if (_pauseService.IsPaused)
                _pauseService.Resume();
            else
                _pauseService.Pause();
        }

        private void Resume()
        {
            if (CanUsePauseMenu)
                _pauseService.Resume();
        }

        private void Restart()
        {
            if (CanUsePauseMenu)
                _navigation.StartGame();
        }

        private void OpenMenu()
        {
            if (CanUsePauseMenu)
                _navigation.OpenMenu();
        }

        private void OnPauseChanged(bool isPaused)
        {
            if (_pauseService == null)
                return;
            bool sessionEnded = _pauseService.IsSessionEnded;
            if (_panel != null)
                _panel.SetActive(isPaused && _pauseService.ShowPauseMenu && !sessionEnded && !_settingsOpen);
            if (_pauseButton != null)
                _pauseButton.gameObject.SetActive(!sessionEnded && !isPaused && !_settingsOpen);
        }
    }
}
