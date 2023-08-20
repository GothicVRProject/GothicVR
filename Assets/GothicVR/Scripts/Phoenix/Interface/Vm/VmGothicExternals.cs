using System;
using System.Globalization;
using AOT;
using GVR.Creator;
using GVR.Debugging;
using PxCs.Extensions;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Phoenix.Interface.Vm
{
    /// <summary>
    /// Contains basic methods only available in Gothic Daedalus module.
    /// </summary>
    public static class VmGothicExternals
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
            // Basic
            PxVm.pxVmRegisterExternalDefault(vmPtr, DefaultExternal);
            PxVm.pxVmRegisterExternal(vmPtr, "ConcatStrings", ConcatStrings);
            PxVm.pxVmRegisterExternal(vmPtr, "IntToString", IntToString);
            PxVm.pxVmRegisterExternal(vmPtr, "FloatToString", FloatToString);
            PxVm.pxVmRegisterExternal(vmPtr, "FloatToInt", FloatToInt);
            PxVm.pxVmRegisterExternal(vmPtr, "IntToFloat", IntToFloat);

            // Debug
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebug", PrintDebug);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugCh", PrintDebugCh);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInst", PrintDebugInst);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInstCh", PrintDebugInstCh); 

            PxVm.pxVmRegisterExternal(vmPtr, "AI_StandUp", AI_StandUp); 
            
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_InsertNpc", Wld_InsertNpc);
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_IsFPAvailable", Wld_IsFPAvailable);

            // NPC visuals
            PxVm.pxVmRegisterExternal(vmPtr, "TA_MIN", TA_MIN);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetVisual", Mdl_SetVisual);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_ApplyOverlayMds", Mdl_ApplyOverlayMds);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetVisualBody", Mdl_SetVisualBody);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetModelScale", Mdl_SetModelScale);
            PxVm.pxVmRegisterExternal(vmPtr, "Mdl_SetModelFatness", Mdl_SetModelFatness);

            // NPC items/talents/...
            PxVm.pxVmRegisterExternal(vmPtr, "Hlp_GetNpc", Hlp_GetNpc);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_PercEnable", Npc_PercEnable);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetPercTime", Npc_SetPercTime);

            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetTalentSkill", Npc_SetTalentSkill);
            PxVm.pxVmRegisterExternal(vmPtr, "CreateInvItem", CreateInvItem);
            PxVm.pxVmRegisterExternal(vmPtr, "CreateInvItems", CreateInvItems);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItem", Npc_GetInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItemBySlot", Npc_GetInvItemBySlot);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItem", Npc_RemoveInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItems", Npc_RemoveInvItems);
            PxVm.pxVmRegisterExternal(vmPtr, "EquipItem", EquipItem);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetTalentValue", Npc_SetTalentValue);

            PxVm.pxVmRegisterExternal(vmPtr, "AI_OUTPUT", AI_OUTPUT);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_SetWalkMode", AI_SetWalkMode);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_GotoWP", AI_GotoWP);
        }

#region Default

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalDefaultCallback))]
        public static void DefaultExternal(IntPtr vmPtr, string missingCallbackName)
        {
            // FIXME: Once solution is released, we can safely throw an exception as it tells us: The game will not work until you implement this missing function.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");
            Debug.LogWarning($"Method >{missingCallbackName}< not yet implemented in DaedalusVM.");
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void ConcatStrings(IntPtr vmPtr)
        {
            var str2 = PxVm.VmStackPopString(vmPtr);
            var str1 = PxVm.VmStackPopString(vmPtr);
            
            PxVm.pxVmStackPushString(vmPtr, str1 + str2);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void IntToString(IntPtr vmPtr)
        {
            var val = PxVm.pxVmStackPopInt(vmPtr);
            PxVm.pxVmStackPushString(vmPtr, val.ToString());
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void FloatToString(IntPtr vmPtr)
        {
            var val = PxVm.pxVmStackPopFloat(vmPtr);
            PxVm.pxVmStackPushString(vmPtr, val.ToString(CultureInfo.InvariantCulture));
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void FloatToInt(IntPtr vmPtr)
        {
            var val = PxVm.pxVmStackPopFloat(vmPtr);
            PxVm.pxVmStackPushInt(vmPtr, (int)val);
        }
            
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void IntToFloat(IntPtr vmPtr)
        {
            var val = PxVm.pxVmStackPopInt(vmPtr);
            PxVm.pxVmStackPushFloat(vmPtr, val);
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
            
            NpcCreator.I.ExtWldInsertNpc(npcInstance, spawnpoint);
        }

        public static void Wld_IsFPAvailable(IntPtr vmPtr)
        {
            var fpName = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var response = NpcCreator.I.ExtWldIsFPAvailable(npcPtr, fpName);
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(response));
        }

        public struct ExtTaMinData
        {
            public IntPtr Npc;
            public int StartH;
            public int StartM;
            public int StopH;
            public int StopM;
            public int Action;
            public string Waypoint;
        }
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void TA_MIN(IntPtr vmPtr)
        {
            var waypoint = PxVm.VmStackPopString(vmPtr);
            var action = PxVm.pxVmStackPopInt(vmPtr);
            var stopM = PxVm.pxVmStackPopInt(vmPtr);
            var stopH = PxVm.pxVmStackPopInt(vmPtr);
            var startM = PxVm.pxVmStackPopInt(vmPtr);
            var startH = PxVm.pxVmStackPopInt(vmPtr);
            var npc = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtTaMin(new()
            {
                Npc = npc,
                StartH = startH,
                StartM = startM,
                StopH = stopH,
                StopM = stopM,
                Action = action,
                Waypoint = waypoint
            });
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_StandUp(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            NpcCreator.I.ExtAiStandUp(npcPtr);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_SetWalkMode(IntPtr vmPtr)
        {
            var walkMode = (VmGothicEnums.WalkMode)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            NpcCreator.I.ExtAiSetWalkMode(npcPtr, walkMode);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_GotoWP(IntPtr vmPtr)
        {
            var spawnPoint = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            NpcCreator.I.ExtAiGotoWP(npcPtr, spawnPoint);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetVisual(IntPtr vmPtr)
        {
            var visual = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtMdlSetVisual(npcPtr, visual);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_ApplyOverlayMds(IntPtr vmPtr)
        {
            var overlayName = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtApplyOverlayMds(npcPtr, overlayName);
        }
        
        public struct ExtSetVisualBodyData
        {
            public IntPtr NpcPtr;
            public string Body;
            public int BodyTexNr;
            public int BodyTexColor;
            public string Head;
            public int HeadTexNr;
            public int TeethTexNr;
            public int Armor;
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

            NpcCreator.I.ExtSetVisualBody(new ()
                {
                    NpcPtr = npcPtr,
                    Body = body,
                    BodyTexNr = bodyTexNr,
                    BodyTexColor = bodyTexColor,
                    Head = head,
                    HeadTexNr = headTexNr,
                    TeethTexNr = teethTexNr,
                    Armor = armor
                }
            );
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetModelScale(IntPtr vmPtr)
        {
            var z = PxVm.pxVmStackPopFloat(vmPtr);
            var y = PxVm.pxVmStackPopFloat(vmPtr);
            var x = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtMdlSetModelScale(npcPtr, new(x, y, z));
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetModelFatness(IntPtr vmPtr)
        {
            var fatness = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtSetModelFatness(npcPtr, fatness);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Hlp_GetNpc(IntPtr vmPtr)
        {
            var instanceId = PxVm.pxVmStackPopInt(vmPtr);

            var npcPtr = NpcCreator.I.ExtHlpGetNpc(instanceId);
            
            PxVm.pxVmStackPushInstance(vmPtr, npcPtr);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_PercEnable(IntPtr vmPtr)
        {
            var function = PxVm.pxVmStackPopInt(vmPtr);
            var perception = (VmGothicEnums.PerceptionType)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.I.ExtNpcPerceptionEnable(npcPtr, perception, function);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetPercTime(IntPtr vmPtr)
        {
            var time = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.I.ExtNpcSetPerceptionTime(npcPtr, time);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetTalentSkill(IntPtr vmPtr)
        {
            var level = PxVm.pxVmStackPopInt(vmPtr);
            var talent = (VmGothicEnums.Talent)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            // FIXME - In OpenGothic it adds MDS overlays based on skill level.
            // NpcCreator.I.ExtNpcSetTalentSkill(npcPtr, talent, level);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetTalentValue(IntPtr vmPtr)
        {
            var level = PxVm.pxVmStackPopInt(vmPtr);
            var talent = (VmGothicEnums.Talent)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.I.ExtNpcSetTalentValue(npcPtr, talent, level);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void CreateInvItem(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.I.ExtCreateInvItems(npcPtr, itemId, 1);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void CreateInvItems(IntPtr vmPtr)
        {
            var amount = PxVm.pxVmStackPopInt(vmPtr);
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.I.ExtCreateInvItems(npcPtr, itemId, amount);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetInvItem(IntPtr vmPtr)
        {
            // NpcCreator.I.ExtGetInvItem();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetInvItemBySlot(IntPtr vmPtr)
        {
            // NpcCreator.I.ExtGetInvItemBySlot();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_RemoveInvItem(IntPtr vmPtr)
        {
            // NpcCreator.I.ExtRemoveInvItem();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_RemoveInvItems(IntPtr vmPtr)
        {
            // NpcCreator.I.ExtRemoveInvItems();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void EquipItem(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.I.ExtEquipItem(npcPtr, itemId);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_OUTPUT(IntPtr vmPtr)
        {
            var soundString = PxVm.VmStackPopString(vmPtr);

            SoundCreator.I.ExtAiOutput(soundString);
        }
    }
}
