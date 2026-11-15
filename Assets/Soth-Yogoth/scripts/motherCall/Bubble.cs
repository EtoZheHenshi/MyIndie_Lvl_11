using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Bubble : MonoBehaviour, IPointerDownHandler
{
    private Image _image;
    private Color _answerColor;
    private bool _inputEnabled;

    public int Type { get; private set; }
    public bool IsAnswered { get; private set; }
    public bool IsRevealed { get; private set; }
    public event Action<Bubble> OnBubbleSelect;

    public void ResetBubble(int answerType, Color answerColor)
    {
        if (_image == null)
            _image = GetComponent<Image>();

        Type = answerType;
        _answerColor = answerColor;
        _image.color = Color.white;
        IsAnswered = false;
        IsRevealed = false;
        _inputEnabled = false;
        gameObject.SetActive(false);
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
    }

    public void Appear()
    {
        if (!IsAnswered)
            gameObject.SetActive(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_inputEnabled && isActiveAndEnabled && !IsAnswered &&
            eventData.button == PointerEventData.InputButton.Left)
        {
            OnBubbleSelect?.Invoke(this);
        }
    }

    public void Reveal()
    {
        _image.color = _answerColor;
        IsRevealed = true;
    }

    public void Vanish()
    {
        IsAnswered = true;
        _inputEnabled = false;
        gameObject.SetActive(false);
    }
}
