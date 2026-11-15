using Code.Pause;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.ActivityTasks
{
    public class FlowTutorialWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _confirmButton;

        private ActivityTaskService _tasks;
        private bool _subscribed;
        private bool _isOpen;

        private void Awake()
        {
            _panel.SetActive(false);
        }

        [Inject]
        public void Construct(ActivityTaskService tasks)
        {
            _tasks = tasks;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_tasks == null || _subscribed)
                return;
            _tasks.FlowEntered += Show;
            _pauseService.PauseChanged += OnPauseChanged;
            _confirmButton.onClick.AddListener(Confirm);
            _subscribed = true;
        }

        private void Show()
        {
            if (_isOpen || _pauseService.IsPaused || _pauseService.IsSessionEnded)
                return;
            _isOpen = true;
            _pauseService.Pause(false);
            _panel.SetActive(true);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void Confirm()
        {
            if (!_isOpen)
                return;
            Hide();
            _pauseService.Resume();
        }

        private void OnPauseChanged(bool paused)
        {
            if (!paused || _pauseService.IsSessionEnded)
                Hide();
        }

        private void Hide()
        {
            _isOpen = false;
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                if (_tasks != null)
                    _tasks.FlowEntered -= Show;
                if (_pauseService != null)
                    _pauseService.PauseChanged -= OnPauseChanged;
                if (_confirmButton != null)
                    _confirmButton.onClick.RemoveListener(Confirm);
                _subscribed = false;
            }
            Hide();
        }
    }
}
