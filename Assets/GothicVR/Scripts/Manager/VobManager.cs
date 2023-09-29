using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Interface;
using GVR.Properties;
using GVR.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.GothicVR.Scripts.Manager
{
    public class VobManager : SingletonBehaviour<VobManager>
    {
        private const float lookupDistance = 10f; // meter
        
        [CanBeNull]
        public VobProperties GetFreeInteractableWithin10M(Vector3 position, string visualScheme)
        {
            return GameData.I.VobsInteractable
                .Where(i => Vector3.Distance(i.transform.position, position) < lookupDistance)
                .Where(i => i.visualScheme.EqualsIgnoreCase(visualScheme))
                .OrderBy(i => Vector3.Distance(i.transform.position, position))
                .FirstOrDefault();
        }

        [CanBeNull]
        public GameObject GetNearestSlot(GameObject go, Vector3 position)
        {
            var goTransform = go.transform;

            if (goTransform.childCount == 0)
                return null;
            
            var zm = go.transform.GetChild(0);
            
            return zm.gameObject.GetAllDirectChildren()
                .Where(i => i.name.ContainsIgnoreCase("ZS"))
                .OrderBy(i => Vector3.Distance(i.transform.position, position))
                .FirstOrDefault();
        }
    }
}