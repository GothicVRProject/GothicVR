#if GVR_HVR_INSTALLED
using GVR.Context.Controls;
using HurricaneVR.Framework.Components;
using HurricaneVR.Framework.Core;
using UnityEngine;

namespace GVR.HVR
{
    public class HVRClimbingAdapter : IClimbingAdapter
    {
        public void AddClimbingComponent(GameObject go)
        {
            go.AddComponent<HVRClimbable>();
            HVRGrabbable grabbable = go.AddComponent<HVRGrabbable>();
            grabbable.PoseType = HurricaneVR.Framework.Shared.PoseType.PhysicPoser;
        }
    }
}
#endif
