using UnityEngine;

namespace Code.WalkingMiniGame
{
    [RequireComponent(typeof(RectTransform))]
    public class WalkingActor : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        private RectTransform _rect;

        public RectTransform Rect => _rect != null ? _rect : _rect = (RectTransform)transform;
        public int Lane { get; private set; }
        public float X => Rect.anchoredPosition.x;
        public float Y => Rect.anchoredPosition.y;
        public float Width => Rect.rect.width;
        public float Height => Rect.rect.height;

        public void Place(int lane, float x, float y)
        {
            Lane = lane;
            Rect.anchoredPosition = new Vector2(x, y);
        }

        public void SetAnimationRunning(bool running)
        {
            _animator.speed = running ? 1f : 0f;
        }
    }
}
