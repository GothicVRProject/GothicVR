using System;
using GVR.Context.Controls;
using GVR.Flat;
using GVR.OXR;
using UnityEngine;

#if GVR_HVR_INSTALLED

#endif

namespace GVR.Context
{
    public static class GVRContext
    {
        public static IClimbingAdapter ClimbingAdapter { get; private set; }

        public enum Controls
        {
            VR,
            Flat
        }

        public static void SetContext(Controls controls)
        {
            switch (controls)
            {
                case Controls.VR:
                    SetVRContext();
                    break;
                case Controls.Flat:
                    SetFlatContext();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(controls), controls, null);
            }

        }

        private static void SetVRContext()
        {
#if GVR_HVR_INSTALLED
            Debug.Log("Selecting Context: VR - HurricaneVR");
#else
            Debug.Log("Selecting Context: VR - OpenXR (legacy)");
            ClimbingAdapter = new OXRClimbingAdapter();
#endif
        }

        private static void SetFlatContext()
        {
            Debug.Log("Selecting Context: Flat");

            ClimbingAdapter = new FlatClimbingAdapter();
        }
    }
}
