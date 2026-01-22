using LayeredInputSystem.Runtime;
using UnityEngine;

namespace LayeredInputSystem.Sample
{
    internal class GameManager : MonoBehaviour
    {
        public InputLayer playerLayer;
        public InputLayer uiLayer;
        public InputLayerSwitcher inputLayerSwitcher;


        private void Update()
        {
	        this.InputKey();
        }


        private void InputKey()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                inputLayerSwitcher.PushInputLayer(playerLayer);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                inputLayerSwitcher.PushInputLayer(uiLayer);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                inputLayerSwitcher.PopInputLayer();
            }
        
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                inputLayerSwitcher.PopAllInputLayersExpectRoot();
            }
        }
    }
}