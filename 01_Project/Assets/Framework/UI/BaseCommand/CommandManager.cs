using Framework.Core;

namespace Framework.UI
{
    public class CommandManager : Singleton<CommandManager>
    {
        public void Excute<TCommand>(object param = null) where TCommand : BaseCommand, new()
        {
            var comman = new TCommand();
            comman.Excute(param);
        }
    }
}