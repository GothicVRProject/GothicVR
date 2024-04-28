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
    public class LabNpcDialogHandler : MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown animationsDropdown;
        public GameObject bloodwynSlotGo;
        private string bloodwynInstanceId = "GRD_233_Bloodwyn";
        

        private NpcInstance bloodwynInstance;
        private string[] animations = {
            "T_LGUARD_2_STAND", "T_STAND_2_LGUARD", "T_LGUARD_SCRATCH", "T_LGUARD_STRETCH", "T_LGUARD_CHANGELEG",
            "T_HGUARD_2_STAND", "T_STAND_2_HGUARD", "T_HGUARD_LOOKAROUND"
        };

        public void Bootstrap()
        {
            animationsDropdown.options = animations.Select(item => new TMP_Dropdown.OptionData(item)).ToList();

            BootstrapBloodwyn();
        }
        

        private void BootstrapBloodwyn()
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(bloodwynSlotGo);

            var npcSymbol = GameData.GothicVm.GetSymbolByName(bloodwynInstanceId);
            bloodwynInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            var properties = newNpc.GetComponent<NpcProperties>();
            LookupCache.NpcCache[bloodwynInstance.Index] = properties;
            properties.npcInstance = bloodwynInstance;

           GameData.GothicVm.InitInstance(bloodwynInstance);
            
            properties.Dialogs = GameData.Dialogs.Instances
                .Where(dialog => dialog.Npc == bloodwynInstance.Index)
                .OrderByDescending(dialog => dialog.Important)
                .ToList();
            newNpc.name = bloodwynInstance.GetName(NpcNameSlot.Slot0);
            GameData.GothicVm.GlobalSelf = bloodwynInstance;

            // Hero
            {
                // Need to be set for later usage (e.g. Bloodwyn checks your inventory if enough nuggets are carried)
                var heroInstance = GameData.GothicVm.InitInstance<NpcInstance>("hero");
                GameData.GothicVm.GlobalHero = heroInstance;
            }

            var mdmName = "Hum_GRDM_ARMOR.asc";
            var mdhName = "Humans_Militia.mds";
            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                Armor = -1,
                Body = "hum_body_Naked0",
                BodyTexColor = 1,
                BodyTexNr = 0,
                Head = "Hum_Head_Bald",
                HeadTexNr = 18,
                TeethTexNr = 1
            };

            MeshFactory.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);
        }

        public void AnimationStartClick()
        {
            VmGothicExternals.AI_PlayAni(bloodwynInstance, animationsDropdown.options[animationsDropdown.value].text);
        }
    }
}
