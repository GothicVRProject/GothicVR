using UnityEngine;

namespace GVR.Phoenix.Util
{
    public static class GameObjectExtension
    {
        public static void SetParent(this GameObject obj, GameObject parent, bool resetLocation = false, bool resetRotation = false)
        {
            if (parent == null)
                obj.transform.parent = null;
            else
                obj.transform.parent = parent.transform;

            // FIXME - I don't know why, but Unity adds location, rotation, and scale to newly attached sub elements.
            // This is how we clean it up right now.
            if (resetLocation)
                obj.transform.localPosition = Vector3.zero;
            if (resetRotation)
                obj.transform.localRotation = Quaternion.identity;
        }
    }
}