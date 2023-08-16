using System;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Manager;
using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Model;
using PxCs.Extensions;
using PxCs.Interface;
using UnityEngine;

namespace GVR.Creator
{
	public class NpcCreator : SingletonBehaviour<NpcCreator>
    {
        private static LookupCache lookupCache;
        private static AssetCache assetCache;
        private static GameObject npcRootGo;

        void Start()
        {
            lookupCache = LookupCache.I;
            assetCache = AssetCache.I;
            
            VmGothicBridge.PhoenixWld_InsertNpc.AddListener(Wld_InsertNpc);
            VmGothicBridge.PhoenixTA_MIN.AddListener(TA_MIN);
            VmGothicBridge.PhoenixMdl_SetVisual.AddListener(Mdl_SetVisual);
            VmGothicBridge.PhoenixMdl_ApplyOverlayMds.AddListener(Mdl_ApplyOverlayMds);
            VmGothicBridge.PhoenixMdl_SetVisualBody.AddListener(Mdl_SetVisualBody);
            VmGothicBridge.PhoenixEquipItem.AddListener(EquipItem);
        }

        private static GameObject GetRootGo()
        {
            // GO need to be created after world is loaded. Otherwise we will spawn NPCs inside Bootstrap.unity
            if (npcRootGo != null)
                return npcRootGo;
            
            npcRootGo = new GameObject("NPCs");
            
            return npcRootGo;
        }

        /// <summary>
        /// Return cached GameObject based on lookup through IntPtr
        /// </summary>
        private static GameObject GetNpcGo(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            return lookupCache.npcCache[symbolIndex];
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
            var initialSpawnPoint = GameData.I.World.waypoints
                .FirstOrDefault(item => item.name.ToLower() == spawnpoint.ToLower());

            if (initialSpawnPoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnpoint));
                return;
            }

            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            lookupCache.npcCache.Add((uint)npcInstance, newNpc);

            var pxNpc = PxVm.InitializeNpc(GameData.I.VmGothicPtr, (uint)npcInstance);

            newNpc.name = pxNpc!.names[0];
            var npcRoutine = pxNpc.routine;

            PxVm.CallFunction(GameData.I.VmGothicPtr, (uint)npcRoutine, pxNpc.instancePtr);

            newNpc.GetComponent<Properties>().npc = pxNpc;

            if (newNpc.GetComponent<Routine>().routines.Any())
            {
                var initialSpawnPointName = newNpc.GetComponent<Routine>().routines.First().waypoint;
                initialSpawnPoint = GameData.I.World.waypointsDict[initialSpawnPointName];
            }
            
            newNpc.transform.position = initialSpawnPoint.position.ToUnityVector();
            newNpc.transform!.parent = GetRootGo().transform;
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

            var npcId = PxVm.pxVmInstanceGetSymbolIndex(data.npc);
            LookupCache.I.npcCache[npcId].GetComponent<Routine>().routines.Add(routine);
            // Add element if key not yet exists.
            GameData.I.npcRoutines.TryAdd(data.npc, new());
            GameData.I.npcRoutines[data.npc].Add(routine);
        }

        private static void Mdl_SetVisual(VmGothicBridge.Mdl_SetVisualData data)
        {
            var npc = GetNpcGo(data.npcPtr);
            var mds = assetCache.TryGetMds(data.visual);

            npc.GetComponent<Properties>().baseMdsName = data.visual;
            npc.GetComponent<Properties>().baseMds = mds;

            // This is something used from OpenGothic. But what is it doing actually? ;-)
            if (mds.skeleton.disableMesh)
            {
                var mdh = assetCache.TryGetMdh(data.visual);
                npc.GetComponent<Properties>().mdh = mdh;
            }
            else
            {
                throw new Exception("Not yet implemented");
                //var skeletonName = mds.skeleton.name.Replace(".ASC", ".MDM");
                //var mdm = PxModelMesh.LoadModelMeshFromVfs(GameData.I.VfsPtr, skeletonName); // --> if null
            }
        }

        private static void Mdl_ApplyOverlayMds(VmGothicBridge.Mdl_ApplyOverlayMdsData data)
        {
            var npc = GetNpcGo(data.npcPtr);
            npc.GetComponent<Properties>().overlayMdsName = data.overlayname;
            npc.GetComponent<Properties>().overlayMds = assetCache.TryGetMds(data.overlayname);
        }

        private static void Mdl_SetVisualBody(VmGothicBridge.Mdl_SetVisualBodyData data)
        {
            var npc = GetNpcGo(data.npcPtr);
            var mdh = npc.GetComponent<Properties>().mdh;
            var mmb = assetCache.TryGetMmb(data.head);
            var name = PxVm.pxVmInstanceNpcGetName(data.npcPtr, 0).MarshalAsString();
            
            PxModelMeshData mdm;
            if (FeatureFlags.I.CreateNpcArmor && data.armor >= 0)
            {
                var armorData = assetCache.TryGetItemData((uint)data.armor);
                mdm = assetCache.TryGetMdm(armorData.visualChange);
            }
            else
            {
                mdm = assetCache.TryGetMdm(data.body);
            }
            
            NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, data, npc);
        }

        private static void EquipItem(VmGothicBridge.EquipItemData data)
        {
            var npc = GetNpcGo(data.npcPtr);
            var itemData = assetCache.TryGetItemData((uint)data.itemId);
            
            NpcMeshCreator.I.EquipWeapon(npc, itemData, itemData.mainFlag, itemData.flags);
        }


        public void DebugAddIdleAnimationToAllNpc()
        {
            foreach (var npcGo in lookupCache.npcCache.Values)
            {
                var mdsName = npcGo.GetComponent<Properties>().baseMdsName;
                var mdh = npcGo.GetComponent<Properties>().mdh;

                var animationName = mdsName.ToLower() == "humans.mds" ? "T_STAND_2_LGUARD" : "S_DANCE1";
                
                AnimationCreator.I.PlayAnimation(mdsName, animationName, mdh, npcGo);
                
            }
        }
    }
}