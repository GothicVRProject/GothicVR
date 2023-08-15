using PxCs.Data.Model;
using PxCs.Data.Vm;
using System;
using UnityEngine;

namespace GVR.Npc
{
    public class Properties: MonoBehaviour
    {
        public PxVmNpcData npc;

        public PxModelHierarchyData mdh;
        
        public string baseMdsName;
        public PxModelScriptData baseMds;

        public string overlayMdsName;
        public PxModelScriptData overlayMds;
    }
}
