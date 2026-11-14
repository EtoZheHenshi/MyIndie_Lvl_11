using System;
using UnityEngine;

namespace Code.Pause
{
    public class PauseService : MonoBehaviour
    {
        private float _previousTimeScale;

        public bool IsPaused { get; private set; }
        public event Action<bool> PauseChanged;

        public void Pause()
        {
            if (IsPaused)
                return;

            _previousTimeScale = Time.timeScale;
            IsPaused = true;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused)
                return;

            IsPaused = false;
            Time.timeScale = _previousTimeScale;
            PauseChanged?.Invoke(false);
        }

        private void OnDestroy()
        {
            if (IsPaused)
                Time.timeScale = _previousTimeScale;
        }
    }
}
