using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViralPicture : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private GameObject likeView;
    private bool isTarget = false;
    private bool _inputEnabled;
    private bool _hasLiked;
    
    public bool IsTarget => isTarget;

    public event Action OnTargetLiked;

    public void MakeTarget()
    {
        isTarget = true;
        gameObject.GetComponent<Image>().color = Color.green;
        
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!_inputEnabled || !isActiveAndEnabled || _hasLiked ||
            eventData.button != PointerEventData.InputButton.Left)
            return;

        _hasLiked = true;
        likeView?.SetActive(true);
        
        if (isTarget)
        {
            isTarget = false;
            OnTargetLiked?.Invoke();
        }
    }
}
