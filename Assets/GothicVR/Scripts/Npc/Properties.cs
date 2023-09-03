using System;
using System.Collections.Generic;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob.WayNet;
using PxCs.Data.Model;
using PxCs.Data.Vm;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

namespace GVR.Npc
{    
    public class Properties: MonoBehaviour
    {
        public IntPtr npcPtr;
        public PxVmNpcData npc;

        public FreePoint CurrentFreePoint;
        
        // Visual
        public string mdmName;
        
        public string baseMdsName;
        public string baseMdhName => baseMdsName;

        public string overlayMdsName;
        public string overlayMdhName => overlayMdsName;
        
        public PxVmItemData EquippedItem;
        public VmGothicExternals.ExtSetVisualBodyData BodyData;
        
        // Perceptions
        public Dictionary<VmGothicEnums.PerceptionType, int> Perceptions = new();
        public float perceptionTime;
        
        // NPC items/talents/...
        public VmGothicEnums.WalkMode walkMode;
        public Dictionary<VmGothicEnums.Talent, int> Talents = new();
        public Dictionary<int, int> Items = new();


#pragma warning disable CS0414 // Just a debug flag for easier debugging if we missed to copy something in the future. 
        private bool isClonedFromAnother;
#pragma warning restore CS0414
        public void Copy(Properties other)
        {
            isClonedFromAnother = true;
            npcPtr = other.npcPtr;
            npc = other.npc;

            mdmName = other.mdmName;
            baseMdsName = other.baseMdsName;
            overlayMdsName = other.overlayMdsName;
            BodyData = other.BodyData;
            Perceptions = other.Perceptions;
            perceptionTime = other.perceptionTime;
        }
    }
}
