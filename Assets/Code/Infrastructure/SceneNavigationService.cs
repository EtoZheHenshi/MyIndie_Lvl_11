using Code.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Code.Infrastructure
{
    public class SceneNavigationService
    {
        private readonly AudioService _audio;

        public bool IsLoading { get; private set; }

        public SceneNavigationService(AudioService audio)
        {
            _audio = audio;
        }

        public void StartGame()
        {
            Load("Main");
        }

        public void OpenMenu()
        {
            Load("StartMenu");
        }

        public void QuitGame()
        {
            if (IsLoading)
                return;
            _audio.StopAll();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void Load(string sceneName)
        {
            if (IsLoading)
                return;
            IsLoading = true;
            _audio.StopAll();
            _audio.SetPaused(false);
            Time.timeScale = 1f;
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            operation.completed += _ =>
            {
                // The departing scene can restore its previous pause state while unloading.
                Time.timeScale = 1f;
                _audio.SetPaused(false);
                IsLoading = false;
            };
        }
    }
}
