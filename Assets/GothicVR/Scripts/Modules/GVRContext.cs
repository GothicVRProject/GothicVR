using System;
using GVR.Context.Controls;
using GVR.Flat;
#if GVR_HVR_INSTALLED
// using Foo.Bar;
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

#else
            throw new Exception("HVR isn't activated in Compiler flags. Please ensure your PlayerSettings have ScriptCompilation flag called >GVR_HVR_INSTALLED<.");
#endif
        }

        private static void SetFlatContext()
        {
            ClimbingAdapter = new FlatClimbingAdapter();
        }
    }
}
