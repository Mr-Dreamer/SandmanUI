using System;
using System.Collections.Generic;

namespace Framework.Core
{
    public class EventManager : Singleton<EventManager>
    {
        public Dictionary<Type, Delegate> EventDic = new();

        private static readonly Dictionary<Type, Type> TypeCache = new(64);

        // private static Type GetCacheType<T>()
        // {
        //     var type = typeof(T);
        //     if (!TypeCache.TryGetValue(type, out var cacheType))
        //     {
        //         cacheType = type;
        //         TypeCache[type] = cacheType;
        //     }
        //     return cacheType;
        // }

        /// <summary>
        /// 发送事件
        /// </summary>
        /// <param name="eventData"></param>
        /// <typeparam name="T"></typeparam>
        public void Send<T>(T eventData) where T: class
        {
            var type = typeof(T);
            if (EventDic.TryGetValue(type, out Delegate handler) && handler is Action<T> action)
            {
                action.Invoke(eventData);
            }
        }

        public void Listen<T>(Action<T> callback) where T : class
        {
            var type = typeof(T);
            if (!EventDic.TryGetValue(type, out Delegate handler))
            {
                EventDic[type] = callback;
            }
            else
            {
                EventDic[type] = Delegate.Combine(handler, callback);
            }
        }
        
        public void UnListen<T>(Action<T> callback) where T : class
        {
            var type = typeof(T);
            if (EventDic.TryGetValue(type, out Delegate handler))
            {
                var newDelegate = Delegate.Remove(handler, callback);
                if (newDelegate == null)
                {
                    EventDic.Remove(type);
                }
                else
                {
                    EventDic[type] = newDelegate;
                }
            }
        }

        public void Clear()
        {
            EventDic.Clear();
        }
    }
}