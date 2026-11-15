using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Code.UI
{
    public class HoverBorder : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private Image _border;
        [SerializeField] private Animator _animator;

        private void Awake()
        {
            _border.raycastTarget = false;
            HideBorder();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _border.enabled = true;

            if (_animator != null)
            {
                _animator.enabled = true;
                _animator.Play(0, 0, 0f);
                _animator.Update(0f);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideBorder();
        }

        private void OnDisable()
        {
            HideBorder();
        }

        private void HideBorder()
        {
            if (_animator != null)
                _animator.enabled = false;

            if (_border != null)
                _border.enabled = false;
        }
    }
}
