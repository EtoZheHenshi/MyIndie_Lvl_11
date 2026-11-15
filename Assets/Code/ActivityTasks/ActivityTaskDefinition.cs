using System;
using Code.MiniGames;
using Code.Audio;
using UnityEngine;

namespace Code.ActivityTasks
{
    [Serializable]
    public class ActivityTaskDefinition
    {
        [SerializeField] private string _title;
        [SerializeField] private MiniGameWindow _window;
        [SerializeField] private Color _color = Color.yellow;
        [SerializeField] private Sprite _icon;
        [SerializeField] private RuntimeAnimatorController _iconAnimation;
        [SerializeField, Min(0.1f)] private float _startDeadline = 20f;
        [SerializeField, Min(0f)] private float _motivationLossPerSecond = 2f;

        [Header("Audio")]
        [SerializeField] private AudioCue _notificationSound;
        [SerializeField] private AudioCue _overdueReminderSound;
        [SerializeField] private Vector2 _reminderInterval = new Vector2(10f, 20f);

        [Header("Forced activity")]
        [SerializeField] private bool _forceStart;
        [SerializeField, Min(0f)] private float _forcedStartDelay = 1.5f;

        public string Title => _title;
        public MiniGameWindow Window => _window;
        public Color Color => _color;
        public Sprite Icon => _icon;
        public RuntimeAnimatorController IconAnimation => _iconAnimation;
        public float StartDeadline => Mathf.Max(0.1f, _startDeadline);
        public float MotivationLossPerSecond => Mathf.Max(0f, _motivationLossPerSecond);
        public AudioCue NotificationSound => _notificationSound;
        public AudioCue OverdueReminderSound => _overdueReminderSound;

        public float GetReminderInterval()
        {
            float minimum = Mathf.Max(0.1f, Mathf.Min(_reminderInterval.x, _reminderInterval.y));
            float maximum = Mathf.Max(minimum, Mathf.Max(_reminderInterval.x, _reminderInterval.y));
            return UnityEngine.Random.Range(minimum, maximum);
        }
        public bool ForceStart => _forceStart;
        public float ForcedStartDelay => Mathf.Max(0f, _forcedStartDelay);
    }
}
