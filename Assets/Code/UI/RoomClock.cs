using UnityEngine;

namespace Code.UI
{
    public class RoomClock : MonoBehaviour
    {
        [SerializeField] private Transform _hourHand;
        [SerializeField] private Transform _minuteHand;
        [SerializeField, Min(0.01f)] private float _minuteRevolutionSeconds = 15f;

        private Quaternion _initialHourRotation;
        private Quaternion _initialMinuteRotation;
        private double _minuteTurns;

        private void Awake()
        {
            _initialHourRotation = _hourHand.localRotation;
            _initialMinuteRotation = _minuteHand.localRotation;
        }

        private void Update()
        {
            // Scaled time stops the clock during pause and after the game ends.
            _minuteTurns = (_minuteTurns + Time.deltaTime /
                Mathf.Max(0.01f, _minuteRevolutionSeconds)) % 12d;
            _minuteHand.localRotation = _initialMinuteRotation *
                Quaternion.AngleAxis((float)(-360d * _minuteTurns), Vector3.forward);
            _hourHand.localRotation = _initialHourRotation *
                Quaternion.AngleAxis((float)(-30d * _minuteTurns), Vector3.forward);
        }
    }
}
