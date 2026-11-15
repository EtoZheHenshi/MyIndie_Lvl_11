using UnityEngine;
using UnityEngine.Audio;

namespace Code.Audio
{
    [CreateAssetMenu(fileName = "AudioCue", menuName = "Game/Audio Cue")]
    public class AudioCue : ScriptableObject
    {
        [SerializeField] private AudioClip[] _clips = new AudioClip[0];
        // Keep the old serialized field to preserve clips in existing AudioCue assets.
        [SerializeField, HideInInspector] private AudioClip _clip;
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

        public string Description => _description;
        public float Volume => Mathf.Clamp01(_volume);
        public bool Loop => _loop;
        public bool IgnoreGamePause => _ignoreGamePause;
        public AudioMixerGroup OutputGroup => _outputGroup;

        public AudioClip GetClip()
        {
            if (_clips == null)
                return null;

            int count = 0;
            foreach (AudioClip clip in _clips)
                if (clip != null)
                    count++;
            if (count == 0)
                return null;

            int selected = count == 1 ? 0 : Random.Range(0, count);
            foreach (AudioClip clip in _clips)
                if (clip != null && selected-- == 0)
                    return clip;
            return null;
        }

        public float GetPitch()
        {
            if (!_randomizePitch)
                return Mathf.Clamp(_pitch, 0.1f, 3f);

            float minimum = Mathf.Clamp(Mathf.Min(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            float maximum = Mathf.Clamp(Mathf.Max(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            return Random.Range(minimum, maximum);
        }

        private void OnEnable()
        {
            MigrateLegacyClip();
        }

        private void OnValidate()
        {
            MigrateLegacyClip();
            float minimum = Mathf.Clamp(Mathf.Min(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            float maximum = Mathf.Clamp(Mathf.Max(_pitchRange.x, _pitchRange.y), 0.1f, 3f);
            _pitchRange = new Vector2(minimum, maximum);
        }

        private void MigrateLegacyClip()
        {
            if (_clip == null)
                return;

            if (_clips == null)
                _clips = new AudioClip[0];
            if (System.Array.IndexOf(_clips, _clip) < 0)
            {
                System.Array.Resize(ref _clips, _clips.Length + 1);
                _clips[_clips.Length - 1] = _clip;
            }
            _clip = null;
        }
    }
}
