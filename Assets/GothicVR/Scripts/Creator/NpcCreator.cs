using PxCs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using Unity.VisualScripting.Dependencies.Sqlite;
using System;

namespace GVR.Creator
{
    public class NpcCreator : SingletonBehaviour<NpcCreator>
    {
        void Start()
        {
            VmGothicBridge.PhoenixWld_InsertNpc.AddListener(Wld_InsertNpc);
            VmGothicBridge.PhoenixTA_MIN.AddListener(TA_MIN);
            VmGothicBridge.PhoenixMdl_SetVisual.AddListener(Mdl_SetVisual);
            VmGothicBridge.PhoenixMdl_ApplyOverlayMds.AddListener(Mdl_ApplyOverlayMds);
            VmGothicBridge.PhoenixMdl_SetVisualBody.AddListener(Mdl_SetVisualBody);
        }

        /// <summary>
        /// Original Gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore execute the daily routines to collect current location and use this as spawn location.
        /// </summary>
        public static void Wld_InsertNpc(int npcInstance, string spawnpoint)
        {
            var npcContainer = GameObject.Find("NPCs");

            var initialSpawnpoint = PhoenixBridge.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            if (initialSpawnpoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var pxNpc = PxVm.InitializeNpc(PhoenixBridge.VmGothicPtr, (uint)npcInstance);

            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            newNpc.name = string.Format("{0}-{1}", string.Concat(pxNpc.names), spawnpoint);
            var npcRoutine = pxNpc.routine;

            PxVm.CallFunction(PhoenixBridge.VmGothicPtr, (uint)npcRoutine, pxNpc.npcPtr);

            newNpc.GetComponent<Properties>().npc = pxNpc;

            if (PhoenixBridge.npcRoutines.TryGetValue(pxNpc.npcPtr, out List<RoutineData> routines))
            {
                initialSpawnpoint = PhoenixBridge.World.waypoints
                    .FirstOrDefault(item => item.name.ToLower() == routines.First().waypoint.ToLower());
                newNpc.GetComponent<Routine>().routines = routines;
            }

            newNpc.transform.position = initialSpawnpoint.position.ToUnityVector();
            newNpc.transform.parent = npcContainer.transform;
        }

        private static void TA_MIN(VmGothicBridge.TA_MINData data)
        {
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = data.stop_h == 24 ? 0 : data.stop_h;

            RoutineData routine = new()
            {
                start_h = data.start_h,
                start_m = data.start_m,
                start = new(1, 1, 1, data.start_h, data.start_m, 0),
                stop_h = data.stop_h,
                stop_m = data.stop_m,
                stop = new(1, 1, 1, stop_hFormatted, data.stop_m, 0),
                action = data.action,
                waypoint = data.waypoint
            };

            // Add element if key not yet exists.
            PhoenixBridge.npcRoutines.TryAdd(data.npc, new());
            PhoenixBridge.npcRoutines[data.npc].Add(routine);
        }


        private static void Mdl_SetVisual(VmGothicBridge.Mdl_SetVisualData data)
        {
            // Example: visualname = HUMANS.MDS
            // FIXME - add cache to ModelScript (E.g. HUMANS.MDS are called multiple times)

            var modelScript = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, data.visual);
            var skeletonName = modelScript.skeleton.name.Replace(".ASC", ".MDM");

            var mrm = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, skeletonName);

            CreateNpcMesh(mrm);
        }

        private static void Mdl_ApplyOverlayMds(VmGothicBridge.Mdl_ApplyOverlayMdsData data)
        {

        }

        private static void Mdl_SetVisualBody(VmGothicBridge.Mdl_SetVisualBodyData data)
        {
            // TBD
        }


        // FIXME - Logic is copy&pasted from VobCreator.cs - We need to create a proper class where we can reuse functionality
        // FIXME - instead of duplicating it!

        private static void CreateNpcMesh(PxMultiResolutionMeshData mrm)
        {
            if (mrm.subMeshes.Length != 1)
                throw new ArgumentOutOfRangeException("Only 1 submesh is implemented for NPC creation as of now.");

            SingletonBehaviour<MultiResolutionMeshCreator>.GetOrCreate().Create(mrm, null, "foobar", Vector3.zero, new PxCs.Data.Misc.PxMatrix3x3Data());

        }
    }
}
