using UnityEngine;

namespace Framework.UI.Core
{
    public abstract class UIBaseView
    {
        public string Location { get; set; }
        
        public RectTransform NodeRect { get; set; }

        public virtual void Inject(UIBehaviour behaviour)
        {
            
        }

        public virtual void Dispose()
        {
            
        }
    }
}