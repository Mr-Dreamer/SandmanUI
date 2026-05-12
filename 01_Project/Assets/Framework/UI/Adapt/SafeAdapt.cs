using System.Collections.Generic;
using UnityEngine;

namespace Framework.UI.Adapt
{
    public class SafeAdapt : MonoBehaviour
    {
        public float SafeAreaTop { get; private set; }
        public float SafeAreaBottom { get; private set; }
        private readonly List<SpecialSafeArea> _deviceAafeAreaList = new(8)
        {
            new SpecialSafeArea("HUAWEI CLT-AL01", 89),
            new SpecialSafeArea("HUAWEI ANE", 89),
            new SpecialSafeArea("OPPO CPH18", 89),
            new SpecialSafeArea("OPPO CPH19", 89),
            new SpecialSafeArea("motoroal moto g", 89),
            new SpecialSafeArea("TECNO BG6", 89),
            new SpecialSafeArea("samsung SM-A025m", 89),
            new SpecialSafeArea("vivo vivo 1906", 89)
        };
        
        private RectTransform _rectTransform;

        internal void SetSafeArea()
        {
            _rectTransform = (RectTransform)transform;
            Vector2 curScreen = Vector3.zero;
            if (SafeAreaTop <= 0 && SafeAreaBottom <= 0)
            {
                SafeAreaTop = curScreen.y - Screen.safeArea.yMax;
#if UNITY_IOS
                SafeAreaTop *= 0.9f;
                SafeAreaBottom = Screen.safeArea.yMin * 0.5f;
#elif UNITY_ANDROID
                SafeAreaBottom = Screen.safeArea.yMin;
#else
                SafeAreaBottom = Screen.safeArea.yMin;
#endif
            }
            
            // int width = curScreen.width;
            // int height = curScreen.height;
            // UpdateSafeArea();
        }
    }

    public readonly struct SpecialSafeArea
    {
        public string Model { get; }
        public float SafeAreaTop { get; }
        public float SafaeAreaBottom { get; }

        public SpecialSafeArea(string model, float top, float bottom = 0)
        {
            Model = model;
            SafeAreaTop = top;
            SafaeAreaBottom = bottom;
        }
    }
}