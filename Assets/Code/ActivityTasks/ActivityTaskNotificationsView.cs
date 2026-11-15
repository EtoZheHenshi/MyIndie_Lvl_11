using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace Code.ActivityTasks
{
    public class ActivityTaskNotificationsView : MonoBehaviour
    {
        [SerializeField] private ActivityTaskView _template;
        [SerializeField] private Transform _container;
        [SerializeField] private ActivityTaskView _forcedTemplate;
        [SerializeField] private Transform _forcedContainer;

        private readonly Dictionary<ActivityTaskState, ActivityTaskView> _views =
            new Dictionary<ActivityTaskState, ActivityTaskView>();
        private ActivityTaskService _taskService;
        private bool _isSubscribed;

        [Inject]
        public void Construct(ActivityTaskService taskService)
        {
            _taskService = taskService;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_taskService == null || _isSubscribed)
                return;
            _taskService.TasksChanged += Refresh;
            _isSubscribed = true;
            Refresh();
        }

        private void OnDisable()
        {
            if (_taskService != null && _isSubscribed)
                _taskService.TasksChanged -= Refresh;
            _isSubscribed = false;
        }

        private void Refresh()
        {
            var removed = new List<ActivityTaskState>();
            foreach (ActivityTaskState task in _views.Keys)
            {
                bool exists = false;
                foreach (ActivityTaskState activeTask in _taskService.ActiveTasks)
                    if (activeTask == task && ShouldShow(task))
                        exists = true;
                if (!exists)
                    removed.Add(task);
            }
            foreach (ActivityTaskState task in removed)
            {
                _views[task].gameObject.SetActive(false);
                Destroy(_views[task].gameObject);
                _views.Remove(task);
            }

            foreach (ActivityTaskState task in _taskService.ActiveTasks)
            {
                if (!ShouldShow(task) || _views.ContainsKey(task))
                    continue;
                bool forced = task.Definition.ForceStart;
                ActivityTaskView view = Instantiate(forced ? _forcedTemplate : _template,
                    forced ? _forcedContainer : _container);
                view.name = "Task_" + task.Definition.Title;
                view.Initialize(task);
                view.gameObject.SetActive(true);
                _views.Add(task, view);
            }
        }

        private bool ShouldShow(ActivityTaskState task)
        {
            return !task.Definition.ForceStart || !task.HasStarted;
        }
    }
}
