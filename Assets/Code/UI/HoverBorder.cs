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

        private void Awake()
        {
            _border.raycastTarget = false;
            _border.enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _border.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _border.enabled = false;
        }

        private void OnDisable()
        {
            if (_border != null)
                _border.enabled = false;
        }
    }
}
