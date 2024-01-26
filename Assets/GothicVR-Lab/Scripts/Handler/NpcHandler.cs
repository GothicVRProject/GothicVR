using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Globals;
using GVR.Properties;
using GVR.Vm;
using TMPro;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Mesh = UnityEngine.Mesh;
using Vector3 = UnityEngine.Vector3;

namespace GVR.Lab.Handler
{
    public class NpcHandler : MonoBehaviour, IHandler
    {
        public TMP_Dropdown animationsDropdown;
        public GameObject bloodwynSlotGo;
        public BloodwynInstanceId bloodwynInstanceId;
        
        public enum BloodwynInstanceId
        {
            Deu = 6596
        }

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

            var npcSymbol = GameData.GothicVm.GetSymbolByIndex((int)bloodwynInstanceId);
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
                var heroGo = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
                heroGo.SetParent(bloodwynSlotGo);
                var heroInstance = GameData.GothicVm.InitInstance<NpcInstance>("hero");
                LookupCache.NpcCache[heroInstance.Index] = properties;
                GameData.GothicVm.GlobalOther = heroInstance;
            }

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

            MeshCreatorFacade.CreateNpc(newNpc.name, mdmName, mdhName, body, newNpc);
        }

        public void AnimationStartClick()
        {
            VmGothicExternals.AI_PlayAni(bloodwynInstance, animationsDropdown.options[animationsDropdown.value].text);
        }
    }
}
