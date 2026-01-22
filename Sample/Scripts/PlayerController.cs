using LayeredInputSystem.Runtime;
using UnityEngine;

namespace LayeredInputSystem.Sample
{
    internal class PlayerController : MonoBehaviour
    {
        public float moveSpeedPerSeconds;

        private Vector3 _inputDirection;
        private InputReceiver _inputReceiver;


        private void Start()
        {
            _inputReceiver = GetComponent<InputReceiver>();
            _inputReceiver.RegisterInputAction("Move", InputCallbackFlags.All, context => _inputDirection = context.ReadValue<Vector2>());
        }


        private void Update()
        {
            transform.position += _inputDirection * (moveSpeedPerSeconds * Time.deltaTime);
        }
    }
}
