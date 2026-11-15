using UnityEngine;
using UnityEngine.Audio;

namespace Code.Audio
{
    [CreateAssetMenu(fileName = "AudioCue", menuName = "Game/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        [SerializeField] private AudioClip _clip;
        [SerializeField, TextArea(3, 8)] private string _description;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;
        [SerializeField] private bool _loop;

        [Header("Pitch")]
        [SerializeField, Range(0.1f, 3f)] private float _pitch = 1f;
        [SerializeField] private bool _randomizePitch;
        [SerializeField] private Vector2 _pitchRange = new Vector2(0.95f, 1.05f);

        [Header("Optional")]
        [Tooltip("For UI sounds that should remain audible while the game is paused.")]
        [SerializeField] private bool _ignoreGamePause;
        [SerializeField] private AudioMixerGroup _outputGroup;

        public AudioClip Clip => _clip;
        public string Description => _description;
        public float Volume => Mathf.Clamp01(_volume);
        public bool Loop => _loop;
        public bool IgnoreGamePause => _ignoreGamePause;
        public AudioMixerGroup OutputGroup => _outputGroup;

        public float GetPitch()
        {
            if (!_randomizePitch)
                return Mathf.Clamp(_pitch, 0.1f, 3f);

            float minimum = Mathf.Clamp(Mathf.Min(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            float maximum = Mathf.Clamp(Mathf.Max(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            return Random.Range(minimum, maximum);
        }

        private void OnValidate()
        {
            float minimum = Mathf.Clamp(Mathf.Min(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            float maximum = Mathf.Clamp(Mathf.Max(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            _pitchRange = new Vector2(minimum, maximum);
        }
    }
}
