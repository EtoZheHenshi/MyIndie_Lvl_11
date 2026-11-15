using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI
{
    public class StartMenuWindow : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _exitButton;
        private SceneNavigationService _navigation;

        [Inject]
        public void Construct(SceneNavigationService navigation)
        {
            _navigation = navigation;
        }

        private void Awake()
        {
            _exitButton.gameObject.SetActive(Application.platform != RuntimePlatform.WebGLPlayer);
        }

        private void OnEnable()
        {
            _startButton.onClick.AddListener(StartGame);
            _exitButton.onClick.AddListener(ExitGame);
        }

        private void OnDisable()
        {
            if (_startButton != null)
                _startButton.onClick.RemoveListener(StartGame);
            if (_exitButton != null)
                _exitButton.onClick.RemoveListener(ExitGame);
        }

        private void StartGame()
        {
            _navigation.StartGame();
        }

        private void ExitGame()
        {
            _navigation.QuitGame();
        }
    }
}
