using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputLayer.Runtime
{
    public class InputReceiver : MonoBehaviour
    {
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> _cachedCallbacks = new(StringComparer.Ordinal);

        [SerializeField]
        private InputLayerName _activeLayer;

        private InputLayer _focusedInputLayer;


        public string layerName
        {
            get { return _activeLayer.name; }
        }

        public int layerHash
        {
            get { return _focusedInputLayer.hash; }
        }


        private void Awake()
        {
            _focusedInputLayer = Singleton<InputManager>.Instance.CreateInputLayer(_activeLayer.id);
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
                
                InputAction action = _focusedInputLayer.GetAction(paths[0]);

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



        public virtual void RegisterInputAction(string actionName, InputCallback callbackType, Action<InputAction.CallbackContext> callback)
        {
            InputAction action = _focusedInputLayer.GetAction(actionName);

            if ((callbackType & InputCallback.Started) > 0)
            {
                string key = $"{actionName}.{InputCallback.Started}";
                _cachedCallbacks[key] = callback;
                action.started += callback;
            }

            if ((callbackType & InputCallback.Canceled) > 0)
            {
                string key = $"{actionName}.{InputCallback.Canceled}";
                _cachedCallbacks[key] = callback;
                action.canceled += callback;
            }

            if ((callbackType & InputCallback.Performed) > 0)
            {
                string key = $"{actionName}.{InputCallback.Performed}";
                _cachedCallbacks[key] = callback;
                action.performed += callback;
            }
        }


        public virtual void UnregisterInputAction(string actionName, InputCallback callbackType)
        {
            InputAction action = _focusedInputLayer.GetAction(actionName);

            if ((callbackType & InputCallback.Started) > 0)
            {
                string key = $"{actionName}.{InputCallback.Started}";
                action.started -= _cachedCallbacks.GetValueOrDefault(key);
            }

            if ((callbackType & InputCallback.Canceled) > 0)
            {
                string key = $"{actionName}.{InputCallback.Canceled}";
                action.canceled -= _cachedCallbacks.GetValueOrDefault(key);
            }

            if ((callbackType & InputCallback.Performed) > 0)
            {
                string key = $"{actionName}.{InputCallback.Performed}";
                action.performed -= _cachedCallbacks.GetValueOrDefault(key);
            }
        }


        public bool ReadInput<T>(in string actionName, out T value) where T : struct
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || manager.peekInputLayer != _focusedInputLayer)
            {
                value = default;
                return false;
            }

            value = _focusedInputLayer.GetAction(actionName)?.ReadValue<T>() ?? default;
            return true;
        }


        public bool ReadButton(in string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || manager.peekInputLayer != _focusedInputLayer)
            {
                return false;
            }

            return _focusedInputLayer.GetAction(actionName)?.IsInProgress() ?? false;
        }


        public bool ReadButtonUp(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || manager.peekInputLayer != _focusedInputLayer)
            {
                return false;
            }

            return _focusedInputLayer.GetAction(actionName)?.WasReleasedThisFrame() ?? false;
        }


        public bool ReadButtonDown(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || manager.peekInputLayer != _focusedInputLayer)
            {
                return false;
            }

            return _focusedInputLayer.GetAction(actionName)?.WasPressedThisFrame() ?? false;
        }
    }
}