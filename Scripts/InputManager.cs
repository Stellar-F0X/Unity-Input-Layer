using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;


namespace InputLayer.Runtime
{
    [DisallowMultipleComponent]
    public sealed class InputManager : Singleton<InputManager>.MonoSingletonable
    {
        private readonly Stack<InputLayer> _inputActionLayer = new Stack<InputLayer>();

        public event Action<InputLayer> onPushedInputLayer;
        public event Action<InputLayer> onPoppedInputLayer;

        
        [SerializeField]
        private bool _debug;

        [SerializeField]
        private InputLayerName _rootLayer;

        private InputActionAsset _inputMapsAsset;
        private InputActionMap _currentInputMap;



        public InputLayer peekInputLayer
        {
            get { return _instance._inputActionLayer.Peek(); }
        }

        public bool layerStackBlock
        {
            get;
            set;
        }

        public bool inputBlock
        {
            get { return _currentInputMap.enabled; }
        }


        protected override void OnMonoAwake()
        {
            this._inputMapsAsset = InputSystem.actions;

            bool errorFlag = _inputMapsAsset?.actionMaps.Count > 0;
            Assert.IsTrue(errorFlag, "No Action Maps defined in actions");

            if (string.IsNullOrEmpty(_rootLayer.layerGuid))
            {
                InputActionMap map = _inputMapsAsset.actionMaps[0];
                _rootLayer = new InputLayerName(map.name, map.id.ToString());
            }

            this.PushInputLayer(_rootLayer, isRoot: true);
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



        internal InputLayer CreateInputLayer(in Guid id, bool isRoot = false)
        {
            Assert.IsTrue(Guid.Empty != id, $"{nameof(InputManager)}: {nameof(id)}은 비어있을 수 없습니다.");
            InputActionMap actionMap = _inputMapsAsset.FindActionMap(id);
            Assert.IsNotNull(actionMap, $"{nameof(InputManager)}: 액션 맵 {id}을 찾을 수 없습니다.");

            return new InputLayer(actionMap, isRoot);
        }



        internal bool PushInputLayer(in InputLayerName layerName, bool isRoot = false)
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            Guid target = Guid.Parse(layerName.layerGuid);
            InputLayer layer = this.CreateInputLayer(target, isRoot);

            _inputActionLayer.Push(layer);

            if (this.SwitchActionMap(target))
            {
                onPushedInputLayer?.Invoke(layer);
            }

            return true;
        }



        public bool PushInputLayer(in string inputActionMapName)
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return false;
            }

            InputActionMap map = InputSystem.actions.FindActionMap(inputActionMapName);
            Assert.IsNotNull(map, $"{nameof(InputManager)}: {inputActionMapName}이 없습니다.");

            if (_inputActionLayer.Any(layer => layer.actionMapId == map.id))
            {
                Debug.LogWarning($"{nameof(InputManager)}: 똑같은 레이어를 추가할 수 없습니다.");
                return false;
            }

            return this.PushInputLayer(new InputLayerName(map.name, map.id.ToString()));
        }



        public void PopInputLayer()
        {
            if (layerStackBlock)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 레이어 변경이 막혀있습니다.");
                return;
            }

            if (peekInputLayer.isRoot)
            {
                Debug.LogWarning($"{nameof(InputManager)}: 최상위 입력 레이어는 제거할 수 없습니다.");
                return;
            }

            bool success = this.TryPopInputLayer();

            if (_debug)
            {
                Debug.Log($"{nameof(InputManager)}: {(success ? "변경에 성공했습니다." : "변경에 실패했습니다.")}");
            }
        }



        public void PopAllInputLayersExpectRoot()
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
            if (peekInputLayer.isRoot)
            {
                return false;
            }

            if (_inputActionLayer.TryPop(out _) == false)
            {
                return false;
            }

            if (this.SwitchActionMap(peekInputLayer.actionMapId))
            {
                onPoppedInputLayer?.Invoke(peekInputLayer);
                return true;
            }
            else
            {
                return false;
            }
        }



        private bool SwitchActionMap(in Guid id)
        {
            _currentInputMap?.Disable();
            
            _currentInputMap = _inputMapsAsset.FindActionMap(id);
            Assert.IsNotNull(_currentInputMap, $"{nameof(InputManager)}: 입력 액션 맵이 없습니다.");

            _currentInputMap.Enable();

            if (peekInputLayer.actionMapId == id)
            {
                return true;
            }
            else
            {
                Debug.LogError($"{nameof(InputManager)}: 입력 맵 변경에 실패했습니다.");
                return false;
            }
        }



        private void OnGUI()
        {
            if (_debug == false)
            {
                return;
            }

            const float textHeight = 24f;

            GUI.Box(new Rect(2f, 2f, 100f, _inputActionLayer.Count * textHeight + 24), string.Empty);
            GUI.Label(new Rect(4f, 2f, 100f, 30f), $"Input Layers");
            Rect textRect = new Rect(4f, 24f, 150f, textHeight);

            foreach (InputLayer inputLayer in _inputActionLayer)
            {
                if (inputLayer.isRoot)
                {
                    GUI.color = Color.yellow;
                }
                else if (inputLayer == peekInputLayer)
                {
                    GUI.color = Color.green;
                }

                GUI.Label(textRect, inputLayer.mapName);
                textRect.y += 24f;
                GUI.color = Color.white;
            }
        }
    }
}