using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image _image;
    [SerializeField] private RectTransform _rectTransform;

    private PointerEventData _dragEvent;

    public bool isEmpty = false;

    public bool IsDragging { get; private set; }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || isEmpty || IsDragging)
            return;

        _dragEvent = eventData;
        IsDragging = true;

        _rectTransform.position = eventData.position;
        SetImageVisible(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || !IsDragging || isEmpty)
            return;

        _rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CancelDrag();
        if (isEmpty)
            gameObject.SetActive(false);
    }

    public void ResetIngredient()
    {
        CancelDrag();
        isEmpty = false;
        gameObject.SetActive(true);
    }

    public void CancelDrag()
    {
        if (_dragEvent != null && _dragEvent.pointerDrag == gameObject)
        {
            _dragEvent.pointerDrag = null;
            _dragEvent.dragging = false;
            _dragEvent.eligibleForClick = false;
        }

        _dragEvent = null;
        IsDragging = false;
        SetImageVisible(false);
    }

    private void OnDisable()
    {
        CancelDrag();
    }

    private void SetImageVisible(bool visible)
    {
        if (_image == null)
            return;

        Color color = _image.color;
        color.a = visible ? 1f : 0f;
        _image.color = color;
    }
}
