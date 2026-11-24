using UnityEngine;

namespace Code.WalkingMiniGame
{
    public class WalkingConversationBubbles : MonoBehaviour
    {
        [SerializeField] private Animator _playerBubble;
        [SerializeField] private Animator _passerBubble;
        [SerializeField] private RuntimeAnimatorController[] _variants;
        [SerializeField] private Vector2 _playerOffset = new Vector2(-30f, 225f);
        [SerializeField] private Vector2 _passerOffset = new Vector2(40f, 225f);

        public void Show(WalkingActor player, WalkingActor passer)
        {
            ShowBubble(_playerBubble, player, _playerOffset, false);
            ShowBubble(_passerBubble, passer, _passerOffset, true);
        }

        private void ShowBubble(Animator bubble, WalkingActor actor, Vector2 offset, bool mirrored)
        {
            bubble.transform.position = actor.Rect.TransformPoint(new Vector3(offset.x, offset.y, 0f));
            bubble.transform.localScale = new Vector3(mirrored ? -1f : 1f, 1f, 1f);
            bubble.runtimeAnimatorController = _variants[Random.Range(0, _variants.Length)];
            bubble.gameObject.SetActive(true);
            bubble.Play(0, 0, 0f);
            bubble.Update(0f);
        }

        public void Hide()
        {
            if (_playerBubble != null) _playerBubble.gameObject.SetActive(false);
            if (_passerBubble != null) _passerBubble.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            Hide();
        }
    }
}
