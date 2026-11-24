using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Code.ActivityTasks;
using Code.MiniGames;
using Zenject;

namespace Code.UI
{
    public class HoverBorder : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private Image _border;
        [SerializeField] private Animator _animator;
        [SerializeField] private MiniGameWindow _window;
        [SerializeField] private MiniGameCoordinator _coordinator;
        [SerializeField] private Color _notificationColor = Color.red;
        [SerializeField, Min(0.1f)] private float _overdueBlinkPeriod = 0.7f;

        private ActivityTaskService _tasks;
        private Color _originalColor;
        private bool _hovered;
        private bool _subscribed;
        private bool _overdue;
        private float _blinkElapsed;

        [Inject]
        public void Construct(ActivityTaskService tasks)
        {
            _tasks = tasks;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void Awake()
        {
            _border.raycastTarget = false;
            _originalColor = _border.color;
            Refresh();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_tasks == null || _subscribed)
                return;
            _tasks.TasksChanged += Refresh;
            _coordinator.WindowOpened += OnWindowChanged;
            _coordinator.WindowClosed += OnWindowChanged;
            _subscribed = true;
            Refresh();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hovered = true;
            Refresh();
            if (_animator != null && _animator.isActiveAndEnabled)
            {
                _animator.Play(0, 0, 0f);
                _animator.Update(0f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hovered = false;
            Refresh();
        }

        private void OnDisable()
        {
            if (_tasks != null && _subscribed)
                _tasks.TasksChanged -= Refresh;
            if (_coordinator != null && _subscribed)
            {
                _coordinator.WindowOpened -= OnWindowChanged;
                _coordinator.WindowClosed -= OnWindowChanged;
            }
            _subscribed = false;
            _hovered = false;
            HideBorder();
        }

        private void OnWindowChanged(MiniGameWindow window)
        {
            // Opening a window invalidates any previous apartment hover.
            _hovered = false;
            Refresh();
        }

        private void Update()
        {
            if (!_overdue || _hovered || !_border.enabled)
                return;

            _blinkElapsed = (_blinkElapsed + Time.deltaTime) %
                Mathf.Max(0.1f, _overdueBlinkPeriod);
            Color color = _notificationColor;
            color.a *= Mathf.Lerp(0.15f, 1f, (1f + Mathf.Cos(
                _blinkElapsed / Mathf.Max(0.1f, _overdueBlinkPeriod) * Mathf.PI * 2f)) * 0.5f);
            _border.color = color;
        }

        private void Refresh()
        {
            bool hasTask = false;
            bool overdue = false;
            if (_tasks != null && _window != null)
                foreach (ActivityTaskState task in _tasks.ActiveTasks)
                    if (task.Definition.Window == _window)
                    {
                        hasTask = true;
                        overdue = task.IsOverdue;
                        break;
                    }

            if (overdue != _overdue)
                _blinkElapsed = 0f;
            _overdue = overdue;

            if (!isActiveAndEnabled || (_coordinator != null && _coordinator.HasOpenWindow) ||
                (!_hovered && !hasTask))
            {
                HideBorder();
                return;
            }

            bool wasVisible = _border.enabled;
            _border.enabled = true;
            _border.color = _hovered ? _originalColor : _notificationColor;
            if (_animator != null)
            {
                _animator.enabled = true;
                if (!wasVisible)
                {
                    _animator.Play(0, 0, 0f);
                    _animator.Update(0f);
                }
            }
        }

        private void HideBorder()
        {
            if (_animator != null)
                _animator.enabled = false;

            if (_border != null)
                _border.enabled = false;
        }
    }
}
