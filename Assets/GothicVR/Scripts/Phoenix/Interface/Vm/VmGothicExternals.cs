using System;
using System.Globalization;
using AOT;
using GVR.Caches;
using GVR.Creator;
using GVR.Debugging;
using GVR.Manager;
using GVR.Npc;
using PxCs.Extensions;
using PxCs.Interface;
using UnityEngine;
using Random = UnityEngine.Random;

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
            PxVm.pxVmRegisterExternal(vmPtr, "Hlp_Random", Hlp_Random);
            PxVm.pxVmRegisterExternal(vmPtr, "Hlp_StrCmp", Hlp_StrCmp);
            PxVm.pxVmRegisterExternal(vmPtr, "Hlp_IsItem", Hlp_IsItem);

            // Debug
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebug", PrintDebug);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugCh", PrintDebugCh);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInst", PrintDebugInst);
            PxVm.pxVmRegisterExternal(vmPtr, "PrintDebugInstCh", PrintDebugInstCh); 

            PxVm.pxVmRegisterExternal(vmPtr, "AI_StandUp", AI_StandUp); 
            PxVm.pxVmRegisterExternal(vmPtr, "AI_SetWalkMode", AI_SetWalkMode);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_GotoWP", AI_GotoWP);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_AlignToWP", AI_AlignToWP);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_PlayAni", AI_PlayAni);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_StartState", AI_StartState);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_UseItemToState", AI_UseItemToState);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_Wait", AI_Wait);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_UseMob", AI_UseMob);
            PxVm.pxVmRegisterExternal(vmPtr, "AI_GoToNextFP", AI_GoToNextFP);
            
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_InsertNpc", Wld_InsertNpc);
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_IsFPAvailable", Wld_IsFPAvailable);
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_IsMobAvailable", Wld_IsMobAvailable);
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_DetectNpcEx", Wld_DetectNpcEx);
            PxVm.pxVmRegisterExternal(vmPtr, "Wld_IsNextFPAvailable", Wld_IsNextFPAvailable);
                
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
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetBodyState", Npc_GetBodyState);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_PerceiveAll", Npc_PerceiveAll);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_HasItems", Npc_HasItems);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetStateTime", Npc_GetStateTime);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetStateTime", Npc_SetStateTime);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetEquippedArmor", Npc_GetEquippedArmor);

            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetTalentSkill", Npc_SetTalentSkill);
            PxVm.pxVmRegisterExternal(vmPtr, "CreateInvItem", CreateInvItem);
            PxVm.pxVmRegisterExternal(vmPtr, "CreateInvItems", CreateInvItems);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItem", Npc_GetInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItemBySlot", Npc_GetInvItemBySlot);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItem", Npc_RemoveInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItems", Npc_RemoveInvItems);
            PxVm.pxVmRegisterExternal(vmPtr, "EquipItem", EquipItem);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_SetTalentValue", Npc_SetTalentValue);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetNearestWP", Npc_GetNearestWP);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_IsOnFP", Npc_IsOnFP);
            PxVm.pxVmRegisterExternal(vmPtr, "Npc_WasInState", Npc_WasInState);
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
        public static void Hlp_Random(IntPtr vmPtr)
        {
            var max = PxVm.pxVmStackPopInt(vmPtr);
            var rand = Random.Range(0, max - 1);
            
            PxVm.pxVmStackPushInt(vmPtr, rand);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Hlp_StrCmp(IntPtr vmPtr)
        {
            var str2 = PxVm.pxVmStackPopString(vmPtr);
            var str1 = PxVm.pxVmStackPopString(vmPtr);

            var equal = (str1 == str2) ? 1 : 0;
            
            PxVm.pxVmStackPushInt(vmPtr, equal);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Hlp_IsItem(IntPtr vmPtr)
        {
            var compareItemSymbol = PxVm.pxVmStackPopInt(vmPtr);
            var itemRef = PxVm.pxVmStackPopInstance(vmPtr);

            var compareItemRef = AssetCache.TryGetItemData((uint)compareItemSymbol);

            bool result;
            if (compareItemRef == null)
                result = false;
            else
                result = compareItemRef.instancePtr == itemRef;
            
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(result));
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebug(IntPtr vmPtr)
        {
            var message = PxVm.VmStackPopString(vmPtr);
            
            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            Debug.Log($"[zspy]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugCh(IntPtr vmPtr)
        {
            var message = PxVm.VmStackPopString(vmPtr);
            var channel = PxVm.pxVmStackPopInt(vmPtr);

            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            Debug.Log($"[zspy,{channel}]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugInst(IntPtr vmPtr)
        {
            var message = PxVm.VmStackPopString(vmPtr);

            if (!FeatureFlags.I.ShowZspyLogs)
                return;

            Debug.Log($"[zspy]: {message}");
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void PrintDebugInstCh(IntPtr vmPtr)
        {
            var message = PxVm.VmStackPopString(vmPtr);
            var channel = PxVm.pxVmStackPopInt(vmPtr);

            if (!FeatureFlags.I.ShowZspyLogs)
                return;
            
            Debug.Log($"[zspy,{channel}]: {message}");
        }
        
#endregion


        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_InsertNpc(IntPtr vmPtr)
        {
            var spawnpoint = PxVm.VmStackPopString(vmPtr);
            var npcInstance = PxVm.pxVmStackPopInt(vmPtr);
            
            NpcCreator.ExtWldInsertNpc(npcInstance, spawnpoint);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_IsFPAvailable(IntPtr vmPtr)
        {
            var fpName = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var response = NpcHelper.ExtWldIsFPAvailable(npcPtr, fpName);
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(response));
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_IsMobAvailable(IntPtr vmPtr)
        {
            var vobName = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var res = NpcHelper.ExtIsMobAvailable(npcPtr, vobName);

            PxVm.pxVmStackPushInt(vmPtr , Convert.ToInt32(res));
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_DetectNpcEx(IntPtr vmPtr)
        {
            var detectPlayer = PxVm.pxVmStackPopInt(vmPtr);
            var guild = PxVm.pxVmStackPopInt(vmPtr);
            var aiState = PxVm.pxVmStackPopInt(vmPtr);
            var npcInstance = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            // Logic from Daedalus mentions, that the player will be ignored if 0. Not "detect" if 1.
            var ignorePlayer = !Convert.ToBoolean(detectPlayer);
            
            var res = NpcHelper.ExtWldDetectNpcEx(npcPtr, npcInstance, aiState, guild, ignorePlayer);
            
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(res));
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Wld_IsNextFPAvailable(IntPtr vmPtr)
        {
            var fpNamePart = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var result = NpcHelper.ExtIsNextFpAvailable(npcPtr, fpNamePart);
            
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(result));
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

            NpcCreator.ExtTaMin(new()
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
            NpcHelper.ExtAiStandUp(npcPtr);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_SetWalkMode(IntPtr vmPtr)
        {
            var walkMode = (VmGothicEnums.WalkMode)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            NpcHelper.ExtAiSetWalkMode(npcPtr, walkMode);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_GotoWP(IntPtr vmPtr)
        {
            var spawnPoint = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            NpcHelper.ExtAiGotoWP(npcPtr, spawnPoint);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_AlignToWP(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtAiAlignToWP(npcPtr);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_PlayAni(IntPtr vmPtr)
        {
            var name = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcHelper.ExtAiPlayAni(npcPtr, name);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_StartState(IntPtr vmPtr)
        {
            var wayPointName = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var stateBehaviour = PxVm.pxVmStackPopInt(vmPtr);
            var function = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcHelper.ExtAiStartState(npcPtr, (uint)function, Convert.ToBoolean(stateBehaviour), wayPointName);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_UseItemToState(IntPtr vmPtr)
        {
            var expectedInventoryCount = PxVm.pxVmStackPopInt(vmPtr);
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtAiUseItemToState(npcPtr, (uint)itemId, expectedInventoryCount);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_Wait(IntPtr vmPtr)
        {
            var seconds = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtAiWait(npcPtr, seconds);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_UseMob(IntPtr vmPtr)
        {
            var state = PxVm.pxVmStackPopInt(vmPtr);
            var target = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtAiUseMob(npcPtr, target, state);
            
            // Hint: It seems the int value is a bug as no G1 Daedalus usage needs it.
            PxVm.pxVmStackPushInt(vmPtr, 0);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void AI_GoToNextFP(IntPtr vmPtr)
        {
            var fpNamePart = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtAiGoToNextFp(npcPtr, fpNamePart);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetVisual(IntPtr vmPtr)
        {
            var visual = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.ExtMdlSetVisual(npcPtr, visual);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_ApplyOverlayMds(IntPtr vmPtr)
        {
            var overlayName = PxVm.VmStackPopString(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.ExtApplyOverlayMds(npcPtr, overlayName);
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

            NpcCreator.ExtSetVisualBody(new ()
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

            NpcCreator.ExtMdlSetModelScale(npcPtr, new(x, y, z));
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Mdl_SetModelFatness(IntPtr vmPtr)
        {
            var fatness = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.ExtSetModelFatness(npcPtr, fatness);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Hlp_GetNpc(IntPtr vmPtr)
        {
            var instanceId = PxVm.pxVmStackPopInt(vmPtr);

            var npcPtr = NpcCreator.ExtHlpGetNpc(instanceId);
            
            PxVm.pxVmStackPushInstance(vmPtr, npcPtr);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_PercEnable(IntPtr vmPtr)
        {
            var function = PxVm.pxVmStackPopInt(vmPtr);
            var perception = (VmGothicEnums.PerceptionType)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.ExtNpcPerceptionEnable(npcPtr, perception, function);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetPercTime(IntPtr vmPtr)
        {
            var time = PxVm.pxVmStackPopFloat(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.ExtNpcSetPerceptionTime(npcPtr, time);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetBodyState(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var bodyState = NpcHelper.ExtGetBodyState(npcPtr);
            
            PxVm.pxVmStackPushInt(vmPtr, (int)bodyState);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_PerceiveAll(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            // Do nothing!
            // Gothic loads all the necessary items into memory to reference them later via Wld_DetectNpc() and Wld_DetectItem().
            // But we don't need to pre-load them and can just load the necessary elements when really needed.
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_HasItems(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var count = NpcHelper.ExtNpcHasItems(npcPtr, (uint)itemId);
            
            PxVm.pxVmStackPushInt(vmPtr, count);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetStateTime(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var stateTime = NpcHelper.ExtNpcGetStateTime(npcPtr);
            
            PxVm.pxVmStackPushInt(vmPtr, (int)stateTime);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetStateTime(IntPtr vmPtr)
        {
            var seconds = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcHelper.ExtNpcSetStateTime(npcPtr, seconds);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetEquippedArmor(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            var itemPtr = NpcHelper.ExtGetEquippedArmor(npcPtr);
            
            PxVm.pxVmStackPushInstance(vmPtr, itemPtr);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetTalentSkill(IntPtr vmPtr)
        {
            var level = PxVm.pxVmStackPopInt(vmPtr);
            var talent = (VmGothicEnums.Talent)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            // FIXME - In OpenGothic it adds MDS overlays based on skill level.
            // NpcCreator.ExtNpcSetTalentSkill(npcPtr, talent, level);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_SetTalentValue(IntPtr vmPtr)
        {
            var level = PxVm.pxVmStackPopInt(vmPtr);
            var talent = (VmGothicEnums.Talent)PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.ExtNpcSetTalentValue(npcPtr, talent, level);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetNearestWP(IntPtr vmPtr)
        {
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var name = NpcHelper.ExtGetNearestWayPoint(npcPtr);

            PxVm.pxVmStackPushString(vmPtr, name);
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_IsOnFP(IntPtr vmPtr)
        {
            var vobNamePart = PxVm.pxVmStackPopString(vmPtr).MarshalAsString();
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var res = NpcHelper.ExtIsNpcOnFp(npcPtr, vobNamePart);

            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(res));
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_WasInState(IntPtr vmPtr)
        {
            var action = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            var result = NpcHelper.ExtNpcWasInState(npcPtr, (uint)action);
            
            PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(result));
        }

        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void CreateInvItem(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.ExtCreateInvItems(npcPtr, (uint)itemId, 1);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void CreateInvItems(IntPtr vmPtr)
        {
            var amount = PxVm.pxVmStackPopInt(vmPtr);
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);
            
            NpcCreator.ExtCreateInvItems(npcPtr, (uint)itemId, amount);
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItem();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_GetInvItemBySlot(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItemBySlot();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_RemoveInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItem();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void Npc_RemoveInvItems(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItems();
        }
        
        [MonoPInvokeCallback(typeof(PxVm.PxVmExternalCallback))]
        public static void EquipItem(IntPtr vmPtr)
        {
            var itemId = PxVm.pxVmStackPopInt(vmPtr);
            var npcPtr = PxVm.pxVmStackPopInstance(vmPtr);

            NpcCreator.ExtEquipItem(npcPtr, itemId);
        }
    }
}
