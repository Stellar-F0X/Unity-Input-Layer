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
            _focusedInputLayer = Singleton<InputManager>.Instance.CreateInputLayer(_activeLayer.name);
        }


        public virtual void RegisterInputAction(string actionName, InputCallback callbackType, Action<InputAction.CallbackContext> callback)
        {
            InputAction action = _focusedInputLayer.GetAction(actionName);

            if ((callbackType & InputCallback.Started) > 0)
            {
                _cachedCallbacks[actionName + InputCallback.Started] = callback;
                action.started += callback;
            }

            if ((callbackType & InputCallback.Canceled) > 0)
            {
                _cachedCallbacks[actionName + InputCallback.Canceled] = callback;
                action.canceled += callback;
            }

            if ((callbackType & InputCallback.Performed) > 0)
            {
                _cachedCallbacks[actionName + InputCallback.Performed] = callback;
                action.performed += callback;
            }
        }


        public virtual void UnregisterInputAction(string actionName, InputCallback callbackType)
        {
            InputAction action = _focusedInputLayer.GetAction(actionName);
            
            if ((callbackType & InputCallback.Started) > 0)
            {
                string key1 = actionName + InputCallback.Started;
                action.started -= _cachedCallbacks.GetValueOrDefault(key1);
            }

            if ((callbackType & InputCallback.Canceled) > 0)
            {
                string key2 = actionName + InputCallback.Canceled;
                action.canceled -= _cachedCallbacks.GetValueOrDefault(key2);
            }

            if ((callbackType & InputCallback.Performed) > 0)
            {
                string key3 = actionName + InputCallback.Performed;
                action.performed -= _cachedCallbacks.GetValueOrDefault(key3);
            }
        }
        
        
        public bool ReadInput<T>(in string actionName, out T value) where T : struct
        {
            if (InputManager.InputBlock || InputManager.PeekInputLayer == _focusedInputLayer)
            {
                value = default;
                return false;
            }

            value = _focusedInputLayer.GetAction(actionName)?.ReadValue<T>() ?? default;
            return true;
        }

        
        public bool ReadButton(in string actionName)
        {
            if (InputManager.InputBlock || InputManager.PeekInputLayer == _focusedInputLayer)
            {
                return false;
            }


            return _focusedInputLayer.GetAction(actionName)?.IsInProgress() ?? false;
        }
        
        
        public bool ReadButtonUp(string actionName)
        {
            if (InputManager.InputBlock || InputManager.PeekInputLayer == _focusedInputLayer)
            {
                return false;
            }

            return _focusedInputLayer.GetAction(actionName)?.WasReleasedThisFrame() ?? false;
        }


        public bool ReadButtonDown(string actionName)
        {
            if (InputManager.InputBlock || InputManager.PeekInputLayer == _focusedInputLayer)
            {
                return false;
            }


            return _focusedInputLayer.GetAction(actionName)?.WasPressedThisFrame() ?? false;
        }
    }
}