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
        private List<Object> _objectList = new();

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
        }
    }
}