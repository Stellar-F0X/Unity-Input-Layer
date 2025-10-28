using InputLayer.Runtime;
using UnityEngine;

namespace InputLayer.Sample
{
    internal class GameManager : Singleton<GameManager>.MonoSingletonable
    {
        public InputLayerName playerLayer;
        public InputLayerName uiLayer;
        
        public InputLayerController inputLayerController;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                inputLayerController.PushInputLayer(playerLayer.name);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                inputLayerController.PushInputLayer(uiLayer.name);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                inputLayerController.PopInputLayer();
            }
        
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                inputLayerController.PopAllInputLayersExpectRoot();
            }
        }
    }
}