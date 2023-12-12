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
    
            public AnimationAction(Type actionType, string string0 = null, int int0 = 0, uint uint0 = 0, float float0 = 0f, bool bool0 = false)
            {
                this.ActionType = actionType;
                this.String0 = string0;
                this.Int0 = int0;
                this.Uint0 = uint0;
                this.Float0 = float0;
                this.Bool0 = bool0;
            }
            
            public readonly Type ActionType;
            public readonly string String0;
            public readonly int Int0;
            public readonly uint Uint0;
            public readonly float Float0;
            public readonly bool Bool0;
    }
}
