using System;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class DropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private List<GameObject> _sandwichLayers;
    [SerializeField] private RectTransform _masterOfSandwich;
    
    private int _currentLayer = 0;
    
    public event Action OnGameFinished;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropedItem = eventData.pointerDrag;
        Ingredient ingredient = dropedItem?.GetComponent<Ingredient>();
        Draggable source = dropedItem?.GetComponent<Draggable>();
        
        if (ingredient is null || (ingredient.Layer != _currentLayer && _currentLayer < 2)) return;
        
        //_sandwichLayers[ingredient.Layer].SetActive(true);
        RectTransform layerTransform = Instantiate(_sandwichLayers[ingredient.Layer]).GetComponent<RectTransform>();
        layerTransform.SetParent(_masterOfSandwich, false);
        
        _currentLayer++;

        source.isEmpty = true;
        
        if (_currentLayer > _sandwichLayers.Count)
        {
            Debug.Log("Game finished");
            OnGameFinished?.Invoke();
        }
    }
}
