using UnityEngine;

namespace GVR.Util
{ 
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        protected static T _instance;

        public static bool Created { get { return _instance != null; } }

        /// <summary>
        /// Always returns the first created instance.
        /// </summary>
        public static T I
        {
            get
            {
                return _instance;
            }
        }

        protected virtual void Awake()
        {
            if (Created && _instance != this)
            {
                Debug.LogWarning("An instance of this singleton (" + _instance.name + ") already exists. Destroying " + this.gameObject);
                Destroy(this.gameObject);
            }
            else
            {
                _instance = (T)this;
            }
        }
    }
}