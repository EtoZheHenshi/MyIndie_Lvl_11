using System;
using System.Collections.Generic;
using Code.MiniGames;
using Code.Motivation;
using Code.Pause;
using Code.TypingLogic;
using UnityEngine;
using Zenject;

namespace Code.ActivityTasks
{
    public class ActivityTaskService : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MiniGameCoordinator _coordinator;
        [SerializeField] private TypingMiniGameWindow _typingWindow;
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private MotivationService _motivationService;

        [Header("Notifications")]
        [SerializeField, Min(0.1f)] private float _minimumInterval = 30f;
        [SerializeField, Min(0.1f)] private float _maximumInterval = 60f;
        [SerializeField] private ActivityTaskDefinition[] _activities;

        [Header("Continuous typing")]
        [SerializeField, Min(0)] private int _safeSymbolCount = 40;
        [SerializeField, Range(0f, 100f)] private float _chanceIncreasePercent = 2f;
        [SerializeField, Min(0)] private int _safeSymbolsAfterRandomNotification = 5;

        private readonly List<ActivityTaskState> _activeTasks = new List<ActivityTaskState>();
        private readonly List<ActivityTaskDefinition> _firstCycle = new List<ActivityTaskDefinition>();
        private readonly List<ActivityTaskDefinition> _candidates = new List<ActivityTaskDefinition>();
        private TextComparer _textComparer;
        private bool _isSubscribed;
        private bool _initialized;

        public IReadOnlyList<ActivityTaskState> ActiveTasks => _activeTasks;
        public float NextNotificationRemaining { get; private set; }
        public int SymbolsSinceBreak { get; private set; }
        public float CurrentChancePercent { get; private set; }
        public int RandomNotificationSafeSymbolsRemaining { get; private set; }
        public bool HasEnteredFlow { get; private set; }
        public bool IsFlowActive { get; private set; }

        public event Action TasksChanged;
        public event Action FlowEntered;
        public event Action<bool> FlowStateChanged;

        [Inject]
        public void Construct(TextComparer textComparer)
        {
            _textComparer = textComparer;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
            RefreshDebuffs();
            foreach (ActivityTaskState task in _activeTasks)
                if (task.Definition.ForceStart && !task.HasStarted)
                    _coordinator.OpenForced(task.Definition.Window, task.Definition.ForcedStartDelay);
        }

        private void Start()
        {
            var windows = new HashSet<MiniGameWindow>();
            foreach (ActivityTaskDefinition activity in _activities)
            {
                if (activity == null || activity.Window == null ||
                    activity.Window == _typingWindow || !windows.Add(activity.Window))
                {
                    throw new InvalidOperationException(
                        "Activity tasks require distinct non-typing mini-game windows.");
                }
                _firstCycle.Add(activity);
            }

            for (int i = _firstCycle.Count - 1; i > 0; i--)
            {
                int other = UnityEngine.Random.Range(0, i + 1);
                ActivityTaskDefinition temporary = _firstCycle[i];
                _firstCycle[i] = _firstCycle[other];
                _firstCycle[other] = temporary;
            }

            ResetNotificationTimer();
            _initialized = true;
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private void Advance(float deltaTime)
        {
            if (!_initialized || !_typingWindow.HasBeenOpened || _pauseService.IsPaused ||
                _motivationService.IsTypingCompleted || _motivationService.IsDepleted)
                return;

            bool changed = false;
            foreach (ActivityTaskState task in _activeTasks)
                changed |= task.Advance(deltaTime);

            // Each deadline pauses only while its own activity is open.
            // Typing never stops deadlines or the next-notification timer.
            if (!_coordinator.IsOpeningForcedWindow &&
                (!_coordinator.HasOpenWindow || _coordinator.CurrentWindow == _typingWindow))
            {
                NextNotificationRemaining = Mathf.Max(0f, NextNotificationRemaining - deltaTime);
                if (NextNotificationRemaining <= 0f)
                {
                    if (TryCreateTask())
                    {
                        CurrentChancePercent *= 0.5f;
                        RandomNotificationSafeSymbolsRemaining =
                            Mathf.Max(0, _safeSymbolsAfterRandomNotification);
                    }
                    ResetNotificationTimer();
                }
            }

            RefreshDebuffs();
            if (changed)
                TasksChanged?.Invoke();
        }

        private void OnCorrectSymbolTyped()
        {
            if (!_initialized || _pauseService.IsPaused ||
                _motivationService.IsTypingCompleted || _motivationService.IsDepleted ||
                _coordinator.IsOpeningForcedWindow ||
                _coordinator.CurrentWindow != _typingWindow)
                return;

            if (SymbolsSinceBreak < int.MaxValue)
                SymbolsSinceBreak++;
            if (SymbolsSinceBreak >= _safeSymbolCount)
                SetFlowActive(true);
            if (!HasEnteredFlow && SymbolsSinceBreak >= _safeSymbolCount)
            {
                HasEnteredFlow = true;
                FlowEntered?.Invoke();
                if (_pauseService.IsPaused)
                    return;
            }
            if (RandomNotificationSafeSymbolsRemaining > 0)
            {
                RandomNotificationSafeSymbolsRemaining--;
                return;
            }
            if (SymbolsSinceBreak <= _safeSymbolCount)
                return;

            CurrentChancePercent = Mathf.Min(100f, CurrentChancePercent + _chanceIncreasePercent);
            if (CurrentChancePercent <= 0f ||
                (CurrentChancePercent < 100f && UnityEngine.Random.value * 100f >= CurrentChancePercent))
                return;

            if (TryCreateTask())
            {
                CurrentChancePercent = 0f;
                RandomNotificationSafeSymbolsRemaining = Mathf.Max(0, _safeSymbolsAfterRandomNotification);
                ResetNotificationTimer();
            }
        }

        private bool TryCreateTask()
        {
            if (!_typingWindow.HasBeenOpened || _coordinator.IsOpeningForcedWindow)
                return false;

            _candidates.Clear();
            if (_firstCycle.Count > 0)
            {
                // Never repeat an activity before every configured activity has
                // appeared once, even if the next one is temporarily unavailable.
                foreach (ActivityTaskDefinition activity in _firstCycle)
                {
                    if (CanCreate(activity))
                    {
                        _candidates.Add(activity);
                        break;
                    }
                }
            }
            else
            {
                foreach (ActivityTaskDefinition activity in _activities)
                    if (CanCreate(activity))
                        _candidates.Add(activity);
            }

            if (_candidates.Count == 0)
                return false;

            ActivityTaskDefinition selected = _candidates[UnityEngine.Random.Range(0, _candidates.Count)];
            if (selected.ForceStart && !_coordinator.OpenForced(selected.Window, selected.ForcedStartDelay))
                return false;
            _firstCycle.Remove(selected);
            _activeTasks.Add(new ActivityTaskState(selected));
            TasksChanged?.Invoke();
            return true;
        }

        private bool CanCreate(ActivityTaskDefinition activity)
        {
            if (!activity.Window.isActiveAndEnabled || activity.Window.IsOpen)
                return false;
            foreach (ActivityTaskState task in _activeTasks)
                if (task.Definition.Window == activity.Window)
                    return false;
            return true;
        }

        private void OnWindowOpened(MiniGameWindow window)
        {
            if (window == _typingWindow)
                return;

            SymbolsSinceBreak = 0;
            SetFlowActive(false);
            CurrentChancePercent = 0f;
            RandomNotificationSafeSymbolsRemaining = 0;
            foreach (ActivityTaskState task in _activeTasks)
                if (task.Definition.Window == window)
                    task.MarkStarted();

            TasksChanged?.Invoke();
        }

        private void OnWindowCompleted(MiniGameWindow window)
        {
            _activeTasks.RemoveAll(task => task.Definition.Window == window);
            RefreshDebuffs();
            TasksChanged?.Invoke();
        }

        private void OnWindowClosed(MiniGameWindow window)
        {
            // Closing is not completion: keep the task and any existing debuff.
            TasksChanged?.Invoke();
        }

        private void RefreshDebuffs()
        {
            if (_motivationService == null)
                return;
            float totalLoss = 0f;
            foreach (ActivityTaskState task in _activeTasks)
                if (task.IsOverdue)
                    totalLoss += task.Definition.MotivationLossPerSecond;
            _motivationService.SetAdditionalLoss(totalLoss);
        }

        private void SetFlowActive(bool active)
        {
            if (IsFlowActive == active)
                return;
            IsFlowActive = active;
            FlowStateChanged?.Invoke(active);
        }

        private void ResetNotificationTimer()
        {
            float minimum = Mathf.Max(0.1f, _minimumInterval);
            float maximum = Mathf.Max(minimum, _maximumInterval);
            NextNotificationRemaining = UnityEngine.Random.Range(minimum, maximum);
        }

        private void Subscribe()
        {
            if (_isSubscribed || _textComparer == null)
                return;
            _coordinator.WindowOpened += OnWindowOpened;
            _coordinator.WindowClosed += OnWindowClosed;
            _coordinator.WindowCompleted += OnWindowCompleted;
            _textComparer.CorrectSymbolTyped += OnCorrectSymbolTyped;
            _isSubscribed = true;
        }

        private void OnDisable()
        {
            if (_coordinator != null)
                _coordinator.CancelForcedOpen();
            if (_isSubscribed)
            {
                if (_coordinator != null)
                {
                    _coordinator.WindowOpened -= OnWindowOpened;
                    _coordinator.WindowClosed -= OnWindowClosed;
                    _coordinator.WindowCompleted -= OnWindowCompleted;
                }
                _textComparer.CorrectSymbolTyped -= OnCorrectSymbolTyped;
                _isSubscribed = false;
            }
            if (_motivationService != null)
                _motivationService.SetAdditionalLoss(0f);
        }
    }
}
