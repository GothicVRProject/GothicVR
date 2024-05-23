using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes.V2;
using GVR.Extensions;
using GVR.Globals;
using GVR.Lab.AnimationActionMocks;
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


        private Dictionary<string, (string Name, string MdhMds, string Mdm, int BodyTexNr, int BodyTexColor, string Head, int HeadTexNr, int TeethTexNr, string sword)> npcs = new()
        {
            {"GRD_233_Bloodwyn", (Name: "Bloodwyn", MdhMds: "Humans_Militia.mds", Mdm: "Hum_GRDM_ARMOR", BodyTexNr: 0, BodyTexColor: 1, Head: "Hum_Head_Bald", HeadTexNr: 18, TeethTexNr: 1, sword: "ItMw_1H_Sword_04")},
            {"EBR_110_Seraphia", (Name: "Seraphia", MdhMds: "Babe.mds", Mdm: "Bab_body_Naked0", BodyTexNr: 2, BodyTexColor: 1, Head: "Bab_Head_Hair1", HeadTexNr: 2, TeethTexNr: 0, sword: null)},
            {"VLK_554_Buddler", (Name: "Buddler", MdhMds: "Humans_Tired.mds", Mdm: "Hum_VLKL_ARMOR", BodyTexNr: 3, BodyTexColor: 1, Head: "Hum_Head_Pony", HeadTexNr: 0, TeethTexNr: 2, sword: null)}
        };

        private Dictionary<string, List<(Type, AnimationAction)>> animations = new()
        {
            {
                "Human - Wash self", new()
                {
                    (typeof(PlayAni), new(string0: "T_STAND_2_WASH")),
                    (typeof(Wait), new(float0: 5)),
                    (typeof(PlayAni), new(string0: "T_WASH_2_STAND")),
                }
            },
            {
                "Human - Sword training", new()
                {
                    (typeof(DrawWeapon), new()),
                    (typeof(PlayAni), new(string0: "T_1HSFREE"))
                }
            },
            {"Human - Eat Apple", new()
                {
                    (typeof(LabCreateInventoryItem), new(string0: "ItFoApple") ),
                    (typeof(LabUseItemToState), new(string0: "ItFoApple", int1: 0)), // int0 needs to be calculated live
                    (typeof(Wait), new(float0: 1)),
                    (typeof(PlayAni), new(string0: "T_FOOD_RANDOM_1")),
                    (typeof(Wait), new(float0: 1)),
                    (typeof(PlayAni), new(string0: "T_FOOD_RANDOM_2")),
                    (typeof(Wait), new(float0: 1)),
                    (typeof(LabUseItemToState), new(string0: "ItFoApple", int1: -1))
                }
            },
            {"Human - Drink Beer", new()
                {
                    (typeof(LabCreateInventoryItem), new(string0: "ItFoBeer") ),
                    (typeof(LabUseItemToState), new(string0: "ItFoBeer", int1: 0)), // int0 needs to be calculated live
                    (typeof(Wait), new(float0: 1)),
                    (typeof(PlayAni), new(string0: "T_POTION_RANDOM_1")),
                    (typeof(Wait), new(float0: 1)),
                    (typeof(PlayAni), new(string0: "T_POTION_RANDOM_3")),
                    (typeof(Wait), new(float0: 1)),
                    (typeof(LabUseItemToState), new(string0: "ItFoBeer", int1: -1))
                }
            },
            {"Babe - Sweep", new()
                {
                    (typeof(LabCreateInventoryItem), new(string0: "ItMiBrush")), // int0 needs to be calculated live
                    (typeof(LabUseItemToState), new(string0: "ItMiBrush", int1: 1)),
                    (typeof(Wait), new(float0: 1)),
                    (typeof(LabUseItemToState), new(string0: "ItMiBrush", int1: -1))
                }
            },
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
            var npcProps = newNpc.GetComponent<NpcProperties>();

            npcProps.npcInstance = npcInstance;
            LookupCache.NpcCache[npcInstance.Index] = npcProps;
            
            GameData.GothicVm.InitInstance(npcInstance);

            npcProps.npcInstance = npcInstance;
            npcProps.overlayMdsName = npcData.MdhMds;
            
            var body = new VmGothicExternals.ExtSetVisualBodyData()
            {
                BodyTexNr = npcData.BodyTexNr,
                BodyTexColor = npcData.BodyTexColor,
                Head = npcData.Head,
                HeadTexNr = npcData.HeadTexNr,
                TeethTexNr = npcData.TeethTexNr,
                
                Body = "", // We set the armor via Mdm file manually
                Armor = -1 // We set the armor via Mdm file manually
            };

            MeshFactory.CreateNpc(newNpc.name, npcData.Mdm, npcData.MdhMds, body, newNpc);

            if (npcData.sword != null)
            {
                var swordIndex = GameData.GothicVm.GetSymbolByName(npcData.sword)!.Index;
                var sword = AssetCache.TryGetItemData(swordIndex);

                MeshFactory.CreateNpcWeapon(newNpc, sword, (VmGothicEnums.ItemFlags)sword.MainFlag, (VmGothicEnums.ItemFlags)sword.Flags);
            }
        }

        public void LoadAnimationClicked()
        {
            // Shortcut
            if (npcSlotGo.transform.childCount == 0)
                LoadNpcClicked();

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
