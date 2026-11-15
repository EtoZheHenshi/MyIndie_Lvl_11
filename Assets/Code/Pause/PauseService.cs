using System;
using Code.Audio;
using UnityEngine;
using Zenject;

namespace Code.Pause
{
    public class PauseService : MonoBehaviour
    {
        private float _previousTimeScale;
        private bool _hasStoredTimeScale;
        private AudioService _audioService;

        public bool IsPaused { get; private set; }
        public bool IsSessionEnded { get; private set; }
        public event Action<bool> PauseChanged;

        [Inject]
        public void Construct(AudioService audioService)
        {
            _audioService = audioService;
            _audioService.SetPaused(IsPaused);
        }

        public void Pause()
        {
            if (IsPaused || IsSessionEnded)
                return;

            StoreTimeScale();
            IsPaused = true;
            Time.timeScale = 0f;
            _audioService.SetPaused(true);
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused || IsSessionEnded)
                return;

            IsPaused = false;
            RestoreTimeScale();
            _audioService.SetPaused(false);
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
            _audioService.SetPaused(true);
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
            if (IsPaused && _audioService != null)
                _audioService.SetPaused(false);
            RestoreTimeScale();
        }
    }
}
