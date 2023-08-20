using System;
using System.Collections.Generic;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob;
using PxCs.Data.Model;
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
        public string baseMdsName;
        public PxModelScriptData baseMds;
        public PxModelHierarchyData baseMdh;

        public string overlayMdsName;
        public PxModelScriptData overlayMds;
        public PxModelHierarchyData overlayMdh;
        
        public PxVmItemData EquippedItem;
        
        // Perceptions
        public Dictionary<VmGothicEnums.PerceptionType, int> Perceptions = new();
        public float perceptionTime;
            
        // NPC items/talents/...
        public VmGothicEnums.WalkMode walkMode;
        public Dictionary<VmGothicEnums.Talent, int> Talents = new();
        public Dictionary<int, int> Items = new();
    }
}
