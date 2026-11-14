using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Bubble : MonoBehaviour, IPointerDownHandler
{
    [SerializeField][Range(1,3)] public int type = 2;
    
    private static Bubble _selectedBubble;
    
    public Action<Bubble> OnBubbleSelect;
    
    public void Appear()
    {
        gameObject.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnBubbleSelect?.Invoke(this);
    }
    
    public void Vanish()
    {
        gameObject.SetActive(false);
    }
}
