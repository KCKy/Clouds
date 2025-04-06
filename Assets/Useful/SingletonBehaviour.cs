using UnityEngine;

namespace Useful
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T Inst { get; private set; }
        public abstract bool IsPersistent { get; }

        protected virtual void Awake()
        {
            TryInitialize();
        }

        protected virtual void OnEnable()
        {
            TryInitialize();
        }

        protected virtual void OnDisable()
        {
            TryUnregister();
        }

        void TryInitialize()
        {
            if (Inst == this)
                return;

            if (Inst != null)
            {
                DestroyImmediate(this);
                return;
            }

            Inst = (T)this;
            if (IsPersistent)
                DontDestroyOnLoad(gameObject);
        }

        void TryUnregister()
        {
            if (Inst != this)
                return;
            Inst = null;
        }
    }
}