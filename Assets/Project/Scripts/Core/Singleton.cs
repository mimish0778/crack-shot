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

        // シーンの切り替えなどで破棄されたら参照を外す。外さないと Instance が破棄済みの
        // オブジェクトを指したままになり、Instance?.Foo() が null 判定をすり抜けて例外になる
        protected virtual void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    public abstract class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
    {
        protected override bool Persistent => true;
    }
}
