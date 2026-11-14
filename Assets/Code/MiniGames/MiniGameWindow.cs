using UnityEngine;

namespace Code.MiniGames
{
    public abstract class MiniGameWindow : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private MiniGameInput _input;

        public bool IsOpen { get; private set; }

        protected virtual void Awake()
        {
            _input.enabled = false;
            _panel.SetActive(false);
        }

        public void Show()
        {
            if (IsOpen)
                return;

            _panel.SetActive(true);
            IsOpen = true;
            _input.enabled = true;
        }

        public void Hide()
        {
            _input.enabled = false;
            IsOpen = false;
            _panel.SetActive(false);
        }
    }
}
