using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LayeredInputSystem.Runtime
{
    public class InputReceiver : MonoBehaviour
    {
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> _cachedCallbacks = new(StringComparer.Ordinal);

        [SerializeField]
        private InputLayer _activeLayer;
        private InputLayerInfo _cachedInputLayerInfo;


        public string layerName
        {
            get { return _activeLayer.name; }
        }

        public bool active
        {
            get { return _activeLayer.id == InputManager.Instance.peekInputLayerInfo.actionMapId; }
        }



        private void Awake()
        {
            this.CheckAndCacheInputLayer();
        }



        private bool CheckAndCacheInputLayer()
        {
            if (_cachedInputLayerInfo.actionMapId != Guid.Empty)
            {
                return _cachedInputLayerInfo == InputManager.Instance.peekInputLayerInfo;
            }

            if (_activeLayer.id != InputManager.Instance.peekInputLayerInfo.actionMapId)
            {
                return false;
            }

            if (_activeLayer.name != InputManager.Instance.peekInputLayerInfo.name)
            {
                return false;
            }

            _cachedInputLayerInfo = InputManager.Instance.peekInputLayerInfo;
            return true;
        }



        private void OnDestroy()
        {
            foreach (KeyValuePair<string, Action<InputAction.CallbackContext>> pair in _cachedCallbacks)
            {
                string[] paths = pair.Key.Split('.');

                if (paths.Length < 2)
                {
                    continue;
                }

                InputAction action = InputSystem.actions.FindActionMap(_activeLayer.id).FindAction(paths[0]);

                if (action is null)
                {
                    continue;
                }

                action.started -= pair.Value;
                action.canceled -= pair.Value;
                action.performed -= pair.Value;
            }

            _cachedCallbacks.Clear();
        }



        public virtual void RegisterInputAction(string actionName, InputCallbackFlags callbackFlagsType, Action<InputAction.CallbackContext> callback)
        {
            InputActionMap foundMap = InputSystem.actions.FindActionMap(_activeLayer.id);
            InputAction action = foundMap.FindAction(actionName);

            if (action is null)
            {
                Debug.LogError($"{nameof(InputReceiver)}: {foundMap.name}에 {actionName}이 없습니다.");
                return;
            }

            if ((callbackFlagsType & InputCallbackFlags.Started) > 0)
            {
                _cachedCallbacks[$"{actionName}.{InputCallbackFlags.Started}"] = callback;
                action.started += callback;
            }

            if ((callbackFlagsType & InputCallbackFlags.Canceled) > 0)
            {
                _cachedCallbacks[$"{actionName}.{InputCallbackFlags.Canceled}"] = callback;
                action.canceled += callback;
            }

            if ((callbackFlagsType & InputCallbackFlags.Performed) > 0)
            {
                _cachedCallbacks[$"{actionName}.{InputCallbackFlags.Performed}"] = callback;
                action.performed += callback;
            }
        }


        public virtual void UnregisterInputAction(string actionName, InputCallbackFlags callbackFlagsType)
        {
            InputAction action = InputSystem.actions.FindActionMap(_activeLayer.id).FindAction(actionName);

            if ((callbackFlagsType & InputCallbackFlags.Started) > 0)
            {
                string key = $"{actionName}.{InputCallbackFlags.Started}";
                action.started -= _cachedCallbacks.GetValueOrDefault(key);
            }

            if ((callbackFlagsType & InputCallbackFlags.Canceled) > 0)
            {
                string key = $"{actionName}.{InputCallbackFlags.Canceled}";
                action.canceled -= _cachedCallbacks.GetValueOrDefault(key);
            }

            if ((callbackFlagsType & InputCallbackFlags.Performed) > 0)
            {
                string key = $"{actionName}.{InputCallbackFlags.Performed}";
                action.performed -= _cachedCallbacks.GetValueOrDefault(key);
            }
        }


        public bool ReadInput<T>(in string actionName, out T value) where T : struct
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                value = default;
                return false;
            }

            value = _cachedInputLayerInfo.GetAction(actionName)?.ReadValue<T>() ?? default;
            return true;
        }


        public bool ReadButton(in string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayerInfo.GetAction(actionName)?.IsInProgress() ?? false;
        }


        public bool ReadButtonUp(string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayerInfo.GetAction(actionName)?.WasReleasedThisFrame() ?? false;
        }


        public bool ReadButtonDown(string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayerInfo.GetAction(actionName)?.WasPressedThisFrame() ?? false;
        }


        public IEnumerator AsyncReadButton(string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayerInfo.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return YieldCache.WaitForCompletion(action.IsPressed);
            }
        }


        public IEnumerator AsyncReadButtonDown(string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayerInfo.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return YieldCache.WaitForCompletion(action.WasPressedThisFrame);
            }
        }
        
        
        public IEnumerator AsyncReadButtonUp(string actionName)
        {
            if (InputManager.Instance.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayerInfo.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return YieldCache.WaitForCompletion(action.WasReleasedThisFrame);
            }
        }
    }
}