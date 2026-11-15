namespace Code.Audio
{
    public sealed class AudioHandle
    {
        private readonly AudioService _service;

        public bool IsValid => _service != null && _service.IsValid(this);
        public bool IsPlaying => _service != null && _service.IsPlaying(this);
        public bool IsPaused => _service != null && _service.IsPaused(this);

        public AudioHandle(AudioService service)
        {
            _service = service;
        }

        public void Pause()
        {
            if (_service != null)
                _service.Pause(this);
        }

        public void Resume()
        {
            if (_service != null)
                _service.Resume(this);
        }

        public void Stop()
        {
            if (_service != null)
                _service.Stop(this);
        }
    }
}
