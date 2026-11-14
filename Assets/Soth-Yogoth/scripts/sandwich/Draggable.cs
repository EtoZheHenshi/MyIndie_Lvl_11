using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] Image _image;
    [SerializeField] RectTransform _rectTransform;
    private Vector2 _startPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // _rectTransform = GetComponent<RectTransform>();
        // _image = GetComponent<Image>();
        _startPosition = _rectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 1);
        Debug.Log("OnBeginDrag: " + _startPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = eventData.position;
        
        Debug.Log("OnDrag: " + eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enabled)
        {
            _rectTransform.position = _startPosition;
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, 0);
        }
    }
}
