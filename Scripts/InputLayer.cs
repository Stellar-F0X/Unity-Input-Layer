using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputLayer.Runtime
{
    public struct InputLayer : IEquatable<InputLayer>
    {
        public InputLayer(InputActionMap inputActionMap, bool isRoot = false)
        {
            this._isRoot = isRoot;
            this._inputActionMap = inputActionMap;
            this._hash = inputActionMap.name.GetHashCode();
            this._actionCacher = new Dictionary<string, InputAction>();
        }

        private readonly int _hash;
        private readonly bool _isRoot;
        
        private readonly InputActionMap _inputActionMap;
        private readonly Dictionary<string, InputAction> _actionCacher;


        public string mapName
        {
            get { return _inputActionMap.name; }
        }

        public int hash
        {
            get { return _hash; }
        }

        public bool isRoot
        {
            get { return _isRoot; }
        }


        public InputAction GetAction(string actionName)
        {
            if (_actionCacher.TryGetValue(actionName, out InputAction action))
            {
                return action;
            }

            action = _inputActionMap.FindAction(actionName);

            if (action != null)
            {
                _actionCacher.Add(actionName, action);
                return action;
            }

            Debug.LogError($"[InputManager] Could not find input action with name \"{actionName}\"!");
            return null;
        }



        public bool Equals(InputLayer other)
        {
            if (this._hash != other._hash)
            {
                return false;
            }

            if (this._isRoot != other._isRoot)
            {
                return false;
            }

            if (_inputActionMap != other._inputActionMap)
            {
                return false;
            }
            else
            {
                return true;
            }
        }



        public static bool operator==(InputLayer left, InputLayer right)
        {
            return left.Equals(right);
        }
        
        

        public static bool operator !=(InputLayer left, InputLayer right)
        {
            return left.Equals(right) == false;
        }
        

        
        public override bool Equals(object obj)
        {
            if (obj is not InputLayer other)
            {
                return false;
            }

            if (this.Equals(other))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        
        
        public override int GetHashCode()
        {
            return _hash;
        }

        

        public override string ToString()
        {
            return $"InputLayer(Map: {mapName}, Hash: {_hash}, IsRoot: {_isRoot})";
        }
    }
}