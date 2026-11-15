using Code.ActivityTasks;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.TypingLogic
{
    public class FlowIndicatorView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        private ActivityTaskService _tasks;
        private bool _subscribed;

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
            _tasks.FlowStateChanged += Refresh;
            _subscribed = true;
            Refresh(_tasks.IsFlowActive);
        }

        private void Refresh(bool active)
        {
            _icon.enabled = active;
        }

        private void OnDisable()
        {
            if (!_subscribed)
                return;
            if (_tasks != null)
                _tasks.FlowStateChanged -= Refresh;
            _subscribed = false;
        }
    }
}
