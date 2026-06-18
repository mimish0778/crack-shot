using UnityEngine;

namespace CrackShot
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual bool Persistent => false;

        protected virtual bool ReplaceOnDuplicate => false;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this && !ReplaceOnDuplicate)
            {
                Destroy(gameObject);
                return;
            }
            Instance = (T)this;
            if (Persistent)
            {
                DontDestroyOnLoad(transform.root.gameObject);
            }
            OnAwake();
        }

        protected virtual void OnAwake() { }
    }

    public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
    {
        protected override bool Persistent => true;
    }
}
