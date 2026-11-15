using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class ContentController : MonoBehaviour
{
    [SerializeField] private List<GameObject> viralImagePrefabs;
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform leftColumn;
    [SerializeField] private RectTransform rightColumn;
    [SerializeField, Min(1)] private int likesToFinish = 3;
    [SerializeField, Min(0f)] private float scrollSpeed = 1f;
    [SerializeField] private Image[] targetPreviews;
    [SerializeField] private GameObject[] foundMarkers;

    private readonly List<ViralPicture> _pictures = new List<ViralPicture>();
    private readonly List<ViralPicture> _targets = new List<ViralPicture>();
    private Vector2 _initialPosition;
    private float _maximumScroll;
    private bool _hasInitialPosition;
    private bool _layoutPending;
    private bool _inputEnabled;

    public int RemainingLikes { get; private set; }
    public bool IsFinished { get; private set; }
    public event Action GameFinished;

    public void ResetGame()
    {
        SetInputEnabled(false);
        ClearPictures();
        if (viralImagePrefabs == null || viralImagePrefabs.Count == 0)
            throw new InvalidOperationException("Assign picture prefabs to the lenta content.");

        if (!_hasInitialPosition)
        {
            _initialPosition = container.anchoredPosition;
            _hasInitialPosition = true;
        }
        container.anchoredPosition = _initialPosition;
        IsFinished = false;

        var available = new List<GameObject>(viralImagePrefabs);
        int leftCount = available.Count / 2;
        int totalCount = available.Count;
        for (int i = 0; i < totalCount; i++)
        {
            int index = Random.Range(0, available.Count);
            GameObject prefab = available[index];
            available.RemoveAt(index);
            GameObject instance = Instantiate(prefab, i < leftCount ? leftColumn : rightColumn, false);
            ViralPicture picture = instance.GetComponent<ViralPicture>();
            if (picture == null)
                throw new InvalidOperationException($"Picture prefab '{prefab.name}' requires ViralPicture.");
            picture.SetInputEnabled(false);
            _pictures.Add(picture);
        }

        RemainingLikes = Mathf.Min(likesToFinish, _pictures.Count);
        if (targetPreviews == null || targetPreviews.Length < RemainingLikes ||
            foundMarkers == null || foundMarkers.Length < RemainingLikes)
            throw new InvalidOperationException("Assign a preview and found marker for each lenta target.");
        for (int i = 0; i < targetPreviews.Length; i++)
            targetPreviews[i].gameObject.SetActive(i < RemainingLikes);
        foreach (GameObject marker in foundMarkers)
            marker.SetActive(false);
        var candidates = new List<ViralPicture>(_pictures);
        for (int i = 0; i < RemainingLikes; i++)
        {
            int index = Random.Range(0, candidates.Count);
            ViralPicture target = candidates[index];
            candidates.RemoveAt(index);
            target.MakeTarget();
            target.OnTargetLiked += TargetLiked;
            _targets.Add(target);
            targetPreviews[i].sprite = target.Sprite;
            targetPreviews[i].color = Color.white;
        }
        _layoutPending = true;
    }

    public void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled && !IsFinished;
        foreach (ViralPicture picture in _pictures)
            if (picture != null)
                picture.SetInputEnabled(_inputEnabled);
    }

    public void Scroll(float amount)
    {
        if (!_inputEnabled || IsFinished || !container.gameObject.activeInHierarchy)
            return;
        if (_layoutPending)
            RefreshLayout();
        Vector2 position = container.anchoredPosition;
        position.y = Mathf.Clamp(position.y + amount, _initialPosition.y,
            _initialPosition.y + _maximumScroll);
        container.anchoredPosition = position;
    }

    private void Update()
    {
        if (_layoutPending && container.gameObject.activeInHierarchy)
            RefreshLayout();
        if (!_inputEnabled || !Application.isFocused || Mouse.current == null)
            return;
        Scroll(Mouse.current.scroll.ReadValue().y * scrollSpeed);
    }

    private void RefreshLayout()
    {
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(container);
        var viewport = (RectTransform)container.parent;
        _maximumScroll = Mathf.Max(0f, container.rect.height - viewport.rect.height);
        _layoutPending = false;
    }

    private void TargetLiked()
    {
        if (!_inputEnabled || IsFinished)
            return;
        RemainingLikes--;
        for (int i = 0; i < _targets.Count; i++)
            foundMarkers[i].SetActive(!_targets[i].IsTarget);
        if (RemainingLikes > 0)
            return;

        IsFinished = true;
        SetInputEnabled(false);
        GameFinished?.Invoke();
    }

    private void ClearPictures()
    {
        foreach (ViralPicture target in _targets)
            if (target != null)
                target.OnTargetLiked -= TargetLiked;
        _targets.Clear();
        foreach (ViralPicture picture in _pictures)
        {
            if (picture == null)
                continue;
            picture.gameObject.SetActive(false);
            Destroy(picture.gameObject);
        }
        _pictures.Clear();
    }

    private void OnDestroy()
    {
        ClearPictures();
    }
}
