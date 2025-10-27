using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace InputLayer.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InputManager : Singleton<InputManager>.MonoSingletonable
    {
        private readonly Stack<InputLayer> _inputActionLayer = new Stack<InputLayer>();

        public event Action<InputLayer> onPushedInputLayer;
        public event Action<InputLayer> onPoppedInputLayer;

        [SerializeField]
        private InputLayerName _rootLayer;

        private InputActionAsset _inputMapsAsset;
        private InputActionMap _currentInputMap;



        public static InputLayer PeekInputLayer
        {
            get { return _instance._inputActionLayer.Peek(); }
        }

        public static bool InputBlock
        {
            get;
            set;
        }

        public static bool LayerStackBlock
        {
            get;
            set;
        }



        protected override void OnMonoAwake()
        {
            this._inputMapsAsset = InputSystem.actions;

            Assert.IsTrue(_inputMapsAsset.actionMaps.Count > 0,
                          "No Action Maps defined in InputSystem.actions. " +
                          "Root input layer initialization failed."
            );

            if (string.IsNullOrEmpty(_rootLayer.layerGuid))
            {
                InputActionMap map = _inputMapsAsset.actionMaps[0];
                _rootLayer = new InputLayerName(map.name, map.id.ToString());
            }

            this.PushActionMap(_rootLayer.layerName);
        }



        public void EnableControls(bool enable)
        {
            InputBlock = !enable;

            if (enable)
            {
                _currentInputMap.Enable();
            }
            else
            {
                _currentInputMap.Disable();
            }
        }



        public InputLayer CreateInputLayer([NotNull] in string actionMapName)
        {
            Assert.IsFalse(string.IsNullOrEmpty(actionMapName), "actionMapName은 Null일 수 없습니다.");

            InputActionMap actionMap = _inputMapsAsset.FindActionMap(actionMapName);
            Assert.IsNotNull(actionMap, $"{nameof(InputManager)}: 액션 맵 {actionMapName}을 찾을 수 없습니다.");

            return new InputLayer(_inputMapsAsset.FindActionMap(actionMapName));
        }



        public bool PushActionMap(string actionMapName, out InputLayer layer)
        {
            if (this.PushActionMap(actionMapName))
            {
                layer = PeekInputLayer;
                return true;
            }
            else
            {
                layer = default;
                return false;
            }
        }



        public bool PushActionMap([NotNull] in string actionMapName)
        {
            if (LayerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            InputLayer layer = this.CreateInputLayer(actionMapName);

            _inputActionLayer.Push(layer);

            if (this.SwitchActionMap(actionMapName))
            {
                onPushedInputLayer?.Invoke(layer);
            }

            return true;
        }



        public void PopActionMap()
        {
            if (LayerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return;
            }

            if (PeekInputLayer.isRoot)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 최상위 입력 레이어는 제거할 수 없습니다.");
                return;
            }

            _inputActionLayer.Pop();

            if (this.SwitchActionMap(PeekInputLayer.mapName))
            {
                onPoppedInputLayer?.Invoke(PeekInputLayer);
            }
        }



        private bool SwitchActionMap([NotNull] in string actionMapName)
        {
            if (_currentInputMap is not null)
            {
                _currentInputMap.Disable();
            }

            _currentInputMap = _inputMapsAsset.FindActionMap(actionMapName);
            Assert.IsNotNull(_currentInputMap, "입력 액션 맵이 없습니다.");
            _currentInputMap.Enable();

            if (string.CompareOrdinal(PeekInputLayer.mapName, actionMapName) != 0)
            {
                Debug.LogError("입력 맵 변경에 실패했습니다.");
                return false;
            }
            else
            {
                Debug.Log("성공적으로 입력 맵을 변경했습니다.");
                return true;
            }
        }
    }



#if UNITY_EDITOR
    [CustomEditor(typeof(InputManager))]
    public class InputManagerDrawer : Editor
    {
        private string[] _options = new string[1];


        private void OnDisable()
        {
            _options[0] = null;
            _options = null;
        }


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_rootLayer"));
            
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            if (Application.isPlaying == false)
            {
                return;
            }

            _options[0] = InputManager.PeekInputLayer.mapName;

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Popup("Current Layer", 0, _options);
            }
        }
    }
#endif
}