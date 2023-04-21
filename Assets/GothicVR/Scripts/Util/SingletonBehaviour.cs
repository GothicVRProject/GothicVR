using UnityEngine;

namespace GVR.Util
{ 
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public bool dontDestroyOnLoad = false;

        protected static T _instance;

        public static bool Created
        { get { return _instance != null; } }

        /// <summary>
        /// Always returns the first created instance.
        /// </summary>
        public static T Instance
        {
            get
            {
                return _instance;
            }
        }
        public static T GetOrCreate()
        {
            if (!Created)
                _instance = FindObjectOfType<T>();

            if (!Created)
            {
                _instance = CreateNewInstance();
                Debug.Log("A new instance of " + _instance.GetType() + " has been created");
            }
            return _instance;
        }

        public static T LoadFromResources(string path)
        {
            if (path != null)
            {
                var container = Instantiate(Resources.Load(path) as GameObject);
                var tmp = container.GetComponent<T>();
                Debug.Log("Loaded prefab instance from resources folder: [" + path + "]");
                return tmp;
            }
            else
            {
                Debug.LogError("Resource path is [" + path + "]! Could not create prefab instance of " + typeof(T).Name);
            }
            return null;
        }

        protected void Awake()
        {
            if (Created && _instance != this)
            {
                Debug.LogWarning("An instance of this singleton (" + _instance.name + ") already exists. Destroying " + this.gameObject);
                Destroy(this.gameObject);
                return;
            }
            else
            {
                if (dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);

                _instance = (T)this;
            }
        }
        private static T CreateNewInstance()
        {
            if (!Created)
            {
                var container = new GameObject(typeof(T).Name);
                _instance = container.AddComponent<T>();

                if (Created)
                {
                    Debug.LogError("Created [" + _instance.name + "] with no settings!");
                    return _instance;
                }
            }
            return _instance;
        }
    }
}