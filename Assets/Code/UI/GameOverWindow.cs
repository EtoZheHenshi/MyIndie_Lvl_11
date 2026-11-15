using Code.Motivation;
using UnityEngine;

namespace Code.UI
{
    public class GameOverWindow : MonoBehaviour
    {
        [SerializeField] private MotivationService _motivationService;
        [SerializeField] private GameObject _panel;

        private void OnEnable()
        {
            _motivationService.Depleted += Show;
            _panel.SetActive(_motivationService.IsDepleted);
        }

        private void OnDisable()
        {
            if (_motivationService != null)
                _motivationService.Depleted -= Show;
            if (_panel != null)
                _panel.SetActive(false);
        }

        private void Show()
        {
            _panel.SetActive(true);
        }
    }
}
