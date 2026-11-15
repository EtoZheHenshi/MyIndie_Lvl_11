using UnityEngine;
using UnityEngine.UI;

namespace Code.Pause
{
    public class PauseWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _resumeButton;

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(RequestPause);
            _resumeButton.onClick.AddListener(RequestResume);
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        private void OnDisable()
        {
            // UI objects can be destroyed before this component during scene teardown.
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(RequestPause);

            if (_resumeButton != null)
                _resumeButton.onClick.RemoveListener(RequestResume);

            if (_pauseService != null)
                _pauseService.PauseChanged -= OnPauseChanged;

            if (_panel != null)
                _panel.SetActive(false);
        }

        private void RequestPause()
        {
            _pauseService.Pause();
        }

        private void RequestResume()
        {
            _pauseService.Resume();
        }

        private void OnPauseChanged(bool isPaused)
        {
            bool sessionEnded = _pauseService.IsSessionEnded;
            _panel.SetActive(isPaused && !sessionEnded);
            _pauseButton.interactable = !sessionEnded;
            _resumeButton.interactable = !sessionEnded;
        }
    }
}
