using InputLayer.Runtime;
using UnityEngine;

namespace InputLayer.Sample
{
    internal class GameManager : Singleton<GameManager>.MonoSingletonable
    {
        public InputLayerName playerLayer;
        
        public InputLayerName uiLayer;


        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log($"{playerLayer.name}으로 입력 레이어를 변경했습니다."); 
                Singleton<InputManager>.Instance.PushInputLayer(playerLayer.name);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log($"{uiLayer.name}으로 입력 레이어를 변경했습니다."); 
                Singleton<InputManager>.Instance.PushInputLayer(uiLayer.name);
            }

            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                Debug.Log($"입력 레이어를 초기화했습니다."); 
                Singleton<InputManager>.Instance.PopInputLayer();
            }
        
            if (Input.GetKeyDown(KeyCode.Alpha9))
            {
                Debug.Log($"입력 레이어를 초기화했습니다."); 
                Singleton<InputManager>.Instance.PopAllInputLayerWithoutRoot();
            }
        }
    }
}