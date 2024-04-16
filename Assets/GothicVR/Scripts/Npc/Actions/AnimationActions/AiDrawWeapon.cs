using GVR.Creator;
using GVR.Data.ZkEvents;
using GVR.Extensions;
using UnityEngine;
using EventType = ZenKit.EventType;

namespace GVR.Npc.Actions.AnimationActions
{
    public class DrawWeapon : AbstractAnimationAction
    {
        public DrawWeapon(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            // "t_1hRun_2_1h" --> undraw animation!
            // "t_Move_2_1hMove" --> drawing
            // "t_1h_2_1hRun"
            AnimationCreator.PlayAnimation(Props.mdsNames, "t_Move_2_1hMove", NpcGo, true);
        }

        // FIXME - 1Hand hardcoded so far. We need to get the information from inventory system itself.
        // FIXME - Sound is hardcoded as well. We need to get material from weapon dynamically of wood or metal.
        public override void AnimationEventCallback(SerializableEventTag data)
        {
            switch (data.Type)
            {
                case EventType.SetFightMode:
                    SyncZSlots();
                    break;
                case EventType.SoundDraw:
                    // FIXME - Handle proper sound effect based on metal or wood weapon
                    // "DRAWSOUND_ME.WAV" --> metal
                    // "DRAWSOUND_WO.WAV" --> wood
                    AnimationSfxEventCallback(new SerializableEventSoundEffect()
                    {
                        Name = "DRAWSOUND_ME.WAV",
                        Range = 2000f
                    });
                    break;
                default:
                    base.AnimationEventCallback(data);
                    break;
            }
        }

        // FIXME - Hardcoded. We need to set it dynamically and not copying the ZS, but an object below. Otherwise it's hard to find previous parent when undrawing.
        private void SyncZSlots()
        {
            var rightHand = NpcGo.FindChildRecursively("ZS_RIGHTHAND");
            var weapon1HSlot = NpcGo.FindChildRecursively("ZS_SWORD");

            // No weapon equipped in slot.
            if (weapon1HSlot.transform.childCount == 0)
                return;

            var weaponGo = weapon1HSlot.transform.GetChild(0).gameObject;

            weaponGo.SetParent(rightHand, true, true);
        }
    }
}
