using System;
using UnityEngine;

namespace Code.Pause
{
    public class PauseService : MonoBehaviour
    {
        private float _previousTimeScale;
        private bool _hasStoredTimeScale;

        public bool IsPaused { get; private set; }
        public bool IsSessionEnded { get; private set; }
        public event Action<bool> PauseChanged;

        public void Pause()
        {
            if (IsPaused || IsSessionEnded)
                return;

            StoreTimeScale();
            IsPaused = true;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused || IsSessionEnded)
                return;

            IsPaused = false;
            RestoreTimeScale();
            PauseChanged?.Invoke(false);
        }

        public void EndSession()
        {
            if (IsSessionEnded)
                return;

            StoreTimeScale();
            IsSessionEnded = true;
            IsPaused = true;
            Time.timeScale = 0f;
            PauseChanged?.Invoke(true);
        }

        private void StoreTimeScale()
        {
            if (_hasStoredTimeScale)
                return;
            _previousTimeScale = Time.timeScale;
            _hasStoredTimeScale = true;
        }

        private void RestoreTimeScale()
        {
            if (!_hasStoredTimeScale)
                return;
            Time.timeScale = _previousTimeScale;
            _hasStoredTimeScale = false;
        }

        private void OnDestroy()
        {
            RestoreTimeScale();
        }
    }
}
