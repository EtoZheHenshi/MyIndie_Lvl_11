using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropArea : MonoBehaviour, IDropHandler
{
    [SerializeField] private List<GameObject> _sandwichLayers;
    [SerializeField] private RectTransform _masterOfSandwich;

    private readonly List<GameObject> _createdLayers = new List<GameObject>();
    private int _currentLayer;
    private bool _isFinished;

    public event Action OnGameFinished;

    public void OnDrop(PointerEventData eventData)
    {
        if (!isActiveAndEnabled || _isFinished)
            return;

        GameObject droppedItem = eventData.pointerDrag;
        if (droppedItem == null)
            return;

        Ingredient ingredient = droppedItem.GetComponent<Ingredient>();
        Draggable source = droppedItem.GetComponent<Draggable>();
        if (ingredient == null || source == null || source.isEmpty ||
            !source.isActiveAndEnabled || !source.IsDragging)
            return;

        int layerIndex = ingredient.Layer;
        if (layerIndex < 0 || layerIndex >= _sandwichLayers.Count)
        {
            Debug.LogError(
                $"Ingredient '{ingredient.name}' has invalid Layer {layerIndex}. " +
                $"Expected an index from 0 to {_sandwichLayers.Count - 1} in _sandwichLayers.",
                ingredient);
            return;
        }

        // Bread first, then sauce. The remaining ingredients can be added in any order.
        if (_currentLayer < 2 && layerIndex != _currentLayer)
            return;

        GameObject layer = Instantiate(_sandwichLayers[layerIndex], _masterOfSandwich, false);
        _createdLayers.Add(layer);
        source.isEmpty = true;
        _currentLayer++;

        if (_currentLayer >= _sandwichLayers.Count)
        {
            _isFinished = true;
            OnGameFinished?.Invoke();
        }
    }

    public void ResetGame()
    {
        foreach (GameObject layer in _createdLayers)
        {
            if (layer == null)
                continue;

            layer.SetActive(false);
            Destroy(layer);
        }

        _createdLayers.Clear();
        _currentLayer = 0;
        _isFinished = false;
    }
}
