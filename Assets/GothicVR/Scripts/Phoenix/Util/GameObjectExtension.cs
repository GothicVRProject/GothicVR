using UnityEngine;

namespace GVR.Phoenix.Util
{
    public static class GameObjectExtension
    {
        public static void SetParent(this GameObject obj, GameObject parent = null)
        {
            if (parent == null)
                obj.transform.parent = null;
            else
                obj.transform.parent = parent.transform;
        }
    }
}