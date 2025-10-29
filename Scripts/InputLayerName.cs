using System;
using System.Linq;
using NUnit.Framework.Internal.Commands;
using UnityEngine;
using UnityEngine.Assertions;
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
        public InputLayerName(InputActionMap map)
        {
            _reference = map;
        }


        [SerializeField, HideInInspector]
        private InputActionMap _reference;


        public string name
        {
            get { return string.IsNullOrEmpty(_reference?.name) ? string.Empty : _reference.name; }
        }

        public Guid id
        {
            get { return _reference.id; }
        }
    }


#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(InputLayerName))]
    internal class InputLayerNameDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using EditorGUI.PropertyScope scope = new EditorGUI.PropertyScope(position, label, property);

            if (InputSystem.actions == null)
            {
                EditorGUI.HelpBox(position, $"{nameof(InputActionMap)}이 존재하지 않습니다.", MessageType.Error);
                return;
            }


            ReadOnlyArray<InputActionMap> maps = InputSystem.actions.actionMaps;

            if (maps.Count == 0)
            {
                EditorGUI.HelpBox(position, $"{nameof(InputActionMap)}이 존재하지 않습니다.", MessageType.Error);
                return;
            }


            InputLayerName layerName = (InputLayerName)property.boxedValue;

            if (string.IsNullOrEmpty(layerName.name))
            {
                property.boxedValue = new InputLayerName(maps[0]);
                property.serializedObject.ApplyModifiedProperties();
                return;
            }

            string[] nameList = maps.Select(map => map.name).ToArray();
            int foundIndex = Array.IndexOf(nameList, layerName.name);
            foundIndex = Mathf.Max(foundIndex, 0);

            EditorGUI.BeginChangeCheck();
            foundIndex = EditorGUI.Popup(position, label.text, foundIndex, nameList);

            if (EditorGUI.EndChangeCheck())
            {
                property.boxedValue = new InputLayerName(maps[foundIndex]);
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
#endif
}