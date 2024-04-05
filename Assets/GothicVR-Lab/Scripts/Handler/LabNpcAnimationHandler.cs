using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes.V2;
using GVR.Extensions;
using GVR.Globals;
using GVR.Properties;
using GVR.Vm;
using TMPro;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Lab.Handler
{
    public class LabNpcAnimationHandler : MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown npcDropdown;
        public TMP_Dropdown animationDropdown;
        public GameObject npcSlotGo;


        private Dictionary<string, (string Name, string Mdh, string Mdm, int Armor, string Body, int BodyTexColor, int BodyTexNr, string Head, int HeadTexNr, int TeethTexNr)> npcs = new()
        {
            {"GRD_233_Bloodwyn", (Name: "Bloodwyn", Mdh: "Humans_Militia.mds", Mdm: "Hum_GRDM_ARMOR", Armor: -1, Body: "hum_body_Naked0", BodyTexColor: 1, BodyTexNr: 0, Head: "Hum_Head_Bald", HeadTexNr: 18, TeethTexNr: 1)},
            {"VLK_554_Buddler", (Name: "Buddler", Mdh: "Humans_Tired.mds", Mdm: "Hum_VLKL_ARMOR", Armor: -1, Body: "hum_body_Naked0", BodyTexColor: 3, BodyTexNr: 1, Head: "Hum_Head_Pony", HeadTexNr: 0, TeethTexNr: 2)}
        };

        private string[] animationNames =
        {
            "T_LGUARD_2_STAND", "T_STAND_2_LGUARD", "T_LGUARD_SCRATCH", "T_LGUARD_STRETCH", "T_LGUARD_CHANGELEG",
            "T_HGUARD_2_STAND", "T_STAND_2_HGUARD", "T_HGUARD_LOOKAROUND"
        };

        public void Bootstrap()
        {
            npcDropdown.options = npcs.Keys.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
            animationDropdown.options = animationNames.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
        }

        public void LoadNpcClicked()
        {
            var npcInstanceName = npcDropdown.options[npcDropdown.value].text;
            var npcData = npcs[npcInstanceName];

            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(npcSlotGo);
            newNpc.name = npcData.Name;

            var npcSymbol = GameData.GothicVm.GetSymbolByName(npcInstanceName);

            var npcInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            var npcProps = newNpc.GetComponent<NpcProperties>();

            npcProps.npcInstance = npcInstance;
            LookupCache.NpcCache[npcInstance.Index] = npcProps;

            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                Armor = npcData.Armor,
                Body = npcData.Body,
                BodyTexColor = npcData.BodyTexColor,
                BodyTexNr = npcData.BodyTexNr,
                Head = npcData.Head,
                HeadTexNr = npcData.HeadTexNr,
                TeethTexNr = npcData.TeethTexNr
            };

            MeshFactory.CreateNpc(newNpc.name, npcData.Mdm, npcData.Mdh, body, newNpc);
        }

        public void LoadAnimationClicked()
        {
            var npcInstance = npcSlotGo.transform.GetChild(0).GetComponent<NpcProperties>().npcInstance;
            VmGothicExternals.AI_PlayAni(npcInstance, animationDropdown.options[animationDropdown.value].text);
        }
    }
}
