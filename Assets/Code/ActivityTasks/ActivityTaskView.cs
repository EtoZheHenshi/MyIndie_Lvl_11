using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.ActivityTasks
{
    public class ActivityTaskView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private Image _timerFill;
        [SerializeField] private TMP_Text _statusText;
        [SerializeField] private TMP_Text _titleText;

        private ActivityTaskState _task;

        public void Initialize(ActivityTaskState task)
        {
            _task = task;
            _icon.color = task.Definition.Color;
            _titleText.text = task.Definition.Title;
            Refresh();
        }

        private void Update()
        {
            if (_task != null)
                Refresh();
        }

        private void Refresh()
        {
            if (_task.IsOverdue)
            {
                _timerFill.fillAmount = 1f;
                _timerFill.color = new Color(1f, 0.3f, 0.25f);
                _statusText.SetText("-{0:1}/с", _task.Definition.MotivationLossPerSecond);
            }
            else if (_task.HasStarted)
            {
                _timerFill.fillAmount = 1f;
                _timerFill.color = new Color(0.45f, 0.9f, 0.6f);
                _statusText.text = "В игре";
            }
            else
            {
                _timerFill.fillAmount = _task.Remaining / _task.Duration;
                _timerFill.color = Color.white;
                _statusText.SetText("{0} с", Mathf.CeilToInt(_task.Remaining));
            }
        }
    }
}
