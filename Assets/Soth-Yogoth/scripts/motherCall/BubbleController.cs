using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = System.Random;

public class BubbleController : MonoBehaviour
{
    [SerializeField] private List<Bubble> bubbles;
    private Bubble _selectedBubble;

    private int _bubblesQ;
    
    Random _rand = new();
    
    public Action OnGameFinished;
    
    private void OnEnable()
    {
        Debug.Log("Начало игры: " + DateTime.Now);
        
        _bubblesQ = bubbles.Count;
        
        foreach (Bubble bubble in bubbles)
        {
            bubble.OnBubbleSelect += SelectBubble;
        }
        
        StartCoroutine(AppearBubbles());
    }

    private void Update()
    {
        if (_selectedBubble is null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            AnswerBubble(_selectedBubble.type == 1);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            AnswerBubble(_selectedBubble.type == 2);
        }
        else if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            AnswerBubble(_selectedBubble.type == 3);
        }
    }

    private IEnumerator AppearBubbles()
    {
        foreach (Bubble bubble in bubbles)
        {
            bubble.Appear();
            
            float delay = _rand.Next(1, 5) / 5f;
            yield return new WaitForSeconds(delay);
        }
    }

    private void SelectBubble(Bubble bubble)
    {
        _selectedBubble = bubble;
    }

    private void AnswerBubble(bool isMatch)
    {
        Debug.Log("isMatch: " + isMatch + " bubbles: " + _bubblesQ);
        
        if (isMatch)
        {
            _selectedBubble.Vanish();
            _bubblesQ--;
            
            if (_bubblesQ <= 0) Debug.Log("Конец игры: " + DateTime.Now);
        }
        else Debug.Log("Не стоило поднимать эту тему");
    }
    
    private void OnDisable()
    {
        foreach (Bubble bubble in bubbles)
        {
            bubble.OnBubbleSelect -= SelectBubble;
        }
    }
}
