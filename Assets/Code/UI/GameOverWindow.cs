using Code.Motivation;
using Code.Infrastructure;
using Code.MiniGames;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace Code.UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private MotivationService _motivationService;
        [SerializeField] private GameObject _panel;
        [SerializeField] private GameObject _victoryPanel;
        [SerializeField] private MiniGameCoordinator _coordinator;
        [SerializeField] private GameObject _gameplayHud;
        [SerializeField] private Button _loseRestartButton;
        [SerializeField] private Button _winRestartButton;
        private SceneNavigationService _navigation;

        [Inject]
        public void Construct(SceneNavigationService navigation)
        {
            _navigation = navigation;
        }

        private void OnEnable()
        {
            _motivationService.Depleted += ShowDefeat;
            _motivationService.TypingCompleted += ShowVictory;
            _loseRestartButton.onClick.AddListener(OpenMenu);
            _winRestartButton.onClick.AddListener(OpenMenu);
            _panel.SetActive(false);
            _victoryPanel.SetActive(false);
            if (_motivationService.IsDepleted)
                ShowDefeat();
            else if (_motivationService.IsTypingCompleted)
                ShowVictory();
        }

        private void OnDisable()
        {
            if (_motivationService != null)
            {
                _motivationService.Depleted -= ShowDefeat;
                _motivationService.TypingCompleted -= ShowVictory;
            }
            if (_loseRestartButton != null)
                _loseRestartButton.onClick.RemoveListener(OpenMenu);
            if (_winRestartButton != null)
                _winRestartButton.onClick.RemoveListener(OpenMenu);
            if (_panel != null)
                _panel.SetActive(false);
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);
        }

        private void ShowDefeat()
        {
            CloseActivity();
            _victoryPanel.SetActive(false);
            _panel.SetActive(true);
        }

        private void ShowVictory()
        {
            CloseActivity();
            _panel.SetActive(false);
            _victoryPanel.SetActive(true);
        }

        private void CloseActivity()
        {
            _coordinator.CancelForcedOpen();
            _coordinator.CloseCurrent();
            _gameplayHud.SetActive(false);
        }

        private void OpenMenu()
        {
            _navigation.OpenMenu();
        }

    }
}
