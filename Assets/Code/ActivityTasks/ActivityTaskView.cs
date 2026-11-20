using UnityEngine;
using UnityEngine.UI;

namespace Code.ActivityTasks
{
    public class ActivityTaskView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Animator _iconAnimator;
        [SerializeField] private Image _timerFill;
        [SerializeField] private Image _timerTrack;
        [SerializeField] private Image _timerGap;
        [SerializeField, Min(1f)] private float _forcedIconScale = 1.5f;

        private ActivityTaskState _task;

        public void Initialize(ActivityTaskState task)
        {
            _task = task;
            _icon.sprite = task.Definition.Icon;
            _icon.color = task.Definition.Icon != null ? Color.white : task.Definition.Color;
            if (_iconAnimator != null)
            {
                _iconAnimator.runtimeAnimatorController = task.Definition.IconAnimation;
                _iconAnimator.enabled = task.Definition.IconAnimation != null;
            }
            bool forced = task.Definition.ForceStart;
            _timerFill.enabled = !forced;
            _timerTrack.enabled = !forced;
            _timerGap.enabled = !forced;
            _icon.rectTransform.localScale = Vector3.one * (forced ? _forcedIconScale : 1f);
            Refresh();
        }

        private void Update()
        {
            if (_task != null)
                Refresh();
        }

        private void Refresh()
        {
            if (_task.Definition.ForceStart)
                return;
            if (_task.IsOverdue)
            {
                _timerFill.fillAmount = 1f;
                _timerFill.color = new Color(1f, 0.3f, 0.25f);
            }
            else
            {
                _timerFill.fillAmount = _task.Remaining / _task.Duration;
                _timerFill.color = Color.white;
            }
        }
    }
}
