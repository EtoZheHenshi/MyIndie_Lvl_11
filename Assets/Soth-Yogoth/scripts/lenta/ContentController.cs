using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class ContentController : MonoBehaviour
{
    private Vector2 scrollLimit;
    
    [SerializeField] private List<GameObject> viralImagePrefabs;
    private List<GameObject> viralImageInstances = new List<GameObject>();
    
    private List<int> targetIndecies = new List<int>();
    private List<ViralPicture> targetPictures = new List<ViralPicture>();
    
    [SerializeField] private RectTransform container;
    [SerializeField] private RectTransform leftColumn;
    [SerializeField] private RectTransform rightColumn;

    private int likesToFinish = 3;
    
    void Start()
    {
        Debug.Log("начадл игры: " + DateTime.Now);
        
        SetTargets();
        Fill();
        
        Canvas.ForceUpdateCanvases();

        scrollLimit.x = container.localPosition.y - 10;
        scrollLimit.y = container.localPosition.y + container.rect.height - 10;
    }

    void Update()
    {
        if (Mouse.current ==  null) return;

        float scroll = Mouse.current.scroll.ReadValue().y * 10;
        
        if ((scroll > 0 && container.localPosition.y + scroll < scrollLimit.y) ||
        (scroll < 0 && container.localPosition.y + scroll > scrollLimit.x))
        {
            container.localPosition = new Vector2(container.localPosition.x, container.localPosition.y + scroll);
        }
    }

    private void Fill()
    {
        if (viralImagePrefabs.Count <= 0) return;
        
        int leftSize = viralImagePrefabs.Count / 2;
        int rightSize = viralImagePrefabs.Count - leftSize;
        
        for (int i = 0; i < leftSize; i++)
        {
            int index = Random.Range(0, viralImagePrefabs.Count);
            
            GameObject instance = Instantiate(viralImagePrefabs[index], leftColumn, false);
            viralImageInstances.Add(instance);
            
            viralImagePrefabs.RemoveAt(index);
        }
        
        for (int i = 0; i < rightSize; i++)
        {
            int index = Random.Range(0, viralImagePrefabs.Count);
            
            GameObject instance = Instantiate(viralImagePrefabs[index], rightColumn, false);
            viralImageInstances.Add(instance);
            
            viralImagePrefabs.RemoveAt(index);
        }
        
        for (int i = 0; i < targetIndecies.Count; i++)
        {
            ViralPicture picture = viralImageInstances[i].GetComponent<ViralPicture>();
            
            picture.MakeTarget();
            picture.OnTargetLiked += TargetLiked;
            
            targetPictures.Add(picture);
        }
    }
    
    private void SetTargets()
    {
        List<int> numbers = Enumerable.Range(0, viralImagePrefabs.Count).ToList();

        for (int i = 0; i < likesToFinish; i++)
        {
            int index = Random.Range(0, numbers.Count);
            
            targetIndecies.Add(numbers[index]);
            
            numbers.RemoveAt(index);
        }
    }

    private void TargetLiked()
    {
        likesToFinish--;
        
        Debug.Log(likesToFinish);
        
        if (likesToFinish == 0) Debug.Log("конец игры: " + DateTime.Now);
    }

    private void OnDisable()
    {
        foreach (ViralPicture picture in targetPictures)
        {
            picture.OnTargetLiked -= TargetLiked;
        }
    }
}
