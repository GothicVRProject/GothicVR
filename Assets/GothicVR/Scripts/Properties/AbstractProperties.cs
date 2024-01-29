using UnityEngine;

namespace GVR.Properties
{
    public abstract class AbstractProperties : MonoBehaviour
    {
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
