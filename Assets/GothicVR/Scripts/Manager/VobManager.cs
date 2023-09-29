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
    }
}