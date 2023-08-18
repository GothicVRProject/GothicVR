using System;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
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

        // Hint - If this scale ratio isn't looking well, feel free to change it.
        private const float fatnessScale = 0.1f;

        void Start()
        {
            lookupCache = LookupCache.I;
            assetCache = AssetCache.I;
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
        public void ExtWldInsertNpc(int npcInstance, string spawnpoint)
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

        public void ExtTaMin(VmGothicExternals.ExtTaMinData data)
        {
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = data.StopH == 24 ? 0 : data.StopH;

            RoutineData routine = new()
            {
                start_h = data.StartH,
                start_m = data.StartM,
                start = new(1, 1, 1, data.StartH, data.StartM, 0),
                stop_h = data.StopH,
                stop_m = data.StopM,
                stop = new(1, 1, 1, stop_hFormatted, data.StopM, 0),
                action = data.Action,
                waypoint = data.Waypoint
            };

            var npcId = PxVm.pxVmInstanceGetSymbolIndex(data.Npc);
            LookupCache.I.npcCache[npcId].GetComponent<Routine>().routines.Add(routine);
            // Add element if key not yet exists.
            GameData.I.npcRoutines.TryAdd(data.Npc, new());
            GameData.I.npcRoutines[data.Npc].Add(routine);
        }

        public void ExtMdlSetVisual(IntPtr npcPtr, string visual)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            var mds = assetCache.TryGetMds(visual);

            props.baseMdsName = visual;
            props.baseMds = mds;

            // This is something used from OpenGothic. But what is it doing actually? ;-)
            if (mds.skeleton!.disableMesh)
            {
                var mdh = assetCache.TryGetMdh(visual);
                props.baseMdh = mdh;
            }
            else
            {
                throw new Exception("Not (yet) implemented");
            }
        }

        public void ExtApplyOverlayMds(IntPtr npcPtr, string overlayName)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            props.overlayMdsName = overlayName;
            props.overlayMds = assetCache.TryGetMds(overlayName);
            props.overlayMdh = assetCache.TryGetMdh(overlayName);
        }

        public void ExtSetVisualBody(VmGothicExternals.ExtSetVisualBodyData data)
        {
            var npc = GetNpcGo(data.NpcPtr);
            var props = npc.GetComponent<Properties>();
            var mmb = assetCache.TryGetMmb(data.Head);
            var name = PxVm.pxVmInstanceNpcGetName(data.NpcPtr, 0).MarshalAsString();

            var mdh = props.overlayMdh ?? props.baseMdh;
            
            PxModelMeshData mdm;
            if (FeatureFlags.I.CreateNpcArmor && data.Armor >= 0)
            {
                var armorData = assetCache.TryGetItemData((uint)data.Armor);
                mdm = assetCache.TryGetMdm(armorData.visualChange);
            }
            else
            {
                mdm = assetCache.TryGetMdm(data.Body);
            }
            
            NpcMeshCreator.I.CreateNpc(name, mdm, mdh, mmb, data, npc);
        }

        public void ExtMdlSetModelScale(IntPtr npcPtr, Vector3 scale)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var npc = lookupCache.npcCache[symbolIndex];

            // FIXME - If fatness is applied before, we reset it here. We need to do proper Vector multiplication here.
            npc.transform.localScale = scale;
        }

        public void ExtSetModelFatness(IntPtr npcPtr, float fatness)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var npc = lookupCache.npcCache[symbolIndex];
            var oldScale = npc.transform.localScale;
            var bonusFat = fatness * fatnessScale;
            
            npc.transform.localScale = new(oldScale.x + bonusFat, oldScale.y, oldScale.z + bonusFat);
        }

        public void ExtNpcSetTalentSkill(IntPtr npcPtr, VmGothicEnums.Talent talent, int level)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            props.Talents[talent] = level;
        }
        
        public void ExtEquipItem(IntPtr npcPtr, int itemId)
        {
            var npc = GetNpcGo(npcPtr);
            var itemData = assetCache.TryGetItemData((uint)itemId);

            npc.GetComponent<Properties>().EquippedItem = itemData;
            
            NpcMeshCreator.I.EquipWeapon(npc, itemData, itemData.mainFlag, itemData.flags);
        }


        public void DebugAddIdleAnimationToAllNpc()
        {
            foreach (var npcGo in lookupCache.npcCache.Values)
            {
                var mdsName = npcGo.GetComponent<Properties>().baseMdsName;
                var mdh = npcGo.GetComponent<Properties>().baseMdh;

                var animationName = mdsName.ToLower() == "humans.mds" ? "T_1HSFREE" : "S_DANCE1";
                
                AnimationCreator.I.PlayAnimation(mdsName, animationName, mdh, npcGo);
                
            }
        }
    }
}