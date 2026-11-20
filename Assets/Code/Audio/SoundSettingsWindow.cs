using Code.Pause;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace Code.Audio
{
    public class SoundSettingsWindow : MonoBehaviour
    {
        [SerializeField] private PauseService _pauseService;
        [SerializeField] private PauseWindow _pauseWindow;
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private Slider _soundSlider;
        [SerializeField] private Slider _musicSlider;

        private AudioService _audio;
        private bool _subscribed;
        public bool IsOpen { get; private set; }

        private void Awake()
        {
            _panel.SetActive(false);
        }

        [Inject]
        public void Construct(AudioService audio)
        {
            _audio = audio;
            if (isActiveAndEnabled)
                Subscribe();
        }

        private void OnEnable()
        {
            Subscribe();
        }

        private void Subscribe()
        {
            if (_audio == null || _subscribed)
                return;
            _subscribed = true;
            _settingsButton.onClick.AddListener(Open);
            _backButton.onClick.AddListener(Close);
            _soundSlider.onValueChanged.AddListener(SetSoundVolume);
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
            _pauseService.PauseChanged += OnPauseChanged;
            RefreshSliders();
            OnPauseChanged(_pauseService.IsPaused);
        }

        private void Open()
        {
            if (IsOpen || _pauseService.IsSessionEnded ||
                !_pauseService.IsPaused || !_pauseService.ShowPauseMenu)
                return;

            IsOpen = true;
            RefreshSliders();
            _pauseWindow.SetSettingsOpen(true);
            _panel.SetActive(true);
            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);
        }

        private void Close()
        {
            if (!IsOpen)
                return;
            IsOpen = false;
            if (_panel != null)
                _panel.SetActive(false);
            if (_pauseWindow != null)
                _pauseWindow.SetSettingsOpen(false);
        }

        private void RefreshSliders()
        {
            _soundSlider.SetValueWithoutNotify(_audio.SoundVolume);
            _musicSlider.SetValueWithoutNotify(_audio.MusicVolume);
        }

        private void SetSoundVolume(float value)
        {
            _audio.SetSoundVolume(value);
        }

        private void SetMusicVolume(float value)
        {
            _audio.SetMusicVolume(value);
        }

        private void OnPauseChanged(bool paused)
        {
            if (_pauseService.IsSessionEnded || (!paused && IsOpen))
                Close();
        }

        private void OnDisable()
        {
            if (_subscribed)
            {
                if (_settingsButton != null)
                    _settingsButton.onClick.RemoveListener(Open);
                if (_backButton != null)
                    _backButton.onClick.RemoveListener(Close);
                if (_soundSlider != null)
                    _soundSlider.onValueChanged.RemoveListener(SetSoundVolume);
                if (_musicSlider != null)
                    _musicSlider.onValueChanged.RemoveListener(SetMusicVolume);
                if (_pauseService != null)
                    _pauseService.PauseChanged -= OnPauseChanged;
                _subscribed = false;
            }
            Close();
        }
    }
}
