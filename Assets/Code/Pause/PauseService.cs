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
        public bool ShowPauseMenu { get; private set; }
        public bool IsHintsOpen { get; private set; }
        public event Action<bool> PauseChanged;

        [Inject]
        public void Construct(AudioService audioService)
        {
            _audioService = audioService;
            _audioService.SetPaused(IsPaused);
        }

        public void Pause(bool showMenu = true)
        {
            if (IsSessionEnded)
                return;
            if (IsPaused)
            {
                if (IsHintsOpen && showMenu && !ShowPauseMenu)
                {
                    ShowPauseMenu = true;
                    PauseChanged?.Invoke(true);
                }
                return;
            }

            StoreTimeScale();
            ShowPauseMenu = showMenu;
            IsPaused = true;
            Time.timeScale = 0f;
            _audioService.SetPaused(true);
            PauseChanged?.Invoke(true);
        }

        public void Resume()
        {
            if (!IsPaused || IsSessionEnded)
                return;

            if (IsHintsOpen)
            {
                // Closing the pause menu returns to hints without resuming play.
                ShowPauseMenu = false;
                PauseChanged?.Invoke(true);
                return;
            }

            IsPaused = false;
            ShowPauseMenu = false;
            RestoreTimeScale();
            _audioService.SetPaused(false);
            PauseChanged?.Invoke(false);
        }

        public void SetHintsOpen(bool open)
        {
            if (IsSessionEnded || IsHintsOpen == open || (open && IsPaused))
                return;

            IsHintsOpen = open;
            if (open)
                Pause(false);
            else if (!ShowPauseMenu)
                Resume();
            else
                PauseChanged?.Invoke(true);
        }

        public void EndSession()
        {
            if (IsSessionEnded)
                return;

            StoreTimeScale();
            IsSessionEnded = true;
            IsHintsOpen = false;
            ShowPauseMenu = false;
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
