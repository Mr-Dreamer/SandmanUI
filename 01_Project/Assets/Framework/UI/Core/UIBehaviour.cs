using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Build.Content;
using UnityEngine;

namespace Framework.UI.Core
{
    [AddComponentMenu("UI/UI Behaviour")]
    public class UIBehaviour : MonoBehaviour
    {
        [HideInInspector]
        public List<string> KeyList = new();
        [HideInInspector]
        public List<UnityEngine.Object> ValueList = new();

        private Dictionary<string, UnityEngine.Object> _componentCache;

        private void Awake()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            if (KeyList == null || ValueList == null)
            {
                _componentCache = new(8);
                return;
            }
            
            _componentCache = new(KeyList.Count);
            int validCount = 0;
            for (int i = 0; i < KeyList.Count && i < ValueList.Count; i++)
            {
                string key = KeyList[i];
                var value = ValueList[i];
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }

                if (value == null)
                {
                    continue;
                }

                if (_componentCache.ContainsKey(key))
                {
                    continue;
                }
                
                _componentCache[key] = value;
                validCount++;
            }
            
            Debug.Log($"{validCount} components found");
        }

        public T Get<T>(string key) where T : UnityEngine.Object
        {
            if (_componentCache == null)
            {
                BuildCache();
            }

            if (_componentCache != null && _componentCache.TryGetValue(key, out var component))
            {
                if (component is T t)
                {
                    return t;
                }
                
                Debug.LogError($"Component {key} not found");
                return null;
            }
            
            return null;
        }

        public void ClearCache()
        {
            _componentCache?.Clear();
            _componentCache = null;
        }

        public void RebuildCache()
        {
            ClearCache();
            BuildCache();
        }
        
        #if UNITY_EDITOR
        private void OnValidate()
        {
            if (KeyList != null && ValueList != null)
            {
                if (KeyList.Count != ValueList.Count)
                {
                    Debug.LogError($"{KeyList.Count} not equal to {ValueList.Count}");
                }
            }
        }
        #endif
    }
}