#if GVR_HVR_INSTALLED
using GVR.Context.Controls;
using HurricaneVR.Framework.Components;
using UnityEngine;

namespace GVR.HVR
{
    public class HVRClimbingAdapter : IClimbingAdapter
    {
        public void AddClimbingComponent(GameObject go)
        {
            go.AddComponent<HVRClimbable>();
        }
    }
}
#endif
