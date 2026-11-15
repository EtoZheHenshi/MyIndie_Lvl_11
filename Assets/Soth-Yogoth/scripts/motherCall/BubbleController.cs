using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleController : MonoBehaviour
{
    [SerializeField] private List<Bubble> bubbles;

    [Header("Answer colors")]
    [SerializeField] private Color _answerOneColor = Color.red;
    [SerializeField] private Color _answerTwoColor = new Color(1f, 0.7f, 0.38f);
    [SerializeField] private Color _answerThreeColor = Color.green;
    private Bubble _selectedBubble;
    private Coroutine _appearance;
    private bool _inputEnabled;

    public int RemainingBubbles { get; private set; }
    public bool IsFinished { get; private set; }
    public event Action OnGameFinished;

    public void ResetGame()
    {
        StopAppearance();
        Unsubscribe();
        if (bubbles == null || bubbles.Count == 0)
            throw new InvalidOperationException("Assign bubbles to the mother call game.");

        var assigned = new HashSet<Bubble>();
        foreach (Bubble bubble in bubbles)
        {
            if (bubble == null || !assigned.Add(bubble))
                throw new InvalidOperationException("Mother call requires distinct, assigned bubbles.");
            int answerType = UnityEngine.Random.Range(1, 4);
            bubble.ResetBubble(answerType, GetAnswerColor(answerType));
            bubble.OnBubbleSelect += SelectBubble;
        }

        _selectedBubble = null;
        _inputEnabled = false;
        RemainingBubbles = bubbles.Count;
        IsFinished = false;
        _appearance = StartCoroutine(AppearBubbles());
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled && !IsFinished;
        if (bubbles == null)
            return;
        foreach (Bubble bubble in bubbles)
            if (bubble != null)
                bubble.SetInputEnabled(_inputEnabled);
    }

    public bool Answer(int answerType)
    {
        if (!_inputEnabled || IsFinished || _selectedBubble == null ||
            !_selectedBubble.isActiveAndEnabled || _selectedBubble.IsAnswered)
            return false;
        if (_selectedBubble.Type != answerType)
            return false;

        Bubble answered = _selectedBubble;
        _selectedBubble = null;
        answered.Vanish();
        RemainingBubbles--;
        if (RemainingBubbles == 0)
        {
            IsFinished = true;
            StopAppearance();
            SetInputEnabled(false);
            OnGameFinished?.Invoke();
        }
        return true;
    }

    private IEnumerator AppearBubbles()
    {
        foreach (Bubble bubble in bubbles)
        {
            bubble.Appear();
            yield return new WaitForSeconds(UnityEngine.Random.Range(1, 5) / 5f);
        }
        _appearance = null;
    }

    private Color GetAnswerColor(int answerType)
    {
        switch (answerType)
        {
            case 1: return _answerOneColor;
            case 2: return _answerTwoColor;
            case 3: return _answerThreeColor;
            default: throw new ArgumentOutOfRangeException(nameof(answerType));
        }
    }

    private void SelectBubble(Bubble bubble)
    {
        if (!_inputEnabled || IsFinished || _selectedBubble != null ||
            bubble.IsAnswered || !bubble.isActiveAndEnabled)
            return;

        _selectedBubble = bubble;
        bubble.Reveal();
    }

    private void StopAppearance()
    {
        if (_appearance == null)
            return;
        StopCoroutine(_appearance);
        _appearance = null;
    }

    private void Unsubscribe()
    {
        if (bubbles == null)
            return;
        foreach (Bubble bubble in bubbles)
            if (bubble != null)
                bubble.OnBubbleSelect -= SelectBubble;
    }

    private void OnDisable()
    {
        StopAppearance();
        SetInputEnabled(false);
        Unsubscribe();
    }
}
