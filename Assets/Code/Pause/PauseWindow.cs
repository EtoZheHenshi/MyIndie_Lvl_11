using UnityEngine;
using UnityEngine.UI;

namespace Code.Pause
{
    public class PauseWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Animator _buttonAnimator;
        [SerializeField] private RuntimeAnimatorController _pauseAnimation;
        [SerializeField] private RuntimeAnimatorController _playAnimation;

        private void OnEnable()
        {
            _pauseButton.onClick.AddListener(TogglePause);
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        private void OnDisable()
        {
            // UI objects can be destroyed before this component during scene teardown.
            if (_pauseButton != null)
                _pauseButton.onClick.RemoveListener(TogglePause);

            if (_pauseService != null)
                _pauseService.PauseChanged -= OnPauseChanged;

            if (_panel != null)
                _panel.SetActive(false);
        }

        private void TogglePause()
        {
            if (_pauseService.IsSessionEnded ||
                (_pauseService.IsPaused && !_pauseService.ShowPauseMenu))
                return;
            if (_pauseService.IsPaused)
                _pauseService.Resume();
            else
                _pauseService.Pause();
        }

        private void OnPauseChanged(bool isPaused)
        {
            bool sessionEnded = _pauseService.IsSessionEnded;
            _panel.SetActive(isPaused && _pauseService.ShowPauseMenu && !sessionEnded);
            _pauseButton.gameObject.SetActive(!sessionEnded);
            _pauseButton.interactable = !isPaused || _pauseService.ShowPauseMenu;
            _buttonAnimator.runtimeAnimatorController = isPaused ? _playAnimation : _pauseAnimation;
        }
    }
}
