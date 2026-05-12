using System;
using System.Collections.Generic;
using Framework.Core;

namespace Framework.UI
{
    public class ProxyManager : Singleton<ProxyManager>
    {
        private Dictionary<Type, BaseProxy> _proxies = new(16);

        public TProxy GetProxy<TProxy>() where TProxy : BaseProxy, new()
        {
            var type = typeof(TProxy);
            if (_proxies.TryGetValue(type, out var proxy))
            {
                return (TProxy)proxy;
            }
            
            proxy = new TProxy();
            _proxies[type] = proxy;
            
            return  (TProxy)proxy;
        }

        public void Clear()
        {
            _proxies.Clear();
        }
    }
}