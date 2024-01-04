using System;
using System.Globalization;
using AOT;
using GVR.Creator;
using GVR.Debugging;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
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
        public static void RegisterExternals()
        {
            var vm = GameData.GothicVm;
            vm.RegisterExternalDefault(DefaultExternal);

            // AI
            vm.RegisterExternal<NpcInstance>("AI_StandUp", AI_StandUp);
            vm.RegisterExternal<NpcInstance, int>("AI_SetWalkMode", AI_SetWalkMode);
            vm.RegisterExternal<NpcInstance, string>("AI_GotoWP", AI_GotoWP);
            vm.RegisterExternal<NpcInstance>("AI_AlignToWP", AI_AlignToWP);
            vm.RegisterExternal<NpcInstance, string>("AI_PlayAni", AI_PlayAni);
            vm.RegisterExternal<NpcInstance, int, int, string>("AI_StartState", AI_StartState);
            vm.RegisterExternal<NpcInstance, int, int>("AI_UseItemToState", AI_UseItemToState);
            vm.RegisterExternal<NpcInstance, float>("AI_Wait", AI_Wait);
            vm.RegisterExternal<int, NpcInstance, string, int>("AI_UseMob", AI_UseMob);
            vm.RegisterExternal<NpcInstance, string>("AI_GoToNextFP", AI_GoToNextFP);
            vm.RegisterExternal<NpcInstance>("AI_DrawWeapon", AI_DrawWeapon);
            vm.RegisterExternal<NpcInstance, NpcInstance, string>("AI_Output", AI_Output);
            vm.RegisterExternal<NpcInstance>("AI_StopProcessInfos", AI_StopProcessInfos);

            // Apply Options
            // Doc
            // Helper
            vm.RegisterExternal<int, int>("Hlp_Random", Hlp_Random);
            vm.RegisterExternal<int, string, string>("Hlp_StrCmp", Hlp_StrCmp);
            // vm.RegisterExternal<int, ItemInstance, int>("Hlp_IsItem", Hlp_IsItem); // Not yet implemented
            vm.RegisterExternal<NpcInstance, int>("Hlp_GetNpc", Hlp_GetNpc);

            // Info
            vm.RegisterExternal<int>("Info_ClearChoices", Info_ClearChoices);
            vm.RegisterExternal<int, string, int>("Info_AddChoice", Info_AddChoice);

            // Log

            // Model
            vm.RegisterExternal<NpcInstance, string>("Mdl_SetVisual", Mdl_SetVisual);
            vm.RegisterExternal<NpcInstance, string>("Mdl_ApplyOverlayMds", Mdl_ApplyOverlayMds);
            vm.RegisterExternal<NpcInstance, string, int, int , string, int, int ,int>("Mdl_SetVisualBody", Mdl_SetVisualBody);
            vm.RegisterExternal<NpcInstance, float, float, float>("Mdl_SetModelScale", Mdl_SetModelScale);
            vm.RegisterExternal<NpcInstance, float>("Mdl_SetModelFatness", Mdl_SetModelFatness);

            // Mission

            // Mob

            // NPC
            vm.RegisterExternal<NpcInstance, int, int>("Npc_SetTalentValue", Npc_SetTalentValue);
            vm.RegisterExternal<NpcInstance, int>("CreateInvItem", CreateInvItem);
            vm.RegisterExternal<NpcInstance, int, int>("CreateInvItems", CreateInvItems);
            vm.RegisterExternal<NpcInstance, int, int>("Npc_PercEnable", Npc_PercEnable);
            vm.RegisterExternal<NpcInstance, float>("Npc_SetPercTime", Npc_SetPercTime);
            vm.RegisterExternal<int, NpcInstance>("Npc_GetBodyState", Npc_GetBodyState);
            vm.RegisterExternal<NpcInstance>("Npc_PerceiveAll", Npc_PerceiveAll);
            vm.RegisterExternal<int, NpcInstance, int>("Npc_HasItems", Npc_HasItems);
            vm.RegisterExternal<int, NpcInstance>("Npc_GetStateTime", Npc_GetStateTime);
            vm.RegisterExternal<NpcInstance, int>("Npc_SetStateTime", Npc_SetStateTime);
            vm.RegisterExternal<ItemInstance, NpcInstance>("Npc_GetEquippedArmor", Npc_GetEquippedArmor);
            // vm.RegisterExternal<NpcInstance, VmGothicEnums.Talent, int>("Npc_SetTalentSkill", Npc_SetTalentSkill);
            vm.RegisterExternal<string, NpcInstance>("Npc_GetNearestWP", Npc_GetNearestWP);
            vm.RegisterExternal<int, NpcInstance, string>("Npc_IsOnFP", Npc_IsOnFP);
            vm.RegisterExternal<int, NpcInstance, int>("Npc_WasInState", Npc_WasInState);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItem", Npc_GetInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItemBySlot", Npc_GetInvItemBySlot);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItem", Npc_RemoveInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItems", Npc_RemoveInvItems);
            vm.RegisterExternal<NpcInstance, int>("EquipItem", EquipItem);
            vm.RegisterExternal<int, NpcInstance, NpcInstance>("Npc_GetDistToNpc", Npc_GetDistToNpc);

            // Print
            vm.RegisterExternal<string>("PrintDebug", PrintDebug);
            vm.RegisterExternal<int, string>("PrintDebugCh", PrintDebugCh);
            vm.RegisterExternal<string>("PrintDebugInst", PrintDebugInst);
            vm.RegisterExternal<int, string>("PrintDebugInstCh", PrintDebugInstCh);

            // Sound

            // Day Routine
            vm.RegisterExternal<NpcInstance, int, int, int, int, int, string>("TA_MIN", TA_MIN);

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


        #region AI

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_StandUp(NpcInstance npc)
        {
            NpcHelper.ExtAiStandUp(npc);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_SetWalkMode(NpcInstance npc, int walkMode)
        {
            NpcHelper.ExtAiSetWalkMode(npc, (VmGothicEnums.WalkMode)walkMode);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_GotoWP(NpcInstance npc, string wayPointName)
        {
            NpcHelper.ExtAiGotoWP(npc, wayPointName);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_AlignToWP(NpcInstance npc)
        {
            NpcHelper.ExtAiAlignToWP(npc);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_PlayAni(NpcInstance npc, string name)
        {
            NpcHelper.ExtAiPlayAni(npc, name);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_StartState(NpcInstance npc, int function, int stateBehaviour, string wayPointName)
        {
            NpcHelper.ExtAiStartState(npc, function, Convert.ToBoolean(stateBehaviour), wayPointName);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_UseItemToState(NpcInstance npc, int itemId, int expectedInventoryCount)
        {
            NpcHelper.ExtAiUseItemToState(npc, itemId, expectedInventoryCount);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_Wait(NpcInstance npc, float seconds)
        {
            NpcHelper.ExtAiWait(npc, seconds);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int AI_UseMob(NpcInstance npc, string target, int state)
        {
            NpcHelper.ExtAiUseMob(npc, target, state);

            // Hint: It seems the int value is a bug as no G1 Daedalus usage needs it.
            return 0;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_GoToNextFP(NpcInstance npc, string fpNamePart)
        {
            NpcHelper.ExtAiGoToNextFp(npc, fpNamePart);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void AI_DrawWeapon(NpcInstance npc)
        {
            NpcHelper.ExtAiDrawWeapon(npc);
        }

        public static void AI_Output(NpcInstance self, NpcInstance target, string outputName)
        {
            DialogHelper.ExtAiOutput(self, target, outputName);
        }

        public static void AI_StopProcessInfos(NpcInstance npc)
        {
            DialogHelper.ExtAiStopProcessInfos(npc);
        }

        #endregion

        #region Apply Options

        //

        #endregion

        #region Doc

        //

        #endregion

        #region Helper

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Hlp_Random(int n0)
        {
            return Random.Range(0, n0 - 1);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Hlp_StrCmp(string s1, string s2)
        {
            return (s1 == s2) ? 1 : 0;
        }

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

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static NpcInstance Hlp_GetNpc(int instanceId)
        {
            return NpcCreator.ExtHlpGetNpc(instanceId);
        }

        #endregion

        #region Info

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Info_ClearChoices(int info)
        {
            DialogHelper.ExtInfoClearChoices(info);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Info_AddChoice(int info, string text, int function)
        {
            DialogHelper.ExtInfoAddChoice(info, text, function);
        }

        #endregion

        #region Log

        //

        #endregion

        #region Model

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Mdl_SetVisual(NpcInstance npc, string visual)
        {
            NpcCreator.ExtMdlSetVisual(npc, visual);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Mdl_ApplyOverlayMds(NpcInstance npc, string overlayName)
        {
            NpcCreator.ExtApplyOverlayMds(npc, overlayName);
        }

        public struct ExtSetVisualBodyData
        {
            public NpcInstance Npc;
            public string Body;
            public int BodyTexNr;
            public int BodyTexColor;
            public string Head;
            public int HeadTexNr;
            public int TeethTexNr;
            public int Armor;
        }
        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Mdl_SetVisualBody(NpcInstance npc, string body, int bodyTexNr, int bodyTexColor, string head, int headTexNr, int teethTexNr, int armor)
        {
            NpcCreator.ExtSetVisualBody(new ()
                {
                    Npc = npc,
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

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Mdl_SetModelScale(NpcInstance npc, float x, float y, float z)
        {
            NpcCreator.ExtMdlSetModelScale(npc, new(x, y, z));
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Mdl_SetModelFatness(NpcInstance npc, float fatness)
        {
            NpcCreator.ExtSetModelFatness(npc, fatness);
        }

        #endregion

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

        #region Sound

        //

        #endregion

        #region Daily Routine

        //

        #endregion

        #region Mission

        //

        #endregion

        #region Mob

        //

        #endregion

        #region NPC

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_SetTalentValue(NpcInstance npc, int talent, int level)
        {
            NpcCreator.ExtNpcSetTalentValue(npc, (VmGothicEnums.Talent)talent, level);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void CreateInvItem(NpcInstance npc, int itemId)
        {
            NpcCreator.ExtCreateInvItems(npc, (uint)itemId, 1);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void CreateInvItems(NpcInstance npc, int itemId, int amount)
        {
            NpcCreator.ExtCreateInvItems(npc, (uint)itemId, amount);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_PercEnable(NpcInstance npc, int perception, int function)
        {
            NpcCreator.ExtNpcPerceptionEnable(npc, (VmGothicEnums.PerceptionType)perception, function);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_SetPercTime(NpcInstance npc, float time)
        {
            NpcCreator.ExtNpcSetPerceptionTime(npc, time);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_GetBodyState(NpcInstance npc)
        {
            return (int)NpcHelper.ExtGetBodyState(npc);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_PerceiveAll(NpcInstance npc)
        {
            // NOP

            // Gothic loads all the necessary items into memory to reference them later via Wld_DetectNpc() and Wld_DetectItem().
            // But we don't need to pre-load them and can just load the necessary elements when really needed.
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_HasItems(NpcInstance npc, int itemId)
        {
            var count = NpcHelper.ExtNpcHasItems(npc, (uint)itemId);
            return count;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_GetStateTime(NpcInstance npc)
        {
            var stateTime = NpcHelper.ExtNpcGetStateTime(npc);
            return (int)stateTime;
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_SetStateTime(NpcInstance npc, int seconds)
        {
            NpcHelper.ExtNpcSetStateTime(npc, seconds);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static ItemInstance Npc_GetEquippedArmor(NpcInstance npc)
        {
            return NpcHelper.ExtGetEquippedArmor(npc);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_SetTalentSkill(NpcInstance npc, VmGothicEnums.Talent talent, int level)
        {
            // FIXME - In OpenGothic it adds MDS overlays based on skill level.
            // NpcCreator.ExtNpcSetTalentSkill(npcPtr, talent, level);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static string Npc_GetNearestWP(NpcInstance npc)
        {
            return NpcHelper.ExtGetNearestWayPoint(npc);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_IsOnFP(NpcInstance npc, string vobNamePart)
        {
            var res = NpcHelper.ExtIsNpcOnFp(npc, vobNamePart);
            return Convert.ToInt32(res);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_WasInState(NpcInstance npc, int action)
        {
            var result = NpcHelper.ExtNpcWasInState(npc, (uint)action);
            return Convert.ToInt32(result);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_GetInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItem();
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_GetInvItemBySlot(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItemBySlot();
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_RemoveInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItem();
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void Npc_RemoveInvItems(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItems();
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void EquipItem(NpcInstance npc, int itemId)
        {
            NpcCreator.ExtEquipItem(npc, itemId);
        }

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static int Npc_GetDistToNpc(NpcInstance npc1, NpcInstance npc2)
        {
            return NpcHelper.ExtNpcGetDistToNpc(npc1, npc2);
        }

        #endregion
        
        #region Day Routine

        [MonoPInvokeCallback(typeof(DaedalusVm.ExternalFuncV))]
        public static void TA_MIN(NpcInstance npc, int startH, int startM, int stopH, int stopM, int action,
            string waypoint)
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

        #endregion
    }
}
