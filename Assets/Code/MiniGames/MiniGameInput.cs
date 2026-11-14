using UnityEngine;

namespace Code.MiniGames
{
    public abstract class MiniGameInput : MonoBehaviour
    {
        protected virtual void OnEnable()
        {
            Subscribe();
        }

        protected virtual void OnDisable()
        {
            Unsubscribe();
        }

        protected abstract void Subscribe();
        protected abstract void Unsubscribe();
    }
}
