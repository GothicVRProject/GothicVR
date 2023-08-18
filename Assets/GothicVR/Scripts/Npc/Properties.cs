﻿using System;
using System.Collections.Generic;
using GVR.Phoenix.Interface.Vm;
using PxCs.Data.Model;
using PxCs.Data.Vm;
using UnityEngine;

namespace GVR.Npc
{    
    public class Properties: MonoBehaviour
    {
        public PxVmNpcData npc;
        
        // Visual
        public string baseMdsName;
        public PxModelScriptData baseMds;
        public PxModelHierarchyData baseMdh;

        public string overlayMdsName;
        public PxModelScriptData overlayMds;
        public PxModelHierarchyData overlayMdh;
        
        // Talent
        public Dictionary<VmGothicEnums.Talent, int> Talents = new();
    }
}
