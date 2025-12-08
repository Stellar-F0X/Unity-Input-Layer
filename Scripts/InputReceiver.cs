using System;
using System.Collections;
using System.Collections.Generic;
using Polygonia;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputLayer.Runtime
{
    public class InputReceiver : MonoBehaviour
    {
        private readonly Dictionary<string, Action<InputAction.CallbackContext>> _cachedCallbacks = new(StringComparer.Ordinal);

        [SerializeField]
        private InputLayerName _activeLayer;

        private InputLayer _cachedInputLayer;


        public string layerName
        {
            get { return _activeLayer.name; }
        }

        public bool active
        {
            get { return _activeLayer.id == Singleton<InputManager>.Instance.peekInputLayer.actionMapId; }
        }



        private void Awake()
        {
            this.CheckAndCacheInputLayer();
        }



        private bool CheckAndCacheInputLayer()
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (_cachedInputLayer.actionMapId != Guid.Empty)
            {
                return _cachedInputLayer == manager.peekInputLayer;
            }

            if (_activeLayer.id != manager.peekInputLayer.actionMapId)
            {
                return false;
            }

            if (_activeLayer.name != manager.peekInputLayer.mapName)
            {
                return false;
            }

            _cachedInputLayer = manager.peekInputLayer;
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



        public virtual void RegisterInputAction(string actionName, InputCallback callbackType, Action<InputAction.CallbackContext> callback)
        {
            InputAction action = InputSystem.actions.FindActionMap(_activeLayer.id).FindAction(actionName);

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
            InputAction action = InputSystem.actions.FindActionMap(_activeLayer.id).FindAction(actionName);

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

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                value = default;
                return false;
            }

            value = _cachedInputLayer.GetAction(actionName)?.ReadValue<T>() ?? default;
            return true;
        }


        public bool ReadButton(in string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayer.GetAction(actionName)?.IsInProgress() ?? false;
        }


        public bool ReadButtonUp(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayer.GetAction(actionName)?.WasReleasedThisFrame() ?? false;
        }


        public bool ReadButtonDown(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                return false;
            }

            return _cachedInputLayer.GetAction(actionName)?.WasPressedThisFrame() ?? false;
        }


        public IEnumerator AsyncReadButton(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayer.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return new WaitForCompletion(action.IsPressed);
            }
        }


        public IEnumerator AsyncReadButtonDown(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayer.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return new WaitForCompletion(action.WasPressedThisFrame);
            }
        }
        
        
        public IEnumerator AsyncReadButtonUp(string actionName)
        {
            InputManager manager = Singleton<InputManager>.Instance;

            if (manager.inputBlock || this.CheckAndCacheInputLayer() == false)
            {
                yield break;
            }

            InputAction action = _cachedInputLayer.GetAction(actionName);

            if (action is null)
            {
                yield break;
            }
            else
            {
                yield return new WaitForCompletion(action.WasReleasedThisFrame);
            }
        }
    }
}