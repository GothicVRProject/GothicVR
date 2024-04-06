using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes.V2;
using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR_Lab.Scripts.AnimationActionMocks;
using GVR.Npc.Actions;
using GVR.Npc.Actions.AnimationActions;
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

        private Dictionary<string, List<(Type, AnimationAction)>> animations = new()
        {
            {"Eat Apple", new()
            {
                (typeof(LabCreateInventoryItemAction), new(string0: "ItFoApple") ),
                (typeof(LabUseItemToState), new(string0: "ItFoApple", int1: 0)), // int0 needs to be calculated live
                (typeof(Wait), new(float0: 1)),
                (typeof(PlayAni), new(string0: "T_FOOD_RANDOM_1")),
                (typeof(Wait), new(float0: 1)),
                (typeof(PlayAni), new(string0: "T_FOOD_RANDOM_1")),
                (typeof(Wait), new(float0: 1)),
                (typeof(LabUseItemToState), new(string0: "ItFoApple", int1: -1))
            }},

        };

        public void Bootstrap()
        {
            npcDropdown.options = npcs.Keys.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
            animationDropdown.options = animations.Keys.Select(item => new TMP_Dropdown.OptionData(item)).ToList();
        }

        /// <summary>
        /// We need to prepare the NPC to load. i.e. set some NpcProperties to work properly.
        /// </summary>
        public void LoadNpcClicked()
        {
            var npcInstanceName = npcDropdown.options[npcDropdown.value].text;
            var npcData = npcs[npcInstanceName];

            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            newNpc.SetParent(npcSlotGo);
            newNpc.name = npcData.Name;

            var npcSymbol = GameData.GothicVm.GetSymbolByName(npcInstanceName);

            var npcInstance = GameData.GothicVm.AllocInstance<NpcInstance>(npcSymbol!);
            GameData.GothicVm.InitInstance(npcInstance);

            var npcProps = newNpc.GetComponent<NpcProperties>();

            npcProps.npcInstance = npcInstance;
            npcProps.baseMdsName = "Humans.mds";
            npcProps.overlayMdsName = npcData.Mdh;

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
            var animationList = animations[animationDropdown.options[animationDropdown.value].text];

            var npcGo = npcSlotGo.transform.GetChild(0).gameObject;
            var props = npcGo.GetComponent<NpcProperties>();

            foreach (var anim in animationList)
            {
                var action = (AbstractAnimationAction)Activator.CreateInstance(anim.Item1, anim.Item2, npcGo);
                props.AnimationQueue.Enqueue(action);
            }
        }
    }
}
