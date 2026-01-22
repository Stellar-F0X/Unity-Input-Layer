using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace LayeredInputSystem.Runtime
{
	[Serializable]
	public struct InputLayer
	{
		public InputLayer(InputActionMap map)
		{
			_reference = map;
		}


		[SerializeReference, HideInInspector]
		private InputActionMap _reference;


		public string name
		{
			get { return _reference is null ? string.Empty : _reference.name; }
		}

		public Guid id
		{
			get { return _reference is null ? Guid.Empty : _reference.id; }
		}
	}


#if UNITY_EDITOR
	[CustomPropertyDrawer(typeof(InputLayer))]
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


			InputLayer layer = (InputLayer)property.boxedValue;

			if (string.IsNullOrEmpty(layer.name))
			{
				property.boxedValue = new InputLayer(maps[0]);
				property.serializedObject.ApplyModifiedProperties();
				return;
			}

			string[] nameList = maps.Select(map => map.name).ToArray();
			int foundIndex = Array.IndexOf(nameList, layer.name);
			foundIndex = Mathf.Max(foundIndex, 0);

			EditorGUI.BeginChangeCheck();
			foundIndex = EditorGUI.Popup(position, label.text, foundIndex, nameList);

			if (EditorGUI.EndChangeCheck())
			{
				property.boxedValue = new InputLayer(maps[foundIndex]);
				property.serializedObject.ApplyModifiedProperties();
			}
		}
	}
#endif
}