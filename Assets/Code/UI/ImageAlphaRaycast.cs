using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    [RequireComponent(typeof(Image))]
    public class ImageAlphaRaycast : MonoBehaviour
    {
        [SerializeField, Range(0.01f, 1f)]
        private float _threshold = 0.1f;

        private void Awake()
        {
            GetComponent<Image>().alphaHitTestMinimumThreshold = _threshold;
        }
    }
}
