using AOT;
using PxCs.Interface;
using System;
using GVR.Debugging;
using UnityEngine;
using UnityEngine.Events;

namespace GVR.Phoenix.Interface.Vm
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



        public static void RegisterExternals(IntPtr vmPtr)
        {
            PxVm.pxVmRegisterExternalDefault(vmPtr, DefaultExternal);
            PxVm.pxVmRegisterExternal(vmPtr, "ConcatStrings", ConcatStrings);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebug", PrintDebug);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugCh", PrintDebugCh);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInst", PrintDebugInst);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInstCh", PrintDebugInstCh); 

            PxVm.pxVmRegisterExternal(vmPtr, "Wld_InsertNpc", Wld_InsertNpc);
            PxVm.pxVmRegisterExternal(vmPtr, "TA_MIN", TA_MIN);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetVisual", Mdl_SetVisual);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_ApplyOverlayMds", Mdl_ApplyOverlayMds);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetVisualBody", Mdl_SetVisualBody);
            PxVm.pxVmRegisterExternal(vmPtr, "EquipItem", EquipItem);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_OUTPUT", AI_OUTPUT);
            
        }

        public static UnityEvent<IntPtr, string> DefaultExternalCallback = new();
        public static UnityEvent<int, string> PhoenixWld_InsertNpc = new();
        public static UnityEvent<TA_MINData> PhoenixTA_MIN = new();
        public static UnityEvent<Mdl_SetVisualData> PhoenixMdl_SetVisual = new();
        public static UnityEvent<Mdl_ApplyOverlayMdsData> PhoenixMdl_ApplyOverlayMds = new();
        public static UnityEvent<Mdl_SetVisualBodyData> PhoenixMdl_SetVisualBody = new();
        public static UnityEvent<EquipItemData> PhoenixEquipItem = new();
        public static UnityEvent<string> PhoenixMdl_AI_OUTPUT = new();



#region Default

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalDefaultCallback))]
        public static void DefaultExternal(IntPtr vmPtr, string missingCallbackName)
        {
            // FIXME: Once solution is released, we can safely throw an exception as it tells us: Brace yourself! The game will not work until you implement it.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");

            // DEBUG During development
            // Debug.LogError("External >" + value + "< not registered but required by DaedalusVM.");

            DefaultExternalCallback.Invoke(vmPtr, missingCallbackName);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void ConcatStrings(IntPtr vmPtr)
        {
            var str2 = PxVm.VmStackPopString(vmPtr);
            var str1 = PxVm.VmStackPopString(vmPtr);
            
            PxVm.pxVmStackPushString(vmPtr, str1 + str2);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebug(IntPtr vmPtr)
        {
            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            var message = PxVm.VmStackPopString(vmPtr);
            Debug.Log($"[zspy]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugCh(IntPtr vmPtr)
        {
            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            var message = PxVm.VmStackPopString(vmPtr);
            var channel = PxVm.pxVmStackPopInt(vmPtr);
            
            Debug.Log($"[zspy,{channel}]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugInst(IntPtr vmPtr)
        {
            if (!FeatureFlags.I.ShowZspyLogs)
                return;

            var message = PxVm.VmStackPopString(vmPtr);
            Debug.Log($"[zspy]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugInstCh(IntPtr vmPtr)
        {
            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            var message = PxVm.VmStackPopString(vmPtr);
            var channel = PxVm.pxVmStackPopInt(vmPtr);
            
            Debug.Log($"[zspy,{channel}]: {message}");
        }
        
#endregion


        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_InsertNpc(IntPtr vmPtr)
        {
            var spawnpoint = PxVm.VmStackPopString(vmPtr);
            var npcInstance = PxVm.pxVmStackPopInt(vmPtr);

            PhoenixWld_InsertNpc.Invoke(npcInstance, spawnpoint);
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

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
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


        public struct Mdl_SetVisualData
        {
            public IntPtr npcPtr;
            public string visual;
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetVisual(IntPtr vmPtr)
        {
            var visual = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            PhoenixMdl_SetVisual.Invoke(
                new()
                {
                    npcPtr = npcPtr,
                    visual = visual
                }
            );
        }


        public struct Mdl_ApplyOverlayMdsData
        {
            public IntPtr npcPtr;
            public string overlayname;
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_ApplyOverlayMds(IntPtr vmPtr)
        {
            var overlayname = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            PhoenixMdl_ApplyOverlayMds.Invoke(
                new()
                {
                    npcPtr = npcPtr,
                    overlayname = overlayname
                }
            );
        }


        public struct Mdl_SetVisualBodyData
        {
            public IntPtr npcPtr;
            public string body;
            public int bodyTexNr;
            public int bodyTexColor;
            public string head;
            public int headTexNr;
            public int teethTexNr;
            public int armor;
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetVisualBody(IntPtr vmPtr)
        {
            var armor = PxVm.pxVmStackPopInt(vmPtr);
            var teethTexNr = PxVm.pxVmStackPopInt(vmPtr);
            var headTexNr = PxVm.pxVmStackPopInt(vmPtr);
            var head = PxVm.VmStackPopString(vmPtr);
            var bodyTexColor = PxVm.pxVmStackPopInt(vmPtr);
            var bodyTexNr = PxVm.pxVmStackPopInt(vmPtr);
            var body = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            PhoenixMdl_SetVisualBody.Invoke(
                new Mdl_SetVisualBodyData()
                {
                    npcPtr = npcPtr,
                    body = body,
                    bodyTexNr = bodyTexNr,
                    bodyTexColor = bodyTexColor,
                    head = head,
                    headTexNr = headTexNr,
                    teethTexNr = teethTexNr,
                    armor = armor
                }
            );
        }

        public struct EquipItemData
        {
            public IntPtr npcPtr;
            public int itemId;
        }
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void EquipItem(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            PhoenixEquipItem.Invoke(new ()
            {
                npcPtr = npcPtr,
                itemId = itemId
            });
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_OUTPUT(IntPtr vmPtr)
        {
            var soundString = PxVm.VmStackPopString(vmPtr);

            PhoenixMdl_AI_OUTPUT.Invoke(soundString);
        }
    }
}
