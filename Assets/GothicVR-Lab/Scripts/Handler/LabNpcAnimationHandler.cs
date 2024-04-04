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

        private string[] npcNames =
        {
            "GRD_233_Bloodwyn", "VLK_554_Buddler"
        };

        private string[] animationNames =
        {
            "T_LGUARD_2_STAND", "T_STAND_2_LGUARD", "T_LGUARD_SCRATCH", "T_LGUARD_STRETCH", "T_LGUARD_CHANGELEG",
            "T_HGUARD_2_STAND", "T_STAND_2_HGUARD", "T_HGUARD_LOOKAROUND"
        };

        public void Bootstrap()
        {
            npcDropdown.options = npcNames.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
            animationDropdown.options = animationNames.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
        }

        public void LoadNpcClicked()
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(npcSlotGo);

            var npcInstanceName = npcDropdown.options[npcDropdown.value].text;
            var npcSymbol = GameData.GothicVm.GetSymbolByName(npcInstanceName);

            var npcInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            var npcProps = newNpc.GetComponent<NpcProperties>();

            npcProps.npcInstance = npcInstance;
            LookupCache.NpcCache[npcInstance.Index] = npcProps;

            var mdmName = "Hum_GRDM_ARMOR.asc";
            var mdhName = "Humans_Militia.mds";
            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                Armor = 3643,
                Body = "hum_body_Naked0",
                BodyTexColor = 1,
                BodyTexNr = 0,
                Head = "Hum_Head_Bald",
                HeadTexNr = 18,
                TeethTexNr = 1
            };

            MeshFactory.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);
        }

        public void LoadAnimationClicked()
        {
            var npcInstance = npcSlotGo.transform.GetChild(0).GetComponent<NpcProperties>().npcInstance;
            VmGothicExternals.AI_PlayAni(npcInstance, animationDropdown.options[animationDropdown.value].text);
        }
    }
}
