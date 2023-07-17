using UnityEngine;

namespace Winkeldief.Utilities
{
    public abstract class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        public static T instance { get; private set; }
        
        protected virtual void Awake()
        {
            if (instance == null)
                instance = this as T;
            else
                Debug.LogWarning("Another instance of " + typeof(T).ToString() + " already exists.");
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}