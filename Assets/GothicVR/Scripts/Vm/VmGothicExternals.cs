using System;
using System.Globalization;
using AOT;
using GVR.Caches;
using GVR.Creator;
using GVR.Debugging;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.World;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Random = UnityEngine.Random;

namespace GVR.Vm
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
            vm.RegisterExternal<NpcInstance>("AI_AlignToFP", AI_AlignToFP);
            vm.RegisterExternal<NpcInstance>("AI_AlignToWP", AI_AlignToWP);
            vm.RegisterExternal<NpcInstance, string>("AI_GotoFP", AI_GotoFP);
            vm.RegisterExternal<NpcInstance, string>("AI_GotoWP", AI_GotoWP);
            vm.RegisterExternal<NpcInstance, NpcInstance>("AI_GotoNpc", AI_GotoNpc);
            vm.RegisterExternal<NpcInstance, string>("AI_PlayAni", AI_PlayAni);
            vm.RegisterExternal<NpcInstance, int, int, string>("AI_StartState", AI_StartState);
            vm.RegisterExternal<NpcInstance, int, int>("AI_UseItemToState", AI_UseItemToState);
            vm.RegisterExternal<NpcInstance, float>("AI_Wait", AI_Wait);
            vm.RegisterExternal<int, NpcInstance, string, int>("AI_UseMob", AI_UseMob);
            vm.RegisterExternal<NpcInstance, string>("AI_GoToNextFP", AI_GoToNextFP);
            vm.RegisterExternal<NpcInstance>("AI_DrawWeapon", AI_DrawWeapon);
            vm.RegisterExternal<NpcInstance, NpcInstance, string>("AI_Output", AI_Output);
            vm.RegisterExternal<NpcInstance>("AI_StopProcessInfos", AI_StopProcessInfos);
            vm.RegisterExternal<NpcInstance, string>("AI_LookAt", AI_LookAt);
            vm.RegisterExternal<NpcInstance, NpcInstance>("AI_LookAtNPC", AI_LookAtNPC);
            vm.RegisterExternal<NpcInstance>("AI_ContinueRoutine", AI_ContinueRoutine);
            vm.RegisterExternal<NpcInstance, NpcInstance>("AI_TurnToNPC", AI_TurnToNPC);
            vm.RegisterExternal<NpcInstance, string, int>("AI_PlayAniBS", AI_PlayAniBS);
            vm.RegisterExternal<NpcInstance>("AI_UnequipArmor", AI_UnequipArmor);
            vm.RegisterExternal<NpcInstance, NpcInstance, string>("AI_OutputSVM", AI_OutputSVM);

            // Apply Options
            // Doc
            // Helper
            vm.RegisterExternal<int, int>("Hlp_Random", Hlp_Random);
            vm.RegisterExternal<int, string, string>("Hlp_StrCmp", Hlp_StrCmp);
            vm.RegisterExternal<int, ItemInstance, int>("Hlp_IsItem", Hlp_IsItem);
            vm.RegisterExternal<int, ItemInstance>("Hlp_IsValidItem", Hlp_IsValidItem);
            vm.RegisterExternal<int, NpcInstance>("Hlp_IsValidNpc", Hlp_IsValidNpc);
            vm.RegisterExternal<NpcInstance, int>("Hlp_GetNpc", Hlp_GetNpc);
            vm.RegisterExternal<int, DaedalusInstance>("Hlp_GetInstanceId", Hlp_GetInstanceId);

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
            vm.RegisterExternal<NpcInstance, int, int>("Npc_SetTalentSkill", Npc_SetTalentSkill);
            vm.RegisterExternal<string, NpcInstance>("Npc_GetNearestWP", Npc_GetNearestWP);
            vm.RegisterExternal<int, NpcInstance, string>("Npc_IsOnFP", Npc_IsOnFP);
            vm.RegisterExternal<int, NpcInstance, int>("Npc_WasInState", Npc_WasInState);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItem", Npc_GetInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_GetInvItemBySlot", Npc_GetInvItemBySlot);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItem", Npc_RemoveInvItem);
            // PxVm.pxVmRegisterExternal(vmPtr, "Npc_RemoveInvItems", Npc_RemoveInvItems);
            vm.RegisterExternal<NpcInstance, int>("EquipItem", EquipItem);
            vm.RegisterExternal<int, NpcInstance, NpcInstance>("Npc_GetDistToNpc", Npc_GetDistToNpc);
            vm.RegisterExternal<int, NpcInstance>("Npc_HasEquippedArmor", Npc_HasEquippedArmor);
            vm.RegisterExternal<ItemInstance, NpcInstance>("Npc_GetEquippedMeleeWeapon", Npc_GetEquippedMeleeWeapon);
            vm.RegisterExternal<int, NpcInstance>("Npc_HasEquippedMeleeWeapon", Npc_HasEquippedMeleeWeapon);
            vm.RegisterExternal<ItemInstance, NpcInstance>("Npc_GetEquippedRangedWeapon", Npc_GetEquippedRangedWeapon);
            vm.RegisterExternal<int, NpcInstance>("Npc_HasEquippedRangedWeapon", Npc_HasEquippedRangedWeapon);
            vm.RegisterExternal<int, NpcInstance, string>("Npc_GetDistToWP", Npc_GetDistToWP);
            vm.RegisterExternal<NpcInstance, int>("Npc_PercDisable", Npc_PercDisable);
            vm.RegisterExternal<int, NpcInstance, NpcInstance>("Npc_CanSeeNpc", Npc_CanSeeNpc);
            vm.RegisterExternal<NpcInstance>("Npc_ClearAiQueue", Npc_ClearAiQueue);
            // vm.RegisterExternal<NpcInstance>("Npc_ClearInventory", Npc_ClearInventory);
            vm.RegisterExternal<string, NpcInstance>("Npc_GetNextWp", Npc_GetNextWp);
            // vm.RegisterExternal<int, NpcInstance, int>("Npc_GetTalentSkill", Npc_GetTalentSkill);
            vm.RegisterExternal<int, NpcInstance, int>("Npc_GetTalentValue", Npc_GetTalentValue);


            // Print
            vm.RegisterExternal<string>("PrintDebug", PrintDebug);
            vm.RegisterExternal<int, string>("PrintDebugCh", PrintDebugCh);
            vm.RegisterExternal<string>("PrintDebugInst", PrintDebugInst);
            vm.RegisterExternal<int, string>("PrintDebugInstCh", PrintDebugInstCh);

            // Sound

            // Day Routine
            vm.RegisterExternal<NpcInstance, int, int, int, int, int, string>("TA_MIN", TA_MIN);
            vm.RegisterExternal<NpcInstance, int, int, int, string>("TA", TA);
            vm.RegisterExternal<NpcInstance, string>("Npc_ExchangeRoutine", Npc_ExchangeRoutine);

            // World
            vm.RegisterExternal<int, string>("Wld_InsertNpc", Wld_InsertNpc);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsFPAvailable", Wld_IsFPAvailable);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsMobAvailable", Wld_IsMobAvailable);
            vm.RegisterExternal<int, NpcInstance, int, int, int>("Wld_DetectNpc", Wld_DetectNpc);
            vm.RegisterExternal<int, NpcInstance, int, int, int, int>("Wld_DetectNpcEx", Wld_DetectNpcEx);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_IsNextFPAvailable", Wld_IsNextFPAvailable);
            vm.RegisterExternal<int, int>("Wld_SetTime", Wld_SetTime);
            vm.RegisterExternal("Wld_GetDay", Wld_GetDay);
            vm.RegisterExternal<int, int, int, int, int>("Wld_IsTime", Wld_IsTime);
            vm.RegisterExternal<int, NpcInstance, string>("Wld_GetMobState", Wld_GetMobState);
            vm.RegisterExternal<int, string>("Wld_InsertItem", Wld_InsertItem);

            // Misc
            vm.RegisterExternal<string, string, string>("ConcatStrings", ConcatStrings);
            vm.RegisterExternal<string, int>("IntToString", IntToString);
            vm.RegisterExternal<string, float>("FloatToString", FloatToString);
            vm.RegisterExternal<int, float>("FloatToInt", FloatToInt);
            vm.RegisterExternal<float, int>("IntToFloat", IntToFloat);
        }

        
        public static void DefaultExternal(DaedalusVm vm, DaedalusSymbol sym)
        {
            // FIXME: Once GVR is fully released, we can safely throw an exception as it tells us: The game will not work until you implement this missing function.
            //throw new NotImplementedException("External >" + value + "< not registered but required by DaedalusVM.");
            try
            {
                if (GameData.GothicVm.GlobalSelf == null)
                {
                    Debug.LogWarning($"Method >{sym.Name}< not yet implemented in DaedalusVM.");
                }
                else
                {
                    var npcName = LookupCache.NpcCache[GameData.GothicVm.GlobalSelf.Index].go.name;
                    Debug.LogWarning($"Method >{sym.Name}< not yet implemented in DaedalusVM (called on >{npcName}<).");
                }
            }
            catch (Exception)
            {
                Debug.LogError("Bug in getting Npc. Fix or delete.");
            }
        }


        #region AI

        public static void AI_StandUp(NpcInstance npc)
        {
            NpcHelper.ExtAiStandUp(npc);
        }

        public static void AI_SetWalkMode(NpcInstance npc, int walkMode)
        {
            NpcHelper.ExtAiSetWalkMode(npc, (VmGothicEnums.WalkMode)walkMode);
        }

        public static void AI_AlignToFP(NpcInstance npc)
        {
            NpcHelper.ExtAiAlignToFp(npc);
        }
        
        public static void AI_AlignToWP(NpcInstance npc)
        {
            NpcHelper.ExtAiAlignToWp(npc);
        }

        public static void AI_GotoFP(NpcInstance npc, string freePointName)
        {
            NpcHelper.ExtAiGoToFp(npc, freePointName);
        }
        
        public static void AI_GotoWP(NpcInstance npc, string wayPointName)
        {
            NpcHelper.ExtAiGoToWp(npc, wayPointName);
        }

        public static void AI_GotoNpc(NpcInstance self, NpcInstance other)
        {
            NpcHelper.ExtAiGoToNpc(self, other);
        }

        public static void AI_PlayAni(NpcInstance npc, string name)
        {
            NpcHelper.ExtAiPlayAni(npc, name);
        }

        public static void AI_StartState(NpcInstance npc, int function, int stateBehaviour, string wayPointName)
        {
            NpcHelper.ExtAiStartState(npc, function, Convert.ToBoolean(stateBehaviour), wayPointName);
        }

        public static void AI_UseItemToState(NpcInstance npc, int itemId, int expectedInventoryCount)
        {
            NpcHelper.ExtAiUseItemToState(npc, itemId, expectedInventoryCount);
        }

        public static void AI_Wait(NpcInstance npc, float seconds)
        {
            NpcHelper.ExtAiWait(npc, seconds);
        }

        public static int AI_UseMob(NpcInstance npc, string target, int state)
        {
            NpcHelper.ExtAiUseMob(npc, target, state);

            // Hint: It seems the int value is a bug as no G1 Daedalus usage needs it.
            return 0;
        }

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

        public static void AI_LookAt(NpcInstance npc, string waypoint)
        {
            NpcHelper.ExtAiLookAt(npc, waypoint);
        }

        public static void AI_LookAtNPC(NpcInstance npc, NpcInstance target)
        {
            NpcHelper.ExtAiLookAtNpc(npc, target);
        }

        public static void AI_ContinueRoutine(NpcInstance npc)
        {
            NpcHelper.ExtAiContinueRoutine(npc);
        }

        public static void AI_TurnToNPC(NpcInstance npc, NpcInstance target)
        {
            NpcHelper.ExtAiTurnToNpc(npc, target);
        }

        public static void AI_PlayAniBS(NpcInstance npc, string name, int bodyState)
        {
            NpcHelper.ExtAiPlayAniBS(npc, name, bodyState);
        }

        public static void AI_UnequipArmor(NpcInstance npc)
        {
            NpcHelper.ExtAiUnequipArmor(npc);
        }

        public static void AI_OutputSVM(NpcInstance npc, NpcInstance target, string svmname)
        {
            DialogHelper.ExtAiOutputSvm(npc, target, svmname);
        }

        #endregion

        #region Apply Options

        //

        #endregion

        #region Doc

        //

        #endregion

        #region Helper

        
        public static int Hlp_Random(int n0)
        {
            return Random.Range(0, n0 - 1);
        }

        
        public static int Hlp_StrCmp(string s1, string s2)
        {
            return (s1 == s2) ? 1 : 0;
        }

        public static int Hlp_IsItem(ItemInstance item, int itemIndexToCheck)
        {
            return Convert.ToInt32(item.Index == itemIndexToCheck);
        }

        public static int Hlp_IsValidItem(ItemInstance item)
        {
            return Convert.ToInt32(item != null);
        }

        public static int Hlp_IsValidNpc(NpcInstance npc)
        {
            return Convert.ToInt32(npc != null);
        }

        public static NpcInstance Hlp_GetNpc(int instanceId)
        {
            return NpcCreator.ExtHlpGetNpc(instanceId);
        }

        public static int Hlp_GetInstanceId(DaedalusInstance instanceId)
        {
            return NpcCreator.ExtHlpGetInstanceId(instanceId);
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

        
        public static void Mdl_SetVisual(NpcInstance npc, string visual)
        {
            NpcCreator.ExtMdlSetVisual(npc, visual);
        }

        
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

        
        public static void Mdl_SetModelScale(NpcInstance npc, float x, float y, float z)
        {
            NpcCreator.ExtMdlSetModelScale(npc, new(x, y, z));
        }

        
        public static void Mdl_SetModelFatness(NpcInstance npc, float fatness)
        {
            NpcCreator.ExtSetModelFatness(npc, fatness);
        }

        #endregion

        #region Print

        
        public static void PrintDebug(string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy]: {message}");
        }

        
        public static void PrintDebugCh(int channel, string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy,{channel}]: {message}");
        }

        
        public static void PrintDebugInst(string message)
        {
            if (!FeatureFlags.I.showZspyLogs)
                return;

            Debug.Log($"[zspy]: {message}");
        }

        
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

        
        public static void Npc_SetTalentValue(NpcInstance npc, int talent, int level)
        {
            NpcCreator.ExtNpcSetTalentValue(npc, (VmGothicEnums.Talent)talent, level);
        }

        
        public static void CreateInvItem(NpcInstance npc, int itemId)
        {
            NpcCreator.ExtCreateInvItems(npc, (uint)itemId, 1);
        }

        
        public static void CreateInvItems(NpcInstance npc, int itemId, int amount)
        {
            NpcCreator.ExtCreateInvItems(npc, (uint)itemId, amount);
        }

        
        public static void Npc_PercEnable(NpcInstance npc, int perception, int function)
        {
            NpcCreator.ExtNpcPerceptionEnable(npc, (VmGothicEnums.PerceptionType)perception, function);
        }

        
        public static void Npc_SetPercTime(NpcInstance npc, float time)
        {
            NpcCreator.ExtNpcSetPerceptionTime(npc, time);
        }

        
        public static int Npc_GetBodyState(NpcInstance npc)
        {
            return (int)NpcHelper.ExtGetBodyState(npc);
        }

        
        public static void Npc_PerceiveAll(NpcInstance npc)
        {
            // NOP

            // Gothic loads all the necessary items into memory to reference them later via Wld_DetectNpc() and Wld_DetectItem().
            // But we don't need to pre-load them and can just load the necessary elements when really needed.
        }

        
        public static int Npc_HasItems(NpcInstance npc, int itemId)
        {
            var count = NpcHelper.ExtNpcHasItems(npc, (uint)itemId);
            return count;
        }

        
        public static int Npc_GetStateTime(NpcInstance npc)
        {
            var stateTime = NpcHelper.ExtNpcGetStateTime(npc);
            return stateTime;
        }

        
        public static void Npc_SetStateTime(NpcInstance npc, int seconds)
        {
            NpcHelper.ExtNpcSetStateTime(npc, seconds);
        }

        
        public static ItemInstance Npc_GetEquippedArmor(NpcInstance npc)
        {
            return NpcHelper.ExtGetEquippedArmor(npc);
        }

        
        public static void Npc_SetTalentSkill(NpcInstance npc, int talent, int level)
        {
            NpcCreator.ExtNpcSetTalentSkill(npc, (VmGothicEnums.Talent)talent, level);
        }

        public static string Npc_GetNearestWP(NpcInstance npc)
        {
            return NpcHelper.ExtGetNearestWayPoint(npc);
        }

        
        public static int Npc_IsOnFP(NpcInstance npc, string vobNamePart)
        {
            var res = NpcHelper.ExtIsNpcOnFp(npc, vobNamePart);
            return Convert.ToInt32(res);
        }

        
        public static int Npc_WasInState(NpcInstance npc, int action)
        {
            var result = NpcHelper.ExtNpcWasInState(npc, (uint)action);
            return Convert.ToInt32(result);
        }

        
        public static void Npc_GetInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItem();
        }

        
        public static void Npc_GetInvItemBySlot(IntPtr vmPtr)
        {
            // NpcCreator.ExtGetInvItemBySlot();
        }

        
        public static void Npc_RemoveInvItem(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItem();
        }

        
        public static void Npc_RemoveInvItems(IntPtr vmPtr)
        {
            // NpcCreator.ExtRemoveInvItems();
        }

        
        public static void EquipItem(NpcInstance npc, int itemId)
        {
            NpcCreator.ExtEquipItem(npc, itemId);
        }

        
        public static int Npc_GetDistToNpc(NpcInstance npc1, NpcInstance npc2)
        {
            return NpcHelper.ExtNpcGetDistToNpc(npc1, npc2);
        }
        
        public static int Npc_HasEquippedArmor(NpcInstance npc)
        {
            return NpcHelper.ExtNpcHasEquippedArmor(npc) ? 1 : 0;
        }

        public static ItemInstance Npc_GetEquippedMeleeWeapon(NpcInstance npc)
        {
            return NpcHelper.ExtNpcGetEquippedMeleeWeapon(npc);
        }

        public static int Npc_HasEquippedMeleeWeapon(NpcInstance npc)
        {
            return NpcHelper.ExtNpcHasEquippedMeleeWeapon(npc) ? 1 : 0;
        }

        public static ItemInstance Npc_GetEquippedRangedWeapon(NpcInstance npc)
        {
            return NpcHelper.ExtNpcGetEquippedRangedWeapon(npc);
        }

        public static int Npc_HasEquippedRangedWeapon(NpcInstance npc)
        {
            return NpcHelper.ExtNpcHasEquippedRangedWeapon(npc) ? 1 : 0;
        }

        public static int Npc_GetDistToWP(NpcInstance npc, string waypoint)
        {
            return NpcHelper.ExtNpcGetDistToWp(npc, waypoint);
        }

        public static void Npc_PercDisable(NpcInstance npc, int perception)
        {
            NpcCreator.ExtNpcPerceptionDisable(npc, (VmGothicEnums.PerceptionType)perception);
        }

        public static int Npc_CanSeeNpc(NpcInstance npc, NpcInstance target)
        {
            return NpcHelper.ExtNpcCanSeeNpc(npc, target) ? 1 : 0;
        }

        public static void Npc_ClearAiQueue(NpcInstance npc)
        {
            NpcHelper.ExtNpcClearAiQueue(npc);
        }

        public static void Npc_ClearInventory(NpcInstance npc)
        {
            NpcHelper.ExtNpcClearInventory(npc);
        }

        public static string Npc_GetNextWp(NpcInstance npc)
        {
            return NpcHelper.ExtNpcGetNextWp(npc);
        }

        public static int Npc_GetTalentSkill(NpcInstance npc, int skillId)
        {
            return NpcHelper.ExtNpcGetTalentSkill(npc, skillId);
        }

        public static int Npc_GetTalentValue(NpcInstance npc, int skillId)
        {
            return NpcHelper.ExtNpcGetTalentValue(npc, skillId);
        }
        
        

        #endregion
        
        #region Day Routine

        
        public static void TA_MIN(NpcInstance npc, int startH, int startM, int stopH, int stopM, int action,
            string waypoint)
        {
            NpcCreator.ExtTaMin(npc, startH, startM, stopH, stopM, action, waypoint);
        }
        
        public static void TA(NpcInstance npc, int startH, int stopH, int action,
            string waypoint)
        {
            NpcCreator.ExtTaMin(npc, startH, 0, stopH, 0, action, waypoint);
        }

        public static void Npc_ExchangeRoutine(NpcInstance self, string routineName)
        {
            NpcHelper.ExtNpcExchangeRoutine(self, routineName);
        }

        #endregion

        #region World

        
        public static void Wld_InsertNpc(int npcInstance, string spawnPoint)
        {
            NpcCreator.ExtWldInsertNpc(npcInstance, spawnPoint);
        }

        
        public static int Wld_IsFPAvailable(NpcInstance npc, string fpName)
        {

            var response = NpcHelper.ExtWldIsFPAvailable(npc, fpName);
            return Convert.ToInt32(response);
        }

        
        public static int Wld_IsMobAvailable(NpcInstance npc, string vobName)
        {
            var res = NpcHelper.ExtIsMobAvailable(npc, vobName);
            return Convert.ToInt32(res);
        }

        public static int Wld_DetectNpc(NpcInstance npc, int npcInstance, int aiState, int guild)
        {
            return Wld_DetectNpcEx(npc, npcInstance, aiState, guild, 1);
        }

        public static int Wld_DetectNpcEx(NpcInstance npc, int npcInstance, int aiState, int guild, int detectPlayer)
        {
            var res = NpcHelper.ExtWldDetectNpcEx(npc, npcInstance, aiState, guild, Convert.ToBoolean(detectPlayer));

            return Convert.ToInt32(res);
        }
        
        public static int Wld_IsNextFPAvailable(NpcInstance npc, string fpNamePart)
        {
            var result = NpcHelper.ExtIsNextFpAvailable(npc, fpNamePart);
            return Convert.ToInt32(result);
        }

        public static void Wld_SetTime(int hour, int minute)
        {
            GameTime.I.SetTime(hour, minute);
        }

        public static int Wld_GetDay()
        {
            return GameTime.I.GetDay();
        }

        public static int Wld_IsTime(int beginHour, int beginMinute, int endHour, int endMinute)
        {
            var begin = new TimeSpan(beginHour, beginMinute, 0);
            var end = new TimeSpan(endHour, endMinute, 0);

            var now = DateTime.Now.TimeOfDay;

            if (begin <= end && begin <= now && now < end)
            {
                return 1;
            }

            if (begin > end && (begin < now || now <= end)) // begin and end span across midnight
            {
                return 1;
            }

            return 0;
        }

        public static int Wld_GetMobState(NpcInstance npc, string scheme)
        {
            return NpcHelper.ExtWldGetMobState(npc, scheme);
        }

        public static void Wld_InsertItem(int itemInstance, string spawnpoint)
        {
            VobHelper.ExtWldInsertItem(itemInstance, spawnpoint);
        }

        #endregion

        #region Misc

        
        public static string ConcatStrings(string str1, string str2)
        {
            return str1 + str2;
        }

        
        public static string IntToString(int x)
        {
            return x.ToString();
        }

        
        public static string FloatToString(float x)
        {
            return x.ToString(CultureInfo.InvariantCulture);
        }

        
        public static int FloatToInt(float x)
        {
            return (int)x;
        }

        
        public static float IntToFloat(int x)
        {
            return x;
        }

        #endregion
    }
}
