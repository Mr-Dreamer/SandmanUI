using System;
using UnityEngine;

namespace Framework.Core
{
    public class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        private static T _instance;
        private static readonly object _lock = new object();

        public static T Instance
        {
            get
            {
                if (_instance != null)  return _instance;
                
                _instance = FindAnyObjectByType<T>();
                if (!_instance)
                {
                    lock (_lock)
                    {
                        if (!_instance)
                        {
                            _instance = new GameObject(typeof(T).FullName).AddComponent<T>();
                        }
                    }
                }
                return _instance;
            }
        }

        public static void Release()
        {
            if (_instance)
            {
                lock (_lock)
                {
                    if (_instance)
                    {
                        Destroy(_instance);
                        _instance = null;
                    }
                }
            }
        }
    }
}