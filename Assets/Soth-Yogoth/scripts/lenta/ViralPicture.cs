using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViralPicture : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject likeView;
    private bool isTarget = false;
    
    public bool IsTarget => isTarget;

    public Action OnTargetLiked;

    public void MakeTarget()
    {
        isTarget = true;
        gameObject.GetComponent<Image>().color = Color.green;
        
        Debug.Log("hi");
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        likeView?.SetActive(true);
        
        if (isTarget)
        {
            OnTargetLiked?.Invoke();
            isTarget = false;
        }
    }
}
