using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InputLayer.Runtime
{
    [Serializable]
    public struct InputLayerName
    {
        public InputLayerName(string layerName, string layerGuid)
        {
            this.layerName = layerName;
            this.layerGuid = layerGuid;
        }

        [SerializeField]
        internal string layerName;
        
        [SerializeField]
        internal string layerGuid;

        
        public string name
        {
            get { return layerName; }
        }
        
        public Guid id
        {
            get { return Guid.Parse(layerGuid); }
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InputLayerName))]
    internal class InputLayerNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using EditorGUI.PropertyScope scope = new EditorGUI.PropertyScope(position, label, property);
            ReadOnlyArray<InputActionMap> maps = InputSystem.actions.actionMaps;

            if (maps.Count == 0)
            {
                EditorGUI.HelpBox(position, $"{nameof(InputActionMap)}이 존재하지 않습니다.", MessageType.Error);
                return;
            }

            
            InputLayerName layerName = (InputLayerName)property.boxedValue;

            if (string.IsNullOrEmpty(layerName.layerName))
            {
                property.boxedValue = new InputLayerName(maps[0].name, maps[0].id.ToString());
                property.serializedObject.ApplyModifiedProperties();
                return;
            }
            
            string[] nameList = maps.Select(map => map.name).ToArray();
            int foundIndex = Array.IndexOf(nameList, layerName.layerName);
            foundIndex = Mathf.Max(foundIndex, 0);

            EditorGUI.BeginChangeCheck();
            foundIndex = EditorGUI.Popup(position, label.text, foundIndex, nameList);

            if (EditorGUI.EndChangeCheck())
            {
                property.boxedValue = new InputLayerName(maps[foundIndex].name, maps[foundIndex].id.ToString());
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}
