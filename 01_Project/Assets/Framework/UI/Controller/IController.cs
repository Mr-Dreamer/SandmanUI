namespace Framework.UI.Controller
{
    public interface IController
    {
        /// <summary> UI和事件的监听注册 </summary>
        void OnRegister(object param = null);
        /// <summary> UI和事件的监听移除 </summary>
        void OnRemove();
        /// <summary> UI面板的激活 </summary>
        void OnActive(bool isActive);
        /// <summary> esc键位的响应 </summary>
        void OnEscape();
    }
}