using System.Collections.Generic;
using Code.ActivityTasks;
using Code.MotherCallMiniGame;
using Code.Motivation;
using Code.Pause;
using UnityEngine;
using Zenject;

namespace Code.Audio
{
    public class ActivityTaskAudio : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private MotivationService _motivationService;
        [SerializeField] private MotherCallMiniGameWindow _motherCallWindow;

        private readonly Dictionary<ActivityTaskState, TaskSounds> _sounds =
            new Dictionary<ActivityTaskState, TaskSounds>();
        private readonly List<ActivityTaskState> _removed = new List<ActivityTaskState>();
        private ActivityTaskService _tasks;
        private AudioService _audio;
        private bool _subscribed;

        [Inject]
        public void Construct(ActivityTaskService tasks, AudioService audio)
        {
            _tasks = tasks;
            _audio = audio;
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
            _subscribed = true;
            _tasks.TasksChanged += Refresh;
            _pauseService.PauseChanged += OnPauseChanged;
            _motherCallWindow.CallAccepted += StopRinging;
            Refresh();
        }

        private void Refresh()
        {
            if (_pauseService.IsSessionEnded)
            {
                StopAll();
                return;
            }

            _removed.Clear();
            foreach (ActivityTaskState task in _sounds.Keys)
            {
                bool exists = false;
                foreach (ActivityTaskState active in _tasks.ActiveTasks)
                    if (active == task)
                        exists = true;
                if (!exists)
                    _removed.Add(task);
            }
            foreach (ActivityTaskState task in _removed)
            {
                _sounds[task].Stop();
                _sounds.Remove(task);
            }

            foreach (ActivityTaskState task in _tasks.ActiveTasks)
            {
                if (!_sounds.TryGetValue(task, out TaskSounds sounds))
                {
                    sounds = new TaskSounds();
                    _sounds.Add(task, sounds);
                    // Existing, already-started tasks must not replay their notification.
                    if (!task.HasStarted && task.Definition.NotificationSound != null)
                        sounds.Notification = _audio.Play(task.Definition.NotificationSound);
                }

                if (task.HasStarted)
                {
                    if (task.Definition.Window != _motherCallWindow || _motherCallWindow.IsCallAccepted)
                        sounds.Notification?.Stop();
                    sounds.Reminder?.Stop();
                    continue;
                }

                if (task.IsOverdue && !sounds.Overdue)
                {
                    sounds.Overdue = true;
                    sounds.Notification?.Stop();
                    PlayReminder(task, sounds);
                }
            }
            if (_pauseService.IsPaused)
                OnPauseChanged(true);
        }

        private void Update()
        {
            if (!_subscribed || _pauseService.IsPaused ||
                _motivationService.IsDepleted || _motivationService.IsTypingCompleted)
                return;
            foreach (var pair in _sounds)
            {
                ActivityTaskState task = pair.Key;
                TaskSounds sounds = pair.Value;
                if (!sounds.Overdue || task.HasStarted || task.Definition.OverdueReminderSound == null)
                    continue;
                sounds.Remaining -= Time.deltaTime;
                if (sounds.Remaining <= 0f && (sounds.Reminder == null || !sounds.Reminder.IsValid))
                    PlayReminder(task, sounds);
            }
        }

        private void PlayReminder(ActivityTaskState task, TaskSounds sounds)
        {
            if (task.Definition.OverdueReminderSound != null)
                sounds.Reminder = _audio.Play(task.Definition.OverdueReminderSound);
            sounds.Remaining = task.Definition.GetReminderInterval();
        }

        private void StopRinging()
        {
            foreach (var pair in _sounds)
                if (pair.Key.Definition.Window == _motherCallWindow)
                    pair.Value.Notification?.Stop();
        }

        private void OnPauseChanged(bool paused)
        {
            if (_pauseService.IsSessionEnded)
            {
                StopAll();
                return;
            }
            // Task sounds pause even if a cue was configured to ignore the global audio pause.
            foreach (TaskSounds sounds in _sounds.Values)
            {
                if (paused)
                {
                    sounds.Notification?.Pause();
                    sounds.Reminder?.Pause();
                }
                else
                {
                    sounds.Notification?.Resume();
                    sounds.Reminder?.Resume();
                }
            }
        }

        private void StopAll()
        {
            foreach (TaskSounds sounds in _sounds.Values)
                sounds.Stop();
            _sounds.Clear();
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                if (_tasks != null)
                    _tasks.TasksChanged -= Refresh;
                if (_pauseService != null)
                    _pauseService.PauseChanged -= OnPauseChanged;
                if (_motherCallWindow != null)
                    _motherCallWindow.CallAccepted -= StopRinging;
                _subscribed = false;
            }
            StopAll();
        }

        private class TaskSounds
        {
            public AudioHandle Notification;
            public AudioHandle Reminder;
            public bool Overdue;
            public float Remaining;

            public void Stop()
            {
                Notification?.Stop();
                Reminder?.Stop();
            }
        }
    }
}
