using Framework.Core;

namespace Framework.UI
{
    public abstract class BaseProxy
    {
        public string ProxyName { get; private set; }
        
        public BaseProxy(string proxyName)
        {
            ProxyName = proxyName;
        }

        protected void SendEvent<T>(T eventParam) where T : class
        {
            EventManager.Instance.Send(eventParam);
        }
    }
}