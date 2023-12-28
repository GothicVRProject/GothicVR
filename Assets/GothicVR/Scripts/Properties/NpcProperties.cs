using System;
using System.Collections.Generic;
using GVR.Npc.Actions.AnimationActions;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob.WayNet;
using PxCs.Data.Vm;
using UnityEngine;
using UnityEngine.Serialization;
using ZenKit.Daedalus;

namespace GVR.Properties
{
    public class NpcProperties : AbstractProperties
    {
        public NpcInstance npcInstance;
            
            
        public AudioSource npcSound;
        public GameObject rootMotionGo;

        public FreePoint currentFreePoint;
        public WayPoint currentWayPoint;
            
        // Visual
        public string mdmName;
        public string baseMdsName;
        public string baseMdhName => baseMdsName;
        public string overlayMdsName;
        public string overlayMdhName => overlayMdsName;
        
        public List<ItemInstance> EquippedItems = new();
        public VmGothicExternals.ExtSetVisualBodyData BodyData;
        
        // Perceptions
        public Dictionary<VmGothicEnums.PerceptionType, int> Perceptions = new();
        public float perceptionTime;
        
        // NPC items/talents/...
        public Dictionary<VmGothicEnums.Talent, int> Talents = new();
        public Dictionary<uint, int> Items = new(); // itemId => amount

        
        public readonly Queue<AbstractAnimationAction> AnimationQueue = new();
        public VmGothicEnums.WalkMode walkMode;
        
        // HINT: These information aren't set within Daedalus. We need to define them manually.
        // HINT: i.e. every animation might have a BS. E.g. when AI_TakeItem() is called, we set BS.BS_TAKEITEM
        public VmGothicEnums.BodyState bodyState;
        
        public uint prevStateStart;
        public int stateStart;
        public int stateLoop;
        public int stateEnd;

        // State time is activated within AI_StartState()
        // e.g. used to handle random wait loops for idle eating animations (eat a cheese only every n-m seconds)
        public bool isStateTimeActive;
        public float stateTime;
        
        public LoopState currentLoopState = LoopState.None;
        public AbstractAnimationAction currentAction;

        public bool hasItemEquipped;
        public uint currentItem;
        public string usedItemSlot;
        public int itemAnimationState = -1; // We need to start with an "invalid" value as >0< is an allowed state value like in >t_Potion_Stand_2_S0<

        public enum LoopState
        {
            None,
            Start,
            Loop,
            End
        }
        
#pragma warning disable CS0414 // Just a debug flag for easier debugging if we missed to copy something in the future. 
        public bool isClonedFromAnother;
#pragma warning restore CS0414
        public void Copy(NpcProperties other)
        {
            isClonedFromAnother = true;
            npcInstance = other.npcInstance;

            mdmName = other.mdmName;
            baseMdsName = other.baseMdsName;
            overlayMdsName = other.overlayMdsName;
            BodyData = other.BodyData;
            Perceptions = other.Perceptions;
            perceptionTime = other.perceptionTime;
        }
    }
}
