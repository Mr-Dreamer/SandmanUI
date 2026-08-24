#if UNITY_EDITOR

using System.Collections.Generic;
using Framework.UI.Core;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomEditor(typeof(UIBehaviour))]
    public class UIBehaviourEditor : UnityEditor.Editor
    {
        private UIBehaviour _uiBehaviour;
        private List<string> _keyList = new();
        private List<Object> _valueList = new();

        private int _selectedIndex = 0;
        private string _error = "";
        private string _keyName = "";
        private GameObject _selectGameObject;
        private Object[] _availableComponents;
        private GameObject _tempGameObject;

        public override void OnInspectorGUI()
        {
            _uiBehaviour = target as UIBehaviour;
            
            base.OnInspectorGUI();
            
            ShowComponentBindingSection();
        }

        /// <summary>
        /// 显示组件绑定部分
        /// </summary>
        private void ShowComponentBindingSection()
        {
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayout.LabelField("UI组件绑定", EditorStyles.boldLabel);
            
            // 显示已绑定的组件列表
            for (int i = 0; i < _valueList.Count; i++)
            {
                GUILayout.Space(2);
                EditorGUILayout.BeginHorizontal();
                
                string key = _keyList[i];
                Object value = _valueList[i];
                
                string newKeyName =  EditorGUILayout.TextField(key, GUILayout.Width(120));
                if (newKeyName != key)
                {
                    _keyList[i] = newKeyName;
                    EditorUtility.SetDirty(_uiBehaviour.gameObject);
                }

                if (value == null)
                {
                    EditorGUILayout.HelpBox("组件为空", MessageType.Error);
                }
                else
                {
                    Object newValue = EditorGUILayout.ObjectField("", value, typeof(Object), true, GUILayout.Width(130));
                    if (newValue != value)
                    {
                        _valueList[i] = newValue;
                        EditorUtility.SetDirty(_uiBehaviour.gameObject);
                    }

                    GameObject go = null;
                    if (newValue is GameObject)
                    {
                        go = (GameObject)newValue;
                    }else if (newValue is Component)
                    {
                        go = ((Component)newValue).gameObject;
                    }

                    if (go != null)
                    {
                        string[] componentNames = GetComponentNames(go, out _availableComponents);
                        int currentIndex = GetComponentIndex(componentNames, value.GetType().Name);
                        int newIndex = EditorGUILayout.Popup(currentIndex, componentNames, GUILayout.Width(160));
                        // int newIndex2 = EditorGUILayout.Popup("", newIndex, componentNames, GUILayout.Width(160));
                        if (newIndex != currentIndex && newIndex > 0 && newIndex < _availableComponents.Length)
                        {
                            _valueList[i] = _availableComponents[newIndex];
                            EditorUtility.SetDirty(_uiBehaviour.gameObject);
                        }
                    }
                }

                if (GUILayout.Button("❌", GUILayout.Width(40)))
                {
                    _valueList.RemoveAt(i);
                    _keyList.RemoveAt(i);
                    EditorUtility.SetDirty(_uiBehaviour.gameObject);
                    return;
                }

                if (GUILayout.Button("⬆️", GUILayout.Width(40)))
                {
                    if (i > 0)
                    {
                        SwapListItems(i, i - 1);
                        EditorUtility.SetDirty(_uiBehaviour.gameObject);
                        return;
                    }
                }

                if (GUILayout.Button("⬇️", GUILayout.Width(40)))
                {
                    if (i < _valueList.Count - 1)
                    {
                        SwapListItems(i, i + 1);
                        EditorUtility.SetDirty(_uiBehaviour.gameObject);
                        return;
                    }
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            GUILayout.Space(10f);
            
            // 添加组件部分
            GUILayout.BeginHorizontal();
            _keyName = EditorGUILayout.TextField(_keyName, GUILayout.Width(120));
            
            GameObject oldSelectGameObject = _selectGameObject;
            _selectGameObject = (GameObject)EditorGUILayout.ObjectField(
                "",
                _selectGameObject,
                _selectGameObject == null ? typeof(GameObject) : _selectGameObject.GetType(),
                true,
                GUILayout.Width(130f));
            
            if (null != _selectGameObject)
            {
                if (_tempGameObject != _selectGameObject ||  _selectGameObject != oldSelectGameObject)
                {
                    _selectedIndex = AutoDetectBaseComponent(_selectGameObject);
                }
                
                _tempGameObject = _selectGameObject;
                
                string[] componentNames = GetComponentNames(_selectGameObject, out _availableComponents);
                _selectedIndex = EditorGUILayout.Popup("", _selectedIndex, componentNames, GUILayout.Width(160));

                if (string.IsNullOrEmpty(_keyName))
                {
                    _keyName = _selectGameObject.name;
                }
            }

            if (_selectGameObject != null || string.IsNullOrEmpty(_keyName))
            {
                if (GUILayout.Button("清空", GUILayout.Width(60)))
                {
                    _selectGameObject = null;
                    _keyName = string.Empty;
                }
            }
            
            EditorGUILayout.EndHorizontal();
            
            // 添加组件按钮
            if (GUILayout.Button("添加组件", GUILayout.Width(120)))
            {
                _error = "";

                if (string.IsNullOrEmpty(_keyName))
                {
                    _error = "key不能为null";
                }else if (_keyList.Contains(_keyName))
                {
                    _error = $"key{_keyName}已存在";
                }else if (_selectGameObject == null)
                {
                    _error = "value不能为null";
                }
                else
                {
                    _valueList.Add(_availableComponents[_selectedIndex]);
                    _keyList.Add(_keyName);
                    _keyName = string.Empty;
                    _selectGameObject = null;
                    EditorUtility.SetDirty(_uiBehaviour.gameObject);
                }
            }

            if (!string.IsNullOrEmpty(_error))
            {
                GUILayout.Space(3f);
                EditorGUILayout.HelpBox(_error, MessageType.Error, true);
            }
            
            EditorGUILayout.EndVertical();
        }

        private void ShowQuickActions()
        {
            GUILayout.Space(10f);
            GUILayout.BeginVertical("box");
            
            EditorGUILayout.LabelField("快捷操作提示", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "右键菜单快捷操作:\n" +
                "Ctrl+Alt+C - 收集组件UI\n" +
                "Ctrl+Alt+G - 一键生成代码\n" +
                "Ctrl_Alt+B - 收集并生成代码",
                MessageType.Info);
        }

        /// <summary>
        /// 获取此物体上所有组件
        /// </summary>
        /// <param name="go"></param>
        /// <param name="components"></param>
        /// <returns></returns>
        private string[] GetComponentNames(GameObject go, out Object[] components)
        {
            Component[] allComponents = go.GetComponents<Component>();
            components  = new Object[allComponents.Length];
            components[0] = go;
            
            string[] names = new string[allComponents.Length];
            names[0] = go.name;

            for (int i = 0; i < allComponents.Length; i++)
            {
                components[i + 1] = allComponents[i];
                names[i + 1] = allComponents[i].GetType().Name;
            }
            
            return names;
        }

        private int GetComponentIndex(string[] names, string typeName)
        {
            if (names == null || names.Length == 0)
            {
                return 0;
            }

            for (int i = 0; i < names.Length; i++)
            {
                if (names[i] == typeName)
                {
                    return i;
                }
            }

            return 0;
        }

        private void SwapListItems(int indexA, int indexB)
        {
            if (indexA == indexB) return;
            
            string tempKey = _keyList[indexA];
            Object tempValue = _valueList[indexA];
            
            _keyList[indexA] = _keyList[indexB];
            _valueList[indexA] = _valueList[indexB];
            
            _keyList[indexB] = tempKey;
            _valueList[indexB] = tempValue;
        }

        /// <summary>
        /// 自动识别该对象身上的最常用组件
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        private int AutoDetectBaseComponent(GameObject go)
        {
            Component[] allComponents = go.GetComponents<Component>();

            System.Type[] priortyTypes =
            {
                typeof(UnityEngine.UI.Button),
                typeof(TMPro.TextMeshProUGUI),
                typeof(UnityEngine.UI.InputField),
                typeof(TMPro.TextMeshProUGUI),
                typeof(TMPro.TMP_Text),
                typeof(UnityEngine.UI.Text),
                typeof(UnityEngine.UI.Image),
                typeof(UnityEngine.UI.RawImage),
                typeof(UnityEngine.UI.Toggle),
                typeof(UnityEngine.UI.Slider),
                typeof(UnityEngine.UI.ScrollRect),
                typeof(UnityEngine.UI.Dropdown),
                typeof(TMPro.TMP_Dropdown),
                typeof(RectTransform),
                typeof(Transform),
            };

            for (int priorty = 0; priorty < priortyTypes.Length; priorty++)
            {
                var type = priortyTypes[priorty];
                for (int i = 0; i < allComponents.Length; i++)
                {
                    if (allComponents[i].GetType() == type)
                    {
                        return i + 1;
                    }
                }
            }
            
            return 0;
        }
    }
}

#endif