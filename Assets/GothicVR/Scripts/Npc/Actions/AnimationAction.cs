namespace GVR.Npc.Actions
{
    public class AnimationAction
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
                AIPlayAni,
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
    
            public AnimationAction(Type actionType, string str0 = null, int i0 = 0, uint ui0 = 0, float f0 = 0f)
            {
                this.ActionType = actionType;
                this.str0 = str0;
                this.i0 = i0;
                this.ui0 = ui0;
                this.f0 = f0;
            }
            
            public readonly Type ActionType;
            public readonly string str0;
            public readonly int i0;
            public readonly uint ui0;
            public readonly float f0;
    }
}