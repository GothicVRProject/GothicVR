using System.Linq;
using UnityEngine;

namespace GVR.Extensions
{
    public static class GameObjectExtension
    {
        public static void SetParent(this GameObject obj, GameObject parent, bool resetLocation = false, bool resetRotation = false)
        {
            if (parent != null)
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

            // The child object was found and isn't ourself
            if (result != null && result != go.transform)
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

        /// <summary>
        /// Returns direct Children of a GameObject. Non-recursive! 
        /// </summary>
        public static GameObject[] GetAllDirectChildren(this GameObject go)
        {
            return Enumerable
                .Range(0, go.transform.childCount)
                .Select(i => go.transform.GetChild(i).gameObject)
                .ToArray();
        }
        
    }
}
