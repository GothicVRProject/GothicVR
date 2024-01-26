using GVR.Caches;
using GVR.Creator;
using GVR.Extensions;
using UnityEngine;
using ZenKit;
using EventType = ZenKit.EventType;

namespace GVR.Npc.Actions.AnimationActions
{
    public class DrawWeapon : AbstractAnimationAction
    {
        public DrawWeapon(AnimationAction action, GameObject npcGo) : base(action, npcGo)
        { }

        public override void Start()
        {
            var mdh = AssetCache.TryGetMdh(Props.overlayMdhName);

            // FIXME - We need to handle both mds and mdh options! (base vs overlay)
            // "t_1hRun_2_1h" --> undraw animation!
            // "t_Move_2_1hMove" --> drawing
            // "t_1h_2_1hRun"
            AnimationCreator.PlayAnimation(Props.baseMdsName, "t_Move_2_1hMove", mdh, NpcGo, true);
        }

        // FIXME - 1Hand hardcoded so far. We need to get the information from inventory system itself.
        // FIXME - Sound is hardcoded as well. We need to get material from weapon dynamically of wood or metal.
        public override void AnimationEventCallback(IEventTag data)
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
                    AnimationSfxEventCallback(new CachedEventSoundEffect()
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
            var weapon1h = NpcGo.FindChildRecursively("ZS_SWORD");

            weapon1h.SetParent(rightHand, true, true);
        }
    }
}
