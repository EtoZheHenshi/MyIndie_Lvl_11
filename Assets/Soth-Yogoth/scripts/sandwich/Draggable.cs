using UnityEngine;
using UnityEngine.EventSystems;

public class draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] RectTransform frame;
    
    RectTransform _rectTransform;
    Canvas _canvas;
    
    private Vector2 _startPosition;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvas = GetComponentInParent<Canvas>();
        
        _startPosition = _rectTransform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        
    }

    public void OnDrag(PointerEventData eventData)
    {
        _rectTransform.position = eventData.position / _canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (enabled) _rectTransform.position = _startPosition;
    }
}
