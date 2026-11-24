using Code.Pause;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class HintsWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private Button _openButton;
        [SerializeField] private Button _previousButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Animator _pageAnimator;
        [SerializeField] private RuntimeAnimatorController[] _pages;
        private int _pageIndex;

        private bool CanUseHints => _pauseService.IsHintsOpen &&
            !_pauseService.ShowPauseMenu && !_pauseService.IsSessionEnded;

        private void OnEnable()
        {
            _openButton.onClick.AddListener(Open);
            _previousButton.onClick.AddListener(PreviousPage);
            _nextButton.onClick.AddListener(NextPage);
            _closeButton.onClick.AddListener(Close);
            _pauseService.PauseChanged += OnPauseChanged;
            OnPauseChanged(_pauseService.IsPaused);
        }

        private void Open()
        {
            if (_pauseService.IsPaused || _pauseService.IsSessionEnded)
                return;
            _pageIndex = 0;
            ShowPage();
            _pauseService.SetHintsOpen(true);
        }

        private void Close()
        {
            if (CanUseHints)
                _pauseService.SetHintsOpen(false);
        }

        private void PreviousPage()
        {
            if (CanUseHints && _pageIndex > 0)
            {
                _pageIndex--;
                ShowPage();
            }
        }

        private void NextPage()
        {
            if (CanUseHints && _pageIndex < _pages.Length - 1)
            {
                _pageIndex++;
                ShowPage();
            }
        }

        private void ShowPage()
        {
            _pageAnimator.runtimeAnimatorController = _pages[_pageIndex];
            // Restart each page's sprite animation on selection.
            if (_pageAnimator.isActiveAndEnabled)
            {
                _pageAnimator.Play(0, 0, 0f);
                _pageAnimator.Update(0f);
            }
            _previousButton.interactable = _pageIndex > 0;
            _nextButton.interactable = _pageIndex < _pages.Length - 1;
        }

        private void OnPauseChanged(bool paused)
        {
            if (_pauseService == null)
                return;
            if (_panel != null)
                _panel.SetActive(_pauseService.IsHintsOpen && !_pauseService.IsSessionEnded);
            if (_openButton != null)
                _openButton.gameObject.SetActive(!paused && !_pauseService.IsSessionEnded);
        }

        private void OnDisable()
        {
            if (_openButton != null) _openButton.onClick.RemoveListener(Open);
            if (_previousButton != null) _previousButton.onClick.RemoveListener(PreviousPage);
            if (_nextButton != null) _nextButton.onClick.RemoveListener(NextPage);
            if (_closeButton != null) _closeButton.onClick.RemoveListener(Close);
            if (_pauseService != null)
            {
                _pauseService.PauseChanged -= OnPauseChanged;
                _pauseService.SetHintsOpen(false);
            }
            if (_panel != null) _panel.SetActive(false);
        }
    }
}
