using System.Collections.Generic;
using UnityEngine;

namespace Code.Audio
{
    [DisallowMultipleComponent]
    public class AudioService : MonoBehaviour
    {
        [SerializeField, Range(0f, 1f)] private float _masterVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _soundVolume = 1f;
        [SerializeField, Range(0f, 1f)] private float _musicVolume = 1f;

        private readonly Dictionary<AudioHandle, Voice> _voices = new Dictionary<AudioHandle, Voice>();
        private readonly Dictionary<string, AudioCue> _cues = new Dictionary<string, AudioCue>();
        private readonly Stack<AudioSource> _sourcePool = new Stack<AudioSource>();
        private readonly Stack<AudioHandle> _musicStack = new Stack<AudioHandle>();
        private readonly List<AudioHandle> _finished = new List<AudioHandle>();
        private AudioHandle _currentMusic;

        public bool IsGloballyPaused { get; private set; }
        public bool IsMuted { get; private set; }
        public int ActiveVoiceCount => _voices.Count;
        public AudioHandle CurrentMusic => IsValid(_currentMusic) ? _currentMusic : null;

        public AudioCue GetCue(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                Debug.LogError("Specify an audio cue path relative to Resources/Audio, without an extension.", this);
                return null;
            }

            if (_cues.TryGetValue(path, out AudioCue cached) && cached != null)
                return cached;

            AudioCue cue = Resources.Load<AudioCue>("Audio/" + path);
            if (cue == null)
            {
                Debug.LogError($"Audio cue not found: Resources/Audio/{path}", this);
                return null;
            }
            _cues[path] = cue;
            return cue;
        }

        public AudioHandle Play(string path)
        {
            AudioCue cue = GetCue(path);
            return cue != null ? Play(cue) : null;
        }

        public AudioHandle Play(AudioCue cue)
        {
            return Validate(cue) ? CreateVoice(cue, false) : null;
        }

        public AudioHandle PlayMusic(string path)
        {
            AudioCue cue = GetCue(path);
            return cue != null ? PlayMusic(cue) : null;
        }

        public AudioHandle PlayMusic(AudioCue cue)
        {
            if (!Validate(cue))
                return null;
            StopMusic();
            _currentMusic = CreateVoice(cue, true);
            return _currentMusic;
        }

        public AudioHandle PushMusic(string path)
        {
            AudioCue cue = GetCue(path);
            return cue != null ? PushMusic(cue) : null;
        }

        public AudioHandle PushMusic(AudioCue cue)
        {
            if (!Validate(cue))
                return null;
            if (TryGetVoice(_currentMusic, out Voice current))
            {
                current.Suspended = true;
                RefreshPause(current);
            }
            _musicStack.Push(_currentMusic);
            _currentMusic = CreateVoice(cue, true);
            return _currentMusic;
        }

        public void PopMusic()
        {
            if (_musicStack.Count == 0)
                return;
            ReleaseVoice(_currentMusic);
            _currentMusic = null;
            while (_musicStack.Count > 0)
            {
                AudioHandle previous = _musicStack.Pop();
                if (previous == null)
                    return;
                if (!TryGetVoice(previous, out Voice voice))
                    continue;
                _currentMusic = previous;
                voice.Suspended = false;
                RefreshPause(voice);
                return;
            }
        }

        public void PauseMusic()
        {
            Pause(_currentMusic);
        }

        public void ResumeMusic()
        {
            Resume(_currentMusic);
        }

        public void StopMusic()
        {
            ReleaseVoice(_currentMusic);
            _currentMusic = null;
            foreach (AudioHandle handle in _musicStack)
                ReleaseVoice(handle);
            _musicStack.Clear();
        }

        public bool IsValid(AudioHandle handle)
        {
            return TryGetVoice(handle, out Voice voice) && voice.Source != null;
        }

        public bool IsPlaying(AudioHandle handle)
        {
            return TryGetVoice(handle, out Voice voice) && voice.Source != null && voice.Source.isPlaying;
        }

        public bool IsPaused(AudioHandle handle)
        {
            return TryGetVoice(handle, out Voice voice) && voice.Paused;
        }

        public void Pause(AudioHandle handle)
        {
            if (!TryGetVoice(handle, out Voice voice))
                return;
            voice.ManuallyPaused = true;
            RefreshPause(voice);
        }

        public void Resume(AudioHandle handle)
        {
            if (!TryGetVoice(handle, out Voice voice))
                return;
            voice.ManuallyPaused = false;
            RefreshPause(voice);
        }

        public void Stop(AudioHandle handle)
        {
            if (handle != null && handle == _currentMusic && _musicStack.Count > 0)
            {
                PopMusic();
                return;
            }
            ReleaseVoice(handle);
            if (handle == _currentMusic)
                _currentMusic = null;
        }

        public void SetPaused(bool paused)
        {
            IsGloballyPaused = paused;
            foreach (Voice voice in _voices.Values)
                RefreshPause(voice);
        }

        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            RefreshVolumes();
        }

        public void SetSoundVolume(float volume)
        {
            _soundVolume = Mathf.Clamp01(volume);
            RefreshVolumes();
        }

        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            RefreshVolumes();
        }

        public void SetMuted(bool muted)
        {
            IsMuted = muted;
            RefreshVolumes();
        }

        public void StopAll()
        {
            StopMusic();
            _finished.Clear();
            _finished.AddRange(_voices.Keys);
            foreach (AudioHandle handle in _finished)
                ReleaseVoice(handle);
            _finished.Clear();
        }

        private AudioHandle CreateVoice(AudioCue cue, bool music)
        {
            AudioSource source;
            if (_sourcePool.Count > 0)
                source = _sourcePool.Pop();
            else
            {
                var go = new GameObject("AudioVoice");
                go.transform.SetParent(transform, false);
                source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.spatialBlend = 0f;
                source.dopplerLevel = 0f;
            }
            source.gameObject.name = cue.name;
            source.gameObject.SetActive(true);
            source.clip = cue.Clip;
            source.loop = cue.Loop;
            source.pitch = cue.GetPitch();
            source.outputAudioMixerGroup = cue.OutputGroup;
            source.ignoreListenerPause = cue.IgnoreGamePause;
            if (cue.Clip.loadState == AudioDataLoadState.Unloaded)
                cue.Clip.LoadAudioData();

            var handle = new AudioHandle(this);
            var voice = new Voice
            {
                Source = source,
                Cue = cue,
                Music = music,
                CleanupAfter = Time.unscaledTime + 0.1f
            };
            _voices.Add(handle, voice);
            RefreshVolume(voice);
            source.Play();
            RefreshPause(voice);
            return handle;
        }

        private bool Validate(AudioCue cue)
        {
            if (cue == null)
            {
                Debug.LogError("Audio cue is not assigned.", this);
                return false;
            }
            if (cue.Clip != null)
                return true;
            Debug.LogError($"Audio cue '{cue.name}' has no AudioClip assigned.", cue);
            return false;
        }

        private bool TryGetVoice(AudioHandle handle, out Voice voice)
        {
            voice = null;
            return handle != null && _voices.TryGetValue(handle, out voice);
        }

        private void RefreshPause(Voice voice)
        {
            bool paused = voice.ManuallyPaused || voice.Suspended ||
                (IsGloballyPaused && !voice.Cue.IgnoreGamePause);
            if (voice.Paused == paused || voice.Source == null)
                return;
            voice.Paused = paused;
            if (paused)
                voice.Source.Pause();
            else
                voice.Source.UnPause();
        }

        private void RefreshVolumes()
        {
            foreach (Voice voice in _voices.Values)
                RefreshVolume(voice);
        }

        private void RefreshVolume(Voice voice)
        {
            voice.Source.volume = voice.Cue.Volume * _masterVolume *
                (voice.Music ? _musicVolume : _soundVolume);
            voice.Source.mute = IsMuted;
        }

        private void ReleaseVoice(AudioHandle handle)
        {
            if (!TryGetVoice(handle, out Voice voice))
                return;
            _voices.Remove(handle);
            if (voice.Source == null)
                return;
            voice.Source.Stop();
            voice.Source.clip = null;
            voice.Source.outputAudioMixerGroup = null;
            voice.Source.loop = false;
            voice.Source.pitch = 1f;
            voice.Source.gameObject.SetActive(false);
            _sourcePool.Push(voice.Source);
        }

        private void LateUpdate()
        {
            _finished.Clear();
            foreach (KeyValuePair<AudioHandle, Voice> pair in _voices)
            {
                Voice voice = pair.Value;
                if (voice.Source == null)
                {
                    _finished.Add(pair.Key);
                    continue;
                }
                if (voice.Paused)
                    continue;
                if (voice.Cue.Clip.loadState == AudioDataLoadState.Failed)
                {
                    Debug.LogError($"Failed to load audio clip for '{voice.Cue.name}'.", voice.Cue);
                    _finished.Add(pair.Key);
                }
                else if (Time.unscaledTime >= voice.CleanupAfter &&
                    voice.Cue.Clip.loadState == AudioDataLoadState.Loaded && !voice.Source.isPlaying)
                    _finished.Add(pair.Key);
            }
            foreach (AudioHandle handle in _finished)
                Stop(handle);
            _finished.Clear();
        }

        private void OnDestroy()
        {
            _voices.Clear();
            _musicStack.Clear();
            _sourcePool.Clear();
            _cues.Clear();
        }

        private class Voice
        {
            public AudioSource Source;
            public AudioCue Cue;
            public bool Music;
            public bool ManuallyPaused;
            public bool Suspended;
            public bool Paused;
            public float CleanupAfter;
        }
    }
}
