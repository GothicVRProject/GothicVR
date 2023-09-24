﻿using System;
using System.Collections.Generic;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob.WayNet;
using PxCs.Data.Vm;
using UnityEngine;

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
        
        public List<PxVmItemData> EquippedItems = new();
        public VmGothicExternals.ExtSetVisualBodyData BodyData;
        
        // Perceptions
        public Dictionary<VmGothicEnums.PerceptionType, int> Perceptions = new();
        public float perceptionTime;
        
        // NPC items/talents/...
        public Dictionary<VmGothicEnums.Talent, int> Talents = new();
        public Dictionary<int, int> Items = new();

#pragma warning disable CS0414 // Just a debug flag for easier debugging if we missed to copy something in the future. 
        public bool isClonedFromAnother;
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
