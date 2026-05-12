using UnityEngine;

namespace Framework.UI.Core
{
    public abstract class UIBaseAnimation : MonoBehaviour
    {
        public virtual void OpenAnimation()
        {
            
        }

        public virtual void CloseAnimation(System.Action onComplete)
        {
            onComplete?.Invoke();
        }

        public virtual void Dispose()
        {
            
        }
    }
}