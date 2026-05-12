


using UnityEngine;

namespace Framework.Core
{
    public class ResManager :Singleton<ResManager>
    {
        public T LoadAsset<T>(string location, string group = null) where T : UnityEngine.Object
        {
            return Resources.Load<T>(location);
        }
    }
}