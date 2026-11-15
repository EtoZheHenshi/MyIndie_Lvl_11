using UnityEngine;
using Zenject;

namespace Code.Audio
{
    public class MusicPlayback : MonoBehaviour
    {
        [SerializeField] private AudioCue _cue;
        [Tooltip("Temporarily replace the background track and resume it when this object is hidden.")]
        [SerializeField] private bool _restorePreviousMusic;

        private AudioService _audio;
        private AudioHandle _handle;

        [Inject]
        public void Construct(AudioService audio)
        {
            _audio = audio;
            if (isActiveAndEnabled)
                Play();
        }

        private void OnEnable()
        {
            if (_audio != null)
                Play();
        }

        private void Play()
        {
            if (_handle != null && _handle.IsValid)
                return;
            _handle = _restorePreviousMusic ? _audio.PushMusic(_cue) : _audio.PlayMusic(_cue);
        }

        private void OnDisable()
        {
            _handle?.Stop();
            _handle = null;
        }
    }
}
