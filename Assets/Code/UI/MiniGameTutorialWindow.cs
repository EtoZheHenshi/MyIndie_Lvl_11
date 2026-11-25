using System;
using Code.MiniGames;
using Code.Pause;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class MiniGameTutorialWindow : MonoBehaviour
    {
        [Serializable]
        public class Tutorial
        {
            public MiniGameWindow Window;
            public GameObject Panel;
            public Button ConfirmButton;
        }

        [SerializeField] private MiniGameCoordinator _coordinator;
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _background;
        [SerializeField] private Tutorial[] _tutorials;

        private Tutorial _current;

        private static string GetSeenKey(MiniGameWindow window)
        {
            return "TutorialSeen." + window.GetType().FullName;
        }

        private void Awake()
        {
            Hide();
        }

        private void OnEnable()
        {
            _coordinator.WindowOpened += OnWindowOpened;
            _coordinator.WindowClosed += OnWindowClosed;
            _pauseService.PauseChanged += OnPauseChanged;
            foreach (Tutorial tutorial in _tutorials)
                tutorial.ConfirmButton.onClick.AddListener(Confirm);
        }

        private void OnWindowOpened(MiniGameWindow window)
        {
            if (PlayerPrefs.GetInt(GetSeenKey(window), 0) == 1 ||
                _pauseService.IsPaused || _pauseService.IsSessionEnded)
                return;
            foreach (Tutorial tutorial in _tutorials)
            {
                if (tutorial.Window != window)
                    continue;
                _current = tutorial;
                _background.SetActive(true);
                tutorial.Panel.SetActive(true);
                _panel.SetActive(true);
                _pauseService.Pause(false);
                // Remember the first display across scene restarts and app launches.
                PlayerPrefs.SetInt(GetSeenKey(window), 1);
                PlayerPrefs.Save();
                if (EventSystem.current != null)
                    EventSystem.current.SetSelectedGameObject(null);
                break;
            }
        }

        private void Confirm()
        {
            if (_current == null || _pauseService.IsSessionEnded)
                return;
            Hide();
            _pauseService.Resume();
        }

        private void OnWindowClosed(MiniGameWindow window)
        {
            if (_current == null || _current.Window != window)
                return;
            Hide();
            if (!_pauseService.IsSessionEnded)
                _pauseService.Resume();
        }

        private void OnPauseChanged(bool paused)
        {
            if (!paused || _pauseService.IsSessionEnded)
                Hide();
        }

        private void Hide()
        {
            _current = null;
            if (_tutorials != null)
                foreach (Tutorial tutorial in _tutorials)
                    if (tutorial.Panel != null)
                        tutorial.Panel.SetActive(false);
            if (_panel != null) _panel.SetActive(false);
            if (_background != null) _background.SetActive(false);
        }

        private void OnDisable()
        {
            if (_coordinator != null)
            {
                _coordinator.WindowOpened -= OnWindowOpened;
                _coordinator.WindowClosed -= OnWindowClosed;
            }
            if (_pauseService != null)
                _pauseService.PauseChanged -= OnPauseChanged;
            if (_tutorials != null)
                foreach (Tutorial tutorial in _tutorials)
                    if (tutorial.ConfirmButton != null)
                        tutorial.ConfirmButton.onClick.RemoveListener(Confirm);
            Hide();
        }
    }
}
