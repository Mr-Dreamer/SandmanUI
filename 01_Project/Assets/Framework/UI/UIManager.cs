using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.UI.Controller;
using Framework.UI.Core;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.UI
{
    public class UIManager : Singleton<UIManager>
    {
        private const string TAG = "[UIManager]";
        public Camera UICamera { get; private set; }
        public UIRoot CurUIRoot { get; private set; }
        
        /// <summary> 存储location 2 ControllerInfo的映射 </summary>
        private Dictionary<string, ControllerInfo> _activeControllerDic = new (64);
        /// <summary> 打开着的面板，用于esc返回 </summary>
        private LinkedList<ControllerInfo> _activeControllerList = new();
        /// <summary> 路径映射缓存（UIPath扫描结果 </summary>
        private Dictionary<string, (Type, Type)> _pathMapDic = new(64);
        
        private ControllerInfo _bottomController;
        private ControllerInfo _loadingController;

        public void Init(UIRoot uiRoot)
        {
            CurUIRoot = uiRoot;
            UICamera = uiRoot.UICamera;
            
            // 扫描所有UIPath特性
            ScanUIPathAttributes();
            Debug.Log("[UIManager] UIManager Init");
        }

        /// <summary>
        /// 扫描所有UIPath特性的Contorller
        /// </summary>
        private void ScanUIPathAttributes()
        {
            int count = 0;
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var controllerType in types)
                    {
                        if (controllerType.IsAbstract || !typeof(IController).IsAssignableFrom(controllerType))
                        {
                            continue;
                        }
                        
                        // 获取所有UIPath特性
                        var attributes = controllerType.GetCustomAttributes(typeof(UIPathAttribute), false);
                        if (attributes.Length == 0) continue;
                        
                        // 推断响应的View类型
                        var viewType = InferViewType(controllerType);
                        if (viewType == null)
                        {
                            Debug.Log($"{TAG} 无法推断View类型：{controllerType.Name}");
                            continue;
                        }
                        
                        // 注册所有路径
                        foreach (UIPathAttribute attribute in attributes)
                        {
                            _pathMapDic[attribute.Path] = (viewType, controllerType);
                            count++;
                            Debug.Log($"{TAG} 注册路径映射： {attribute.Path}➡{viewType.Name}➡{controllerType.Name}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"{TAG} 扫描程序集失败 {assembly.FullName} : {e.Message}");
                }
            }
            
            Debug.Log($"{TAG} 扫描完成，共注册{count}个路径映射");
        }

        /// <summary>
        /// UIxxController : BaseController<UIxxView> 得到UIxxView
        /// <param name="controllerType">某个具体的UIController</param>
        /// <returns></returns>
        private Type InferViewType(Type controllerType)
        {
            var baseType = controllerType.BaseType;
            if (baseType == null || !baseType.IsGenericType)
                return null;
            
            var genericArgs = baseType.GetGenericArguments();
            return genericArgs.Length > 0 ? genericArgs[0] : null;
        }

        public void Open(string location, UILayer layer = UILayer.Normal, object param = null,
            Action closeCallback = null)
        {
            var layerRect = GetLayerRect(layer);
            
            // 如果已经打开，先关闭
            if (_activeControllerDic.TryGetValue(location, out var controllerInfo))
            {
                // CloseView(location);
            }
            
            var prefab = ResManager.Instance.LoadAsset<GameObject>(location);
            if (prefab == null)
            {
                Debug.Log($"{TAG} UI预制体加载失败{location}");
                return;
            }
            
            var go = Object.Instantiate(prefab, layerRect, false);
            var uiBehaviour = go.GetComponent<UIBehaviour>();
            if (uiBehaviour == null)
            {
                Debug.LogError($"{TAG} 此UI预制体上没有UIBehaviour组件，{location}");
                Object.Destroy(go);
                return;
            }
            
            var rectTransform = go.transform as RectTransform;
            if (rectTransform == null)
            {
                Debug.LogError($"{TAG} 此UI预制体上没有RectTransform组件，{location}");
                Object.Destroy(go);
                return;
            }
            
            rectTransform.localScale = Vector3.one;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            // Type viewType;
            // Type controllerType;
            var types = GetPathMapDic(location);
            var viewType = types.Item1;
            var controllerType = types.Item2;
            if (viewType == null || controllerType == null)
            {
                Debug.LogError($"{TAG} 无法解析view或controller，{location}");
                Object.Destroy(go);
                return;
            }

            UIBaseView view = CreatViewInstance(viewType);
            if (view == null)
            {
                Debug.LogError($"{TAG} 创建view实例失败{viewType.Name}");
                Object.Destroy(go);
                return;
            }

            view.Location = location;
            view.NodeRect = layerRect;
            
            view.Inject(uiBehaviour);
            
            IController controller = CreatControllerInstance(controllerType, view);
            if (controller == null)
            {
                Debug.LogError($"{TAG} 创建Controller失败{controllerType.Name}");
                view.Dispose();
                Object.Destroy(go);
                return;
            }
            
            controller.OnRegister(param);
            var animation = go.GetComponent<UIBaseAnimation>();
            if (animation != null)
            {
                animation.OpenAnimation();
            }

            var controllerInfoCache = new ControllerInfo()
            {
                Location = location,
                View = view,
                Controller = controller,
                GameObject = go,
                Layer = layer,
                CloseCallback = closeCallback
            };
            
            _activeControllerDic[location] = controllerInfoCache;
            if (layer != UILayer.Bottom && layer != UILayer.Loading)
            {
                var last = GetLastActiveController();
                if (last != null)
                {
                    last.Controller.OnActive(false);
                }
                _activeControllerList.AddLast(controllerInfoCache);
                controllerInfoCache.Controller.OnActive(true);

                if (layer == UILayer.Bottom)
                {
                    _bottomController = controllerInfoCache;
                }

                if (layer == UILayer.Loading)
                {
                    _loadingController = controllerInfoCache;
                }
                
                Debug.Log($"{TAG} UI面板打开成功{location}");
            }
        }

        public void CloseView(string location, object param = null)
        {
            if (!_activeControllerDic.TryGetValue(location, out var controllerInfo))
            {
                Debug.LogError($"{TAG} 尝试关闭不存在的UI{location}");
                return;
            }
            
            controllerInfo.Controller.OnRemove();
            controllerInfo.View.Dispose();

            if (controllerInfo.GameObject != null)
            {
                Object.Destroy(controllerInfo.GameObject);
            }

            if (controllerInfo.CloseCallback != null)
            {
                controllerInfo.CloseCallback.Invoke();
            }
            
            _activeControllerDic.Remove(location);
            _activeControllerList.Remove(controllerInfo);

            if (controllerInfo == _bottomController)
            {
                _bottomController = null;
            }

            if (controllerInfo == _loadingController)
            {
                _loadingController = null;
            }

            if (controllerInfo.Layer != UILayer.Bottom && controllerInfo.Layer != UILayer.Loading)
            {
                var last = GetLastActiveController();
                if (last != null)
                {
                    last.Controller.OnActive(true);
                }
            }
            
            Debug.Log($"{TAG} 关闭UI{location}");
        }

        public void CloseCew(IController controller, object param = null)
        {
            ControllerInfo controllerInfo = null;
            foreach (var kvp in _activeControllerDic)
            {
                if (kvp.Value.Controller == controller)
                {
                    controllerInfo =  kvp.Value;
                    break;
                }
            }

            if (controllerInfo == null)
            {
                Debug.LogError($"{TAG} 未找到对应Controller");
                return;
            }
            
            CloseView(controllerInfo.Location, param);
        }

        public void Escape()
        {
            var last = GetLastActiveController();
            last?.Controller.OnEscape();
        }

        /// <summary>
        /// 获取对应的层级的RectTransform节点
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        private RectTransform GetLayerRect(UILayer layer)
        {
            if (CurUIRoot != null)
                return layer switch
                {
                    UILayer.Bottom => CurUIRoot.BottomRect,
                    UILayer.Game => CurUIRoot.GameRect,
                    UILayer.Normal => CurUIRoot.NormalRect,
                    UILayer.Common => CurUIRoot.CommonRect,
                    UILayer.Top => CurUIRoot.TopRect,
                    UILayer.Loading => CurUIRoot.LoadingRect,
                    _ => CurUIRoot.NormalRect
                };
            Debug.LogError($"${TAG} UI根节点为null请检查UIRoot的初始化");
            return null;

        }

        private (Type, Type) GetPathMapDic(string location)
        {
            if(_pathMapDic.TryGetValue(location, out (Type viewType, Type controllerType) types))
            {
                return types;
            }
            
            return (null, null);
        }

        private UIBaseView CreatViewInstance(Type viewType)
        {
            try
            {
                var assembly = viewType.Assembly;
                if (viewType.FullName == null)
                {
                    Debug.Log($"{TAG} 未获取到此view的名称，所属程序集{assembly.GetName().Name}"); 
                    return null;
                }
                
                var instnce = assembly.CreateInstance(viewType.FullName);
                if (instnce == null)
                {
                    Debug.Log($"{TAG} 程序集创建实例返回null\n Type:{viewType.FullName}\n Assembly{assembly.GetName().Name}");
                    return null;
                }
                
                var view = instnce as UIBaseView;
                if (view == null)
                {
                    Debug.LogError($"{TAG} 类型转换失败\n" +
                                   $"instance type:{instnce.GetType().FullName}\n" +
                                   $"instance assembly:{instnce.GetType().Assembly.GetName().Name}\n" +
                                   $"base type:{typeof(UIBaseView).FullName}\n" +
                                   $"base assembly:{typeof(UIBaseView).Assembly.GetName().Name}");
                    return null;
                }
                
                Debug.Log($"{TAG} UI面板创建成功{viewType.Name}");
                return view;
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} 创建view实例异常：{e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private IController CreatControllerInstance(Type controllerType, UIBaseView view)
        {
            try
            {
                // var controller = Activator.CreateInstance(controllerType) as IController;
                var assembly = controllerType.Assembly;
                var instance = assembly.CreateInstance(
                    controllerType.FullName,
                    false,
                    System.Reflection.BindingFlags.CreateInstance,
                    null,
                    new object[] { view },
                    null,
                    null);
                if (instance == null)
                {
                    Debug.LogError($"{TAG} Controller创建失败{controllerType.FullName}");
                    return null;
                }
                
                var controller = instance as IController;
                if (controller == null)
                {
                    Debug.LogError($"{TAG} Controller转换{instance.GetType().FullName}");
                    return null;
                }

                Debug.Log($"{TAG} Controller创建成功{controllerType.FullName}");
                return controller;
            }
            catch (Exception e)
            {
                Debug.LogError($"{TAG} CreatControllerInstance异常{e.Message}\n{e.StackTrace}");
                throw;
            }
        }

        private ControllerInfo GetLastActiveController()
        {
            var node = _activeControllerList.Last;
            while (node != null)
            {
                var info = node.Value;
                if (info != _bottomController && info != _loadingController)
                {
                    return info;
                }

                node = node.Previous;
            }
            
            return null;
        }
        
        private class ControllerInfo
        {
            public string Location;
            public UIBaseView View;
            public IController Controller;
            public GameObject GameObject;
            public UILayer Layer;
            public Action CloseCallback;
        }
    }
}