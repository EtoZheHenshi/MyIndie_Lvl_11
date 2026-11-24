using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Code.Audio;
using Zenject;

public class BubbleController : MonoBehaviour
{
    [SerializeField] private List<Bubble> bubbles;

    [Header("Answer colors")]
    [SerializeField] private Color _answerOneColor = Color.red;
    [SerializeField] private Color _answerTwoColor = new Color(1f, 0.7f, 0.38f);
    [SerializeField] private Color _answerThreeColor = Color.green;

    [Header("Wrong answer penalty")]
    [SerializeField] private Image _errorIndicator;
    [SerializeField, Min(0f)] private float _errorIndicatorDuration = 1f;
    [SerializeField, Min(0f)] private float _wrongAnswerInputLockDuration = 1f;
    private float _errorIndicatorRemaining;
    private float _inputLockRemaining;
    private AudioService _audio;
    private Bubble _selectedBubble;
    private Coroutine _appearance;
    private bool _inputEnabled;
    private bool _conversationStarted;

    public int RemainingBubbles { get; private set; }
    public bool IsFinished { get; private set; }
    public bool IsInputLocked => _inputLockRemaining > 0f;
    public event Action OnGameFinished;

    [Inject]
    public void Construct(AudioService audio)
    {
        _audio = audio;
    }

    public void ResetGame()
    {
        StopAppearance();
        ClearWrongAnswerPenalty();
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
        _conversationStarted = false;
    }

    public void StartConversation()
    {
        if (_conversationStarted || IsFinished)
            return;
        _conversationStarted = true;
        _appearance = StartCoroutine(AppearBubbles());
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled && !IsFinished;
        if (bubbles == null)
            return;
        foreach (Bubble bubble in bubbles)
            if (bubble != null)
                bubble.SetInputEnabled(_inputEnabled && _conversationStarted && !IsInputLocked);
    }

    public bool Answer(int answerType)
    {
        if (!_inputEnabled || !_conversationStarted || IsInputLocked || IsFinished || _selectedBubble == null ||
            !_selectedBubble.isActiveAndEnabled || _selectedBubble.IsAnswered)
            return false;
        if (_selectedBubble.Type != answerType)
        {
            ShowInputError();
            return false;
        }

        Bubble answered = _selectedBubble;
        _selectedBubble = null;
        answered.Vanish();
        RemainingBubbles--;
        if (RemainingBubbles == 0)
        {
            IsFinished = true;
            StopAppearance();
            ClearWrongAnswerPenalty();
            SetInputEnabled(false);
            OnGameFinished?.Invoke();
        }
        return true;
    }

    public void ShowInputError()
    {
        if (!_inputEnabled || IsInputLocked || IsFinished)
            return;

        _audio.Play("SFX/OnMomError");
        _inputLockRemaining = _wrongAnswerInputLockDuration;
        _errorIndicatorRemaining = _errorIndicatorDuration;
        if (_errorIndicator != null)
            _errorIndicator.enabled = _errorIndicatorRemaining > 0f;
        SetInputEnabled(_inputEnabled);
    }

    private void Update()
    {
        // Input is disabled by the mini-game window during pause, so penalty
        // timers pause together with the conversation and resume afterwards.
        if (!_inputEnabled)
            return;

        if (_inputLockRemaining > 0f)
        {
            _inputLockRemaining = Mathf.Max(0f, _inputLockRemaining - Time.deltaTime);
            if (!IsInputLocked)
                SetInputEnabled(_inputEnabled);
        }
        if (_errorIndicatorRemaining > 0f)
        {
            _errorIndicatorRemaining = Mathf.Max(0f, _errorIndicatorRemaining - Time.deltaTime);
            if (_errorIndicator != null)
            {
                float elapsed = _errorIndicatorDuration - _errorIndicatorRemaining;
                _errorIndicator.enabled = _errorIndicatorRemaining > 0f &&
                    Mathf.FloorToInt(elapsed / 0.12f) % 2 == 0;
            }
        }
    }

    private void ClearWrongAnswerPenalty()
    {
        _inputLockRemaining = 0f;
        _errorIndicatorRemaining = 0f;
        if (_errorIndicator != null)
            _errorIndicator.enabled = false;
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
        if (!_inputEnabled || !_conversationStarted || IsInputLocked || IsFinished || _selectedBubble != null ||
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
        ClearWrongAnswerPenalty();
        SetInputEnabled(false);
        Unsubscribe();
    }
}
