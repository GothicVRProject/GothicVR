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
using ZenKit;
using ZenKit.Daedalus;
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


        public static void RegisterExternals()
        {
            var vm = GameData.GothicVm;
            vm.RegisterExternalDefault(DefaultExternal);

            // AI
            // Apply Options
            // Doc
            // Helper
            vm.RegisterExternal<int, int>("Hlp_Random", Hlp_Random);
            vm.RegisterExternal<int, string, string>("Hlp_StrCmp", Hlp_StrCmp);
            // vm.RegisterExternal<int, ItemInstance, int>("Hlp_IsItem", Hlp_IsItem); // Not yet implemented

            // Info
            // Log
            // Model
            // Mission
            // Mob
            // NPC
            vm.RegisterExternal<NpcInstance, int, int>("Npc_SetTalentValue", Npc_SetTalentValue);

            // Print
            vm.RegisterExternal<string>("PrintDebug", PrintDebug);
            vm.RegisterExternal<int, string>("PrintDebugCh", PrintDebugCh);
            vm.RegisterExternal<string>("PrintDebugInst", PrintDebugInst);
            vm.RegisterExternal<int, string>("PrintDebugInstCh", PrintDebugInstCh);

            // Sound
            // Day Routine
            // vm.RegisterExternal<NpcInstance, int, int, int, int, int, string>("TA_MIN", TA_MIN);

            // World
            vm.RegisterExternal<int, string>("Wld_InsertNpc", Wld_InsertNpc);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsFPAvailable", Wld_IsFPAvailable);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsMobAvailable", Wld_IsMobAvailable);
            vm.RegisterExternal<int, NpcInstance, int, int, int, int>("Wld_DetectNpcEx", Wld_DetectNpcEx);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsNextFPAvailable", Wld_IsNextFPAvailable);

            // Misc
            vm.RegisterExternal<string, string, string>("ConcatStrings", ConcatStrings);
            vm.RegisterExternal<string, int>("IntToString", IntToString);
            vm.RegisterExternal<string, float>("FloatToString", FloatToString);
            vm.RegisterExternal<int, float>("FloatToInt", FloatToInt);
            vm.RegisterExternal<float, int>("IntToFloat", IntToFloat);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalDefaultFunction))]
        public static void DefaultExternal(DaedalusVm vm, DaedalusSymbol sym)
        {
            // FIXME: Once GVR is fully released, we can safely throw an exception as it tells us: The game will not work until you implement this missing function.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");
            Debug.LogWarning($"Method >{sym.Name}< not yet implemented in DaedalusVM.");
        }



        #region Print

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void PrintDebug(string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy]: {message}");
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void PrintDebugCh(int channel, string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy,{channel}]: {message}");
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void PrintDebugInst(string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy]: {message}");
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void PrintDebugInstCh(int channel, string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy,{channel}]: {message}");
        }

        #endregion
        #region NPC

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_SetTalentValue(NpcInstance npc, int talent, int level)
        {
            NpcCreator.ExtNpcSetTalentValue(npc, (VmGothicEnums.Talent)talent, level);
        }

        #endregion
        #region Day Routine

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void TA_MIN(NpcInstance npc, int startH, int startM, int stopH, int stopM, int action, string waypoint)
        {
            NpcCreator.ExtTaMin(npc, startH, startM, stopH, stopM, action, waypoint);
        }

        #endregion
        #region World

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Wld_InsertNpc(int npcInstance, string spawnPoint)
        {
            NpcCreator.ExtWldInsertNpc(npcInstance, spawnPoint);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Wld_IsFPAvailable(NpcInstance npc, string fpName)
        {

            var response = NpcHelper.ExtWldIsFPAvailable(npc, fpName);
            return Convert.ToInt32(response);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Wld_IsMobAvailable(NpcInstance npc, string vobName)
        {
            var res = NpcHelper.ExtIsMobAvailable(npc, vobName);
            return Convert.ToInt32(res);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Wld_DetectNpcEx(NpcInstance npc, int npcInstance, int aiState, int guild, int detectPlayer)
        {
            // Logic from Daedalus mentions, that the player will be ignored if 0. Not "detect" if 1.
            var ignorePlayer = !Convert.ToBoolean(detectPlayer);

            var res = NpcHelper.ExtWldDetectNpcEx(npc, npcInstance, aiState, guild, ignorePlayer);

            return Convert.ToInt32(res);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Wld_IsNextFPAvailable(NpcInstance npc, string fpNamePart)
        {
            var result = NpcHelper.ExtIsNextFpAvailable(npc, fpNamePart);
            return Convert.ToInt32(result);
        }

        #endregion
        #region Misc

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static string ConcatStrings(string str1, string str2)
        {
            return str1 + str2;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static string IntToString(int x)
        {
            return x.ToString();
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static string FloatToString(float x)
        {
            return x.ToString(CultureInfo.InvariantCulture);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int FloatToInt(float x)
        {
            return (int)x;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static float IntToFloat(int x)
        {
            return x;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Hlp_StrCmp(string s1, string s2)
        {
            return (s1 == s2) ? 1 : 0;
        }

        #endregion

        // [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        // public static int Hlp_IsItem(ItemInstance item, int instanceName)
        // {
            // TODO - Needs to be reimplemented.
        //     var compareItemSymbol = PxVm.pxVmStackPopInt(vmPtr);
        //     var itemRef = PxVm.pxVmStackPopInstance(vmPtr);
        //
        //     var compareItemRef = AssetCache.TryGetItemData((uint)compareItemSymbol);
        //
        //     bool result;
        //     if (compareItemRef == null)
        //         result = false;
        //     else
        //         result = compareItemRef.instancePtr == itemRef;
        //
        //     PxVm.pxVmStackPushInt(vmPtr, Convert.ToInt32(result));
        // }


























        [Obsolete("Use new ZenKit logic instead.")]
        public static void RegisterLegacyExternals(IntPtr vmPtr)
        {
            // Basic
            PxVm.pxVmRegisterExternalDefault(vmPtr, DefaultExternal);

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

            // NPC visuals
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
        public static int Hlp_Random(int n0)
        {
            return Random.Range(0, n0 - 1);
        }


        
#endregion

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
