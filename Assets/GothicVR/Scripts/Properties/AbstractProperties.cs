using UnityEngine;

namespace GVR.Properties
{
    public abstract class AbstractProperties : MonoBehaviour
    {
        // using this to store the hero gameobject as monobehaviour has gameObject as readonly
        // I did this because it's easier than to recreate the properties on every world transition
        private GameObject customGameObject; 

        public GameObject go
        {
            get
            {
                if (customGameObject != null)
                    return customGameObject;

                return gameObject;
            }
            set => customGameObject = value;
        }
    }
}
