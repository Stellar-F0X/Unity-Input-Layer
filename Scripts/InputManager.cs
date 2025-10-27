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

            this.PushInputLayer(_rootLayer);
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



        internal InputLayer CreateInputLayer(in Guid id)
        {
            Assert.IsTrue(Guid.Empty != id, $"{nameof(id)}은 비어있을 수 없습니다.");

            InputActionMap actionMap = _inputMapsAsset.FindActionMap(id);
            Assert.IsNotNull(actionMap, $"{nameof(InputManager)}: 액션 맵 {id}을 찾을 수 없습니다.");

            return new InputLayer(actionMap);
        }



        internal bool PushInputLayer(in InputLayerName layerName)
        {
            if (LayerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            Guid target = Guid.Parse(layerName.layerGuid);
            InputLayer layer = this.CreateInputLayer(target);

            _inputActionLayer.Push(layer);

            if (this.SwitchActionMap(target))
            {
                onPushedInputLayer?.Invoke(layer);
            }

            return true;
        }



        public bool PushInputLayer(in string inputActionMapName)
        {
            if (LayerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            InputActionMap map = InputSystem.actions.FindActionMap(inputActionMapName);
            Assert.IsNotNull(map, $"{nameof(InputManager)}: 액션 맵 {inputActionMapName}을 찾을 수 없습니다.");

            if (PeekInputLayer.actionMapId == map.id)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 똑같은 레이어를 연속해서 추가할 수 없습니다.");
                return false;
            }

            return this.PushInputLayer(new InputLayerName(map.name, map.id.ToString()));
        }



        public void PopInputLayer()
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

            if (this.SwitchActionMap(PeekInputLayer.actionMapId))
            {
                onPoppedInputLayer?.Invoke(PeekInputLayer);
            }
        }



        private bool SwitchActionMap(in Guid id)
        {
            _currentInputMap?.Disable();

            _currentInputMap = _inputMapsAsset.FindActionMap(id);
            Assert.IsNotNull(_currentInputMap, "입력 액션 맵이 없습니다.");
            _currentInputMap.Enable();

            if (PeekInputLayer.actionMapId == id)
            {
                return true;
            }

            Debug.LogError("입력 맵 변경에 실패했습니다.");
            return false;
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

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
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

                EditorGUILayout.Popup("Current Layer", 0, _options);
            }
        }
    }
#endif
}