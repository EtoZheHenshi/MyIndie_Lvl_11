using Code.Motivation;
using Code.Infrastructure;
using Code.MiniGames;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using TMPro;

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

        [Header("Completed fanfic")]
        [SerializeField] private Button _readFanficButton;
        [SerializeField] private GameObject _fanficPanel;
        [SerializeField] private Button _closeFanficButton;
        [SerializeField] private ScrollRect _fanficScroll;
        [SerializeField] private TMP_Text _fanficText;
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
            _readFanficButton.onClick.AddListener(ShowFanfic);
            _closeFanficButton.onClick.AddListener(CloseFanfic);
            _fanficPanel.SetActive(false);
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
            if (_readFanficButton != null)
                _readFanficButton.onClick.RemoveListener(ShowFanfic);
            if (_closeFanficButton != null)
                _closeFanficButton.onClick.RemoveListener(CloseFanfic);
            if (_fanficPanel != null)
                _fanficPanel.SetActive(false);
            if (_panel != null)
                _panel.SetActive(false);
            if (_victoryPanel != null)
                _victoryPanel.SetActive(false);
        }

        private void ShowDefeat()
        {
            CloseActivity();
            _fanficPanel.SetActive(false);
            _victoryPanel.SetActive(false);
            _panel.SetActive(true);
        }

        private void ShowVictory()
        {
            CloseActivity();
            _fanficPanel.SetActive(false);
            _panel.SetActive(false);
            _victoryPanel.SetActive(true);
        }

        private void ShowFanfic()
        {
            if (!_motivationService.IsTypingCompleted || !_victoryPanel.activeSelf)
                return;

            // Read the same source as the typing game, preserving its paragraphs.
            _fanficText.text = Code.TypingLogic.TextReader.LoadText();
            _victoryPanel.SetActive(false);
            _fanficPanel.SetActive(true);
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_fanficScroll.content);
            _fanficScroll.StopMovement();
            _fanficScroll.verticalNormalizedPosition = 1f;
        }

        private void CloseFanfic()
        {
            _fanficPanel.SetActive(false);
            if (_motivationService.IsTypingCompleted)
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
