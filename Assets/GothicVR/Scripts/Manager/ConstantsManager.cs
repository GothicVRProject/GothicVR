using UnityEngine;
using GVR.Util;


namespace GVR.Manager
{
    public class ConstantsManager : SingletonBehaviour<ConstantsManager>
    {
        public LayerMask ItemLayer;

        protected override void Awake()
        {
            ItemLayer = LayerMask.NameToLayer("Item");
        }
    }
}
