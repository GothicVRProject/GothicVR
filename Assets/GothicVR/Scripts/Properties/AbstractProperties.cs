using UnityEngine;

namespace GVR.Properties
{
    public abstract class AbstractProperties : MonoBehaviour
    {
        private GameObject _customGameObject = null;

        public GameObject go
        {
            get
            {
                if (_customGameObject != null)
                {
                    return _customGameObject;
                }

                try
                {
                    if (gameObject != null)
                    {
                        return gameObject;
                    }
                }
                catch
                {
                }

                return null;
            }
            set => _customGameObject = value;
        }
    }
}