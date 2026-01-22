using System;
using UnityEngine;

namespace LayeredInputSystem.Runtime
{
	public class InputLayerSwitcher : MonoBehaviour
	{
		public event Action<InputLayer> onPushedInputLayer;
		public event Action<InputLayer> onPoppedInputLayer;
		

		public InputLayer peekInputLayerInfo
		{
			get { return new InputLayer(InputManager.Instance.peekInputLayerInfo.inputActionMap); }
		}


		private void Awake()
		{
			InputManager.Instance.onPushedInputLayer += this.OnPushedInputLayer;
			InputManager.Instance.onPoppedInputLayer += this.OnPoppedInputLayer;
		}
		


		private void OnDestroy()
		{
			if (InputManager.Instance == null)
			{
				return;
			}
			
			InputManager.Instance.onPushedInputLayer -= this.OnPushedInputLayer;
			InputManager.Instance.onPoppedInputLayer -= this.OnPoppedInputLayer;
		}



		public void PopAndPushInputLayer(string inputActionMapName)
		{
			this.PopInputLayer();
			this.PushInputLayer(inputActionMapName);
		}


		public void PopAndPushInputLayer(InputLayer layer)
		{
			this.PopInputLayer();
			this.PushInputLayer(layer);
		}


		public void PushInputLayer(string inputActionMapName)
		{
			InputManager.Instance.PushInputLayer(inputActionMapName);
		}


		public void PushInputLayer(InputLayer layer)
		{
			this.PushInputLayer(layer.name);
		}


		public bool TryPushInputLayer(string inputActionMapName)
		{
			return InputManager.Instance.PushInputLayer(inputActionMapName);
		}


		public bool TryPushInputLayer(InputLayer layer)
		{
			return this.TryPushInputLayer(layer.name);
		}


		public void PopInputLayer()
		{
			InputManager.Instance.PopInputLayer();
		}


		public void PopAllInputLayersExpectRoot()
		{
			InputManager.Instance.PopAllInputLayersExpectRoot();
		}


		private void OnPushedInputLayer(InputLayerInfo layerInfo)
		{
			onPushedInputLayer?.Invoke(new InputLayer(layerInfo.inputActionMap));
		}
		
		
		private void OnPoppedInputLayer(InputLayerInfo layerInfo)
		{
			onPoppedInputLayer?.Invoke(new InputLayer(layerInfo.inputActionMap));
		}
	}
}