using UnityEngine;

namespace GVR.Extensions
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

        public static GameObject FindChildRecursively(this GameObject go, string name)
        {
            var result = go.transform.Find(name);

            // The child object was found
            if (result != null)
                return result.gameObject;

            // Search recursively in the children of the current object
            foreach (Transform child in go.transform)
            {
                var resultGo = child.gameObject.FindChildRecursively(name);

                // The child object was found in a recursive call
                if (resultGo != null)
                    return resultGo;
            }

            // The child object was not found
            return null;
        }
    }
}