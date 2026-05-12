using Framework.Core;
using Framework.UI.Core;

namespace Framework.UI.Controller
{
    public abstract class BaseController<TView> : IController where TView : UIBaseView
    {
        protected TView View;

        public BaseController(TView view)
        {
            View = view;
        }

        public abstract void OnRegister(object param = null);

        public abstract void OnRemove();

        public virtual void OnActive(bool isActive)
        {
        }

        public virtual void OnEscape()
        {
        }

        protected void ExcuteCommand<TCommand>(object param = null) where TCommand : BaseCommand, new()
        {
            // TODO
        }

        protected void ListenEvent<TEvent>(System.Action<TEvent> callback) where TEvent : class
        {
            EventManager.Instance.Listen(callback);
        }

        protected void UnListenEvent<TEvent>(System.Action<TEvent> callback) where TEvent : class
        {
            EventManager.Instance.UnListen(callback);
        }

        protected void CloseView()
        {
            // TODO 关闭
        }
    }
}