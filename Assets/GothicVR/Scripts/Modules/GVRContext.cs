using System;
using GVR.Context.Controls;
using GVR.Flat;
using UnityEngine;
#if GVR_HVR_INSTALLED
using GVR.HVR.Adapter;
#else
using GVR.OXR.Adapter;
#endif


namespace GVR.Context
{
    public static class GVRContext
    {
        public static IInteractionAdapter InteractionAdapter { get; private set; }

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
            InteractionAdapter = new HVRInteractionAdapter();
#else
            Debug.Log("Selecting Context: VR - OpenXR (legacy)");
            InteractionAdapter = new OXRInteractionAdapter();
#endif
        }

        private static void SetFlatContext()
        {
            Debug.Log("Selecting Context: Flat");
            InteractionAdapter = new FlatInteractionAdapter();
        }
    }
}
