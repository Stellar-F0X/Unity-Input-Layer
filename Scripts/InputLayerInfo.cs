using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace LayeredInputSystem.Runtime
{
    internal struct InputLayerInfo : IEquatable<InputLayerInfo>
    {
        public InputLayerInfo(InputActionMap inputActionMap, bool isRoot = false)
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


        public string name
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
        
        public Guid actionMapId
        {
            get { return _inputActionMap?.id ?? Guid.Empty; }
        }

        public InputActionMap inputActionMap
        {
            get { return _inputActionMap; }
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



        public bool Equals(InputLayerInfo other)
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



        public static bool operator==(InputLayerInfo left, InputLayerInfo right)
        {
            return left.Equals(right);
        }
        
        

        public static bool operator !=(InputLayerInfo left, InputLayerInfo right)
        {
            return left.Equals(right) == false;
        }
        

        
        public override bool Equals(object obj)
        {
            return obj is InputLayerInfo other && this.Equals(other);
        }

        
        
        public override int GetHashCode()
        {
            return HashCode.Combine(_hash, _isRoot, _inputActionMap);
        }
        
        

        public override string ToString()
        {
            return $"InputLayer(Map: {name}, Hash: {_hash}, IsRoot: {_isRoot})";
        }
    }
}