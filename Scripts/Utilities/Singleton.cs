using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InputLayer.Runtime
{
    public partial class Singleton<TSingleton> where TSingleton : Singleton<TSingleton>.MonoSingletonable, new()
    {
        public static event Action<TSingleton> OnInitialized;

        public static event Action<TSingleton> OnDestroyed;


        private static TSingleton _singleInstance;


        public static TSingleton Instance
        {
            get { return _singleInstance = _singleInstance != null ? _singleInstance : MakeOrFindObject(); }
        }


        public static bool IsInitialized
        {
            get;
            private set;
        }



        private static TSingleton MakeOrFindObject()
        {
            _singleInstance = Object.FindAnyObjectByType<TSingleton>();

            if (_singleInstance == null)
            {
                GameObject newGameObject = new GameObject(typeof(TSingleton).Name);
                _singleInstance = newGameObject.AddComponent<TSingleton>();
            }

            return _singleInstance;
        }
    }


    public partial class Singleton<TSingleton>
    {
        [DefaultExecutionOrder(-10)]
        public class MonoSingletonable : MonoBehaviour
        {
            protected static TSingleton _instance
            {
                get { return Instance; }
            }


            protected void Awake()
            {
                if (_singleInstance != null)
                {
                    Object.Destroy(this.gameObject);
                    return;
                }

                _singleInstance = this as TSingleton;
                IsInitialized = true;
                OnInitialized?.Invoke(_singleInstance);
                Object.DontDestroyOnLoad(this.gameObject);
                this.OnMonoAwake();
            }
            

            protected void OnDestroy()
            {
                this.OnMonoDestroy();
                OnDestroyed?.Invoke(_singleInstance);

                _singleInstance = null;
            }
            

            protected void OnApplicationQuit()
            {
                this.OnMonoDestroy();
                OnDestroyed?.Invoke(_singleInstance);
            }


            protected virtual void OnMonoAwake() { }


            protected virtual void OnMonoDestroy() { }


            protected virtual void OnMonoQuit() { }
        }
    }
}