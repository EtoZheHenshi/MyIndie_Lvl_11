using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private List<GameObject> _sandwichLayers;
    
    private int _currentLayer = 0;
    
    public event Action OnGameFinished;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropedItem = eventData.pointerDrag;
        Ingredient ingredient = dropedItem?.GetComponent<Ingredient>();
        
        if (ingredient is null || (ingredient.Layer != _currentLayer && _currentLayer < 2)) return;
        
        _sandwichLayers[_currentLayer].SetActive(true);
        _currentLayer++;
        
        dropedItem.SetActive(false);
        
        if (_currentLayer > _sandwichLayers.Count)
        {
            Debug.Log("Game finished");
            OnGameFinished?.Invoke();
        }
    }
}
