using System;
using Code.MiniGames;
using UnityEngine;

namespace Code.ActivityTasks
{
    [Serializable]
    public class ActivityTaskDefinition
    {
        [SerializeField] private string _title;
        [SerializeField] private MiniGameWindow _window;
        [SerializeField] private Color _color = Color.yellow;
        [SerializeField, Min(0.1f)] private float _startDeadline = 20f;
        [SerializeField, Min(0f)] private float _motivationLossPerSecond = 2f;

        public string Title => _title;
        public MiniGameWindow Window => _window;
        public Color Color => _color;
        public float StartDeadline => Mathf.Max(0.1f, _startDeadline);
        public float MotivationLossPerSecond => Mathf.Max(0f, _motivationLossPerSecond);
    }
}
