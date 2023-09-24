using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Interface;
using GVR.Util;
using JetBrains.Annotations;
using UnityEngine;

namespace GVR.GothicVR.Scripts.Manager
{
    public class VobManager : SingletonBehaviour<VobManager>
    {
        private const float lookupDistance = 10f; // meter
        
        [CanBeNull]
        public GameObject GetFreeInteractableWithin10m(Vector3 position, string name)
        {
            return GameData.I.VobsInteractable
                .Where(i => Vector3.Distance(i.transform.position, position) < lookupDistance)
                .Where(i => gameObject.name.EqualsIgnoreCase(name))
                .OrderBy(i => Vector3.Distance(i.transform.position, position))
                .FirstOrDefault();
        }
    }
}