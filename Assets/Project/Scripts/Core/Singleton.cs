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
                // 永続シングルトンの重複は、子のマネージャー単体ではなくルートごと破棄する。
                // (PersistentManagers を再生成するシーンに戻った際、Canvas/EventSystem が残って二重化するのを防ぐ)
                Destroy(Persistent ? transform.root.gameObject : gameObject);
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
