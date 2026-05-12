using System;
using System.Windows.Input;
using Framework.Core;

namespace Framework.UI
{
    public abstract class BaseCommand : IBaseCommand
    {
        public abstract void Excute(object param = null);

        protected void SendEvent<T>(T eventParam) where T : class
        {
            EventManager.Instance.Send(eventParam);
        }

        // protected TProxy GetProxy<TProxy>() where TProxy : BaseProxy, new()
        // {
        //     return null; // TODO
        // }

        protected void ExcuteCommand<TCommand>(object param = null) where TCommand : BaseCommand, new()
        {
            // TODO
            CommandManager.Instance.Excute<TCommand>(param);
        }
    }
}