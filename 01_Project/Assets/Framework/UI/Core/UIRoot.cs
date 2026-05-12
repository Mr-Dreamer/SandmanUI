using System;
using System.Collections.Generic;
using Framework.Core;
using Framework.UI.Event;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace Framework.UI.Core
{
    public class UIRoot : MonoBehaviour
    {
        private static UIRoot _instance;

        public static UIRoot Instance => _instance;
        
        private static class InjdctKeys
        {
            public const string UICamera = "UICamera";
            public const string UICanvasScaler = "UICanvasScaler";
            public const string UISafeArea = "UISafeArea";
            public const string BottomRect = "BottomRect";
            public const string GameRect = "GameRect";
            public const string NormalRect = "NormalRect";
            public const string CommonRect = "CommonRect";
            public const string TopRect = "TopRect";
            public const string LoadingRect = "LoadingRect";
        }
        
        public UIBehaviour UIBehaviour;
        [HideInInspector]
        public Camera UICamera;
        [HideInInspector]
        public CanvasScaler UICanvasScaler;
        [HideInInspector]
        public RectTransform UISafeArea;
        [HideInInspector]
        public RectTransform BottomRect;
        [HideInInspector]
        public RectTransform GameRect;
        [HideInInspector]
        public RectTransform NormalRect;
        [HideInInspector]
        public RectTransform CommonRect;
        [HideInInspector]
        public RectTransform TopRect;
        [HideInInspector]
        public RectTransform LoadingRect;

        private List<CanvasScaler> _canvasScalerList = new();
        public Action<bool> OnReady;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                
                Inject(UIBehaviour);
                // TODO UIManager ScreenManager
                
                _canvasScalerList.Add(UICanvasScaler);

                if (UISafeArea != null)
                {
                    // TODO 根据当前屏幕模式切换适配
                }
                
                OnReadyFinish(true);
            }
            else
            {
                Destroy(gameObject);
                OnReadyFinish(false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                // TODO UIManager
            }
        }

        private void Inject(UIBehaviour uiBehaviour)
        {
            if (_canvasScalerList == null) return;
            
            UICamera = uiBehaviour.Get<Camera>(InjdctKeys.UICamera);
            UICanvasScaler = uiBehaviour.Get<CanvasScaler>(InjdctKeys.UICanvasScaler);
            UISafeArea = uiBehaviour.Get<RectTransform>(InjdctKeys.UISafeArea);
            BottomRect = uiBehaviour.Get<RectTransform>(InjdctKeys.BottomRect);
            GameRect = uiBehaviour.Get<RectTransform>(InjdctKeys.GameRect);
            NormalRect = uiBehaviour.Get<RectTransform>(InjdctKeys.NormalRect);
            CommonRect = uiBehaviour.Get<RectTransform>(InjdctKeys.CommonRect);
            TopRect = uiBehaviour.Get<RectTransform>(InjdctKeys.TopRect);
            LoadingRect = uiBehaviour.Get<RectTransform>(InjdctKeys.LoadingRect);
        }

        public T Get<T>(string name) where T : Component
        {
            return UIBehaviour != null ? UIBehaviour.Get<T>(name) : null;
        }

        public void AddCanvasScaler(CanvasScaler canvasScaler)
        {
            if(canvasScaler == null) return;
            if(_canvasScalerList.Contains(canvasScaler)) return;
            _canvasScalerList.Add(canvasScaler);
            canvasScaler.matchWidthOrHeight = 1; // TODO
        }

        public void SetScreenMode()
        {
            foreach (var canvasScaler in _canvasScalerList)
            {
                if (canvasScaler == null) continue;
                canvasScaler.matchWidthOrHeight = 1; // TODO
            }
        }

        private void OnReadyFinish(bool isReady)
        {
            EventManager.Instance.Send(new UIRootReady{IsReady = isReady});
        }

        private void OnDestroy()
        {
            _canvasScalerList.Clear();
            if(_instance == this) _instance = null;
        }
    }
}