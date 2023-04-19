using PxCs;
using PxCs.Extensions;
using System;
using UnityEngine.Events;

namespace UZVR.Phoenix.Interface.Vm
{
    /// <summary>
    /// Contains basic methods only available in Gothic Daedalus module.
    /// </summary>
    public static class VmGothicBridge
    {
        public static IntPtr LoadVm(string fullDatFilePath)
        {
            var bufferPtr = LoadBuffer(fullDatFilePath);
            var vmPtr = PxVm.pxVmLoad(bufferPtr);
            PxBuffer.pxBufferDestroy(bufferPtr); // Data isn't needed any longer.

            return vmPtr;
        }

        private static IntPtr LoadBuffer(string fullDatFilePath)
        {
            var bufferPtr = PxBuffer.pxBufferMmap(fullDatFilePath);

            if (bufferPtr == IntPtr.Zero)
                throw new ArgumentNullException($"No buffer loaded. Are you asking for the wrong file?: >{fullDatFilePath}<");

            return bufferPtr;
        }







        public struct TA_MINData
        {
            public IntPtr npc;
            public int start_h;
            public int start_m;
            public int stop_h;
            public int stop_m;
            public int action;
            public string waypoint;
        }

        public static void RegisterExternals(IntPtr vmPtr)
        {
            PxVm.pxVmRegisterExternalDefault(vmPtr, DefaultExternal);

            PxVm.pxVmRegisterExternal(vmPtr, "Wld_InsertNpc", Wld_InsertNpc);
            PxVm.pxVmRegisterExternal(vmPtr, "TA_MIN", TA_MIN);
        }

        public static UnityEvent<IntPtr, string> DefaultExternalCallback = new();
        public static UnityEvent<int, string> PhoenixWld_InsertNpc = new();
        public static UnityEvent<TA_MINData> PhoenixTA_MIN = new();


        public static void DefaultExternal(IntPtr vmPtr, string missingCallbackName)
        {
            // FIXME: Once solution is released, we can safely throw an exception as it tells us: Brace yourself! The game will not work until you implement it.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");

            // DEBUG During development
            // Debug.LogError("External >" + value + "< not registered but required by DaedalusVM.");

            DefaultExternalCallback.Invoke(vmPtr, missingCallbackName);
        }

        public static void Wld_InsertNpc(IntPtr vmPtr)
        {
            var spawnpoint = PxVm.VmStackPopString(vmPtr);
            var npcInstance = PxVm.pxVmStackPopInt(vmPtr);

            PhoenixWld_InsertNpc.Invoke(npcInstance, spawnpoint);
        }

        public static void TA_MIN(IntPtr vmPtr)
        {
            var waypoint = PxVm.VmStackPopString(vmPtr);
            var action = PxVm.pxVmStackPopInt(vmPtr);
            var stop_m = PxVm.pxVmStackPopInt(vmPtr);
            var stop_h = PxVm.pxVmStackPopInt(vmPtr);
            var start_m = PxVm.pxVmStackPopInt(vmPtr);
            var start_h = PxVm.pxVmStackPopInt(vmPtr);
            var npc = PxVm.pxVmStackPopInstance(vmPtr);

            PhoenixTA_MIN.Invoke(
                new TA_MINData
                {
                    npc = npc,
                    start_h = start_h,
                    start_m = start_m,
                    stop_h = stop_h,
                    stop_m = stop_m,
                    action = action,
                    waypoint = waypoint
                }
            );
        }
    }
}
