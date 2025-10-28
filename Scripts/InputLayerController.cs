using System;
using UnityEngine;

namespace InputLayer.Runtime
{
    public class InputLayerController : MonoBehaviour
    {
        public event Action<InputLayer> onPushedInputLayer
        {
            add { Singleton<InputManager>.Instance.onPushedInputLayer += value; }

            remove { Singleton<InputManager>.Instance.onPushedInputLayer -= value; }
        }

        public event Action<InputLayer> onPoppedInputLayer
        {
            add { Singleton<InputManager>.Instance.onPoppedInputLayer += value; }

            remove { Singleton<InputManager>.Instance.onPoppedInputLayer -= value; }
        }

        public InputLayer peekInputLayer
        {
            get { return Singleton<InputManager>.Instance.peekInputLayer; }
        }


        
        public bool PushInputLayer(in string inputActionMapName)
        {
            return Singleton<InputManager>.Instance.PushInputLayer(inputActionMapName);
        }


        public void PopInputLayer()
        {
            Singleton<InputManager>.Instance.PopInputLayer();
        }


        public void PopAllInputLayersExpectRoot()
        {
            Singleton<InputManager>.Instance.PopAllInputLayersExpectRoot();
        }
    }
}