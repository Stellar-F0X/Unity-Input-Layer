using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace LayeredInputSystem.Runtime
{
    [DisallowMultipleComponent]
    internal sealed class InputManager : MonoBehaviour
    {
        private static InputManager _instance;
        
        public static InputManager Instance
        {
            get { return _instance = _instance != null ? _instance : InputManager.CreateInstance(); }
        }
        
        private readonly Stack<InputLayerInfo> _inputActionLayer = new Stack<InputLayerInfo>();

        internal event Action<InputLayerInfo> onPushedInputLayer;
        internal event Action<InputLayerInfo> onPoppedInputLayer;

        
        [SerializeField]
        private InputLayer _rootLayer;

        private InputActionAsset _inputMapsAsset;
        private InputActionMap _currentInputMap;



        public InputLayerInfo peekInputLayerInfo
        {
            get { return _inputActionLayer.Count > 0 ? _inputActionLayer.Peek() : default; }
        }

        public bool inputBlock
        {
            get { return (_currentInputMap is null) ? false : !_currentInputMap.enabled; }
        }

        public bool layerStackBlock
        {
            get;
            set;
        }

        internal IReadOnlyCollection<InputLayerInfo> inputLayerStack
        {
            get { return _inputActionLayer; }
        }



        private static InputManager CreateInstance()
        {
            _instance = Object.FindAnyObjectByType<InputManager>();

            if (_instance == null)
            {
                GameObject newGameObject = new GameObject(typeof(InputManager).Name);
                _instance = newGameObject.AddComponent<InputManager>();
            }

            return _instance;
        }



        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Object.Destroy(this);
                return;
            }

            _instance = this;
            Object.DontDestroyOnLoad(this.gameObject);
            this._inputMapsAsset = InputSystem.actions;
            
            if (_inputMapsAsset?.actionMaps.Count == 0)
            {
                Debug.Log("No Action Maps defined in actions");
                return;
            }
            
            if (string.IsNullOrEmpty(_rootLayer.name))
            {
                InputActionMap map = _inputMapsAsset.actionMaps[0];
                _rootLayer = new InputLayer(map);
            }
            
            this.PushInputLayer(_rootLayer, isRoot: true);
        }

        

        private void OnDestroy()
        {
            if (_instance != this)
            {
                return;
            }

            _instance = null;
        }



        public void EnableControls(bool enable)
        {
            if (enable)
            {
                _currentInputMap.Enable();
            }
            else
            {
                _currentInputMap.Disable();
            }
        }



        private InputLayerInfo CreateInputLayer(in Guid id, bool isRoot = false)
        {
            InputActionMap actionMap = _inputMapsAsset.FindActionMap(id);
            
            if (actionMap is null)
            {
                throw new NullReferenceException($"{nameof(InputManager)}: 입력 액션 맵이 없습니다.");
            }
            else
            {
                return new InputLayerInfo(actionMap, isRoot);
            }
        }



        private bool PushInputLayer(in InputLayer layer, bool isRoot = false)
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }
            
            this.SwitchActionMap(layer.id);

            InputLayerInfo layerInfo = this.CreateInputLayer(layer.id, isRoot);
            _inputActionLayer.Push(layerInfo);
            onPushedInputLayer?.Invoke(layerInfo);
            return true;
        }



        internal bool PushInputLayer(in string inputActionMapName)
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            InputActionMap map = InputSystem.actions.FindActionMap(inputActionMapName);

            if (map is null)
            {
                throw new NullReferenceException($"{nameof(InputManager)}: {inputActionMapName}이 없습니다.");
            }

            if (_inputActionLayer.Any(layer => layer.actionMapId == map.id))
            {
                Debug.LogWarning($"{nameof(InputManager)}: 똑같은 레이어를 추가할 수 없습니다.");
                return false;
            }

            return this.PushInputLayer(new InputLayer(map));
        }



        internal void PopInputLayer()
        {
            if (this.layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return;
            }

            if (this.peekInputLayerInfo.isRoot)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 루트 레이어는 제거할 수 없습니다.");
                return;
            }

            bool success = this.TryPopInputLayer();
            Assert.IsTrue(success);
        }



        internal void PopAllInputLayersExpectRoot()
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
            }
            else
            {
                while (this.TryPopInputLayer()) { }
            }
        }



        private bool TryPopInputLayer()
        {
            if (this.peekInputLayerInfo.isRoot)
            {
                return false;
            }

            _inputActionLayer.Pop();

            this.SwitchActionMap(peekInputLayerInfo.actionMapId);
            this.onPoppedInputLayer?.Invoke(peekInputLayerInfo);
            return true;
        }



        private void SwitchActionMap(in Guid id)
        {
            _currentInputMap?.Disable();
            _currentInputMap = _inputMapsAsset.FindActionMap(id);

            if (_currentInputMap is null)
            {
                throw new NullReferenceException($"{nameof(InputManager)}: 입력 액션 맵이 없습니다.");
            }

            _currentInputMap.Enable();
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(InputManager))]
    internal class InputManagerEditor : Editor
    {
        private InputManager _target;

        
        private void OnEnable()
        {
            _target = (InputManager)target;
        }
        

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Input Layer Stack", EditorStyles.boldLabel);

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("플레이 모드에서만 레이어 스택을 확인할 수 있습니다.", MessageType.Info);
                return;
            }

            IReadOnlyCollection<InputLayerInfo> stack = _target.inputLayerStack;

            if (stack == null || stack.Count == 0)
            {
                EditorGUILayout.HelpBox("레이어 스택이 비어있습니다.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            int index = stack.Count - 1;
            InputLayerInfo peek = _target.peekInputLayerInfo;

            foreach (InputLayerInfo layer in stack)
            {
                this.DrawLayerItem(layer, index--, layer.Equals(peek));
            }

            EditorGUILayout.EndVertical();
            base.Repaint();
        }

        
        private void DrawLayerItem(InputLayerInfo layer, int index, bool isActive)
        {
            Color originalColor = GUI.backgroundColor;

            if (layer.isRoot)
            {
                GUI.backgroundColor = new Color(1f, 0.9f, 0.5f);
            }
            else if (isActive)
            {
                GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
            }

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);

            string prefix = layer.isRoot ? "[Root]" : isActive ? "[Active]" : $"[{index}]";
            EditorGUILayout.LabelField(prefix, GUILayout.Width(60));
            EditorGUILayout.LabelField(layer.name, EditorStyles.boldLabel);

            EditorGUILayout.EndHorizontal();
            GUI.backgroundColor = originalColor;
        }
    }
#endif
}