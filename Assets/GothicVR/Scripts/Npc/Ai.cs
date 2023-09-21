using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Creator;
using Unity.VisualScripting;
using UnityEngine;

namespace GVR.Npc
{
    public class Ai : MonoBehaviour, IAnimationCallbackEnd
    {
        public readonly Queue<Action> Queue = new();

        private bool isPlayingAnimation;


        private void Update()
        {
            if (Queue.Count == 0 || isPlayingAnimation)
                return;

            PlayNextAnimation(Queue.Dequeue());
        }

        private void PlayNextAnimation(Action action)
        {
            var props = GetComponent<Properties>();
            
            switch (action.type)
            {
                case Action.Type.AIPlayAnim:
                    var mdh = AssetCache.I.TryGetMdh(props.overlayMdhName);
                    // FIXME - We need to handle both mds and mdh options! (base vs overlay)
                    AnimationCreator.I.PlayAnimation(props.baseMdsName, action.data, mdh, gameObject);
                    isPlayingAnimation = true;
                    break;
                default:
                    break;
            }
        }
        
        
        
        
        public class Action
            {
                public enum Type
                {
                    AINone,
                    AILookAtNpc,
                    AIStopLookAt,
                    AIRemoveWeapon,
                    AITurnToNpc,
                    AIGoToNpc,
                    AIGoToNextFp,
                    AIGoToFP,
                    AIGoToWP,
                    AIStartState,
                    AIPlayAnim,
                    AIPlayAnimBs,
                    AIWait,
                    AIStandUp,
                    AIStandUpQuick,
                    AIEquipArmor,
                    AIEquipBestArmor,
                    AIEquipMelee,
                    AIEquipRange,
                    AIUseMob,
                    AIUseItem,
                    AIUseItemToState,
                    AITeleport,
                    AIDrawWeaponMelee,
                    AIDrawWeaponRange,
                    AIDrawSpell,
                    AIAttack,
                    AIFlee,
                    AIDodge,
                    AIUnEquipWeapons,
                    AIUnEquipArmor,
                    AIOutput,
                    AIOutputSvm,
                    AIOutputSvmOverlay,
                    AIProcessInfo,
                    AIStopProcessInfo,
                    AIContinueRoutine,
                    AIAlignToFp,
                    AIAlignToWp,
                    AISetNpcsToState,
                    AISetWalkMode,
                    AIFinishingMove,
                    AIDrawWeapon,
                    AITakeItem,
                    AIGotoItem,
                    AIPointAtNpc,
                    AIPointAt,
                    AIStopPointAt,
                    AIPrintScreen,
                    AILookAt
                }
        
                public Action(Type type, string data = null)
                {
                    this.type = type;
                    this.data = data;
                }
                
                public Type type;
                public string data;
            }

        public void AnimationEndCallback(string name)
        {
            isPlayingAnimation = false;
        }
    }
}