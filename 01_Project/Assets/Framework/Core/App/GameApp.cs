using Framework.UI;

namespace Framework.Core
{
    public class GameApp
    {
        public static UIManager UI => UIManager.Instance;
        public static ResManager Res => ResManager.Instance;
        public static EventManager Event => EventManager.Instance;
        public static CommandManager Command => CommandManager.Instance;
        public static ProxyManager Proxy => ProxyManager.Instance;
    }
}