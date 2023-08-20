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

        // Hint - If this scale ratio isn't looking well, feel free to change it.
        private const float fatnessScale = 0.1f;
        private const float fplookupDistance = 20f;
        
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
        private GameObject GetNpcGo(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var npcGo = lookupCache.npcCache[symbolIndex];

            var props = npcGo.GetComponent<Properties>();

            // Workaround: When calling PxVm.InitializeNpc(), phoenix will start executing all of the INSTANCEs methods.
            // But some of them like Hlp_GetNpc() need the IntPtr before it's being returned by InitializeNpc().
            // But Phoenix gives us the Pointer via other External calls. We then set it asap.
            if (props.npcPtr == IntPtr.Zero)
                props.npcPtr = npcPtr;

            return npcGo;
        }

        private Properties GetProperties(IntPtr npcPtr)
        {
            return GetNpcGo(npcPtr).GetComponent<Properties>();
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
            newNpc.GetComponent<Properties>().npc = pxNpc;

            newNpc.name = pxNpc!.names[0];
            
            var npcRoutine = pxNpc.routine;
            PxVm.CallFunction(GameData.I.VmGothicPtr, (uint)npcRoutine, pxNpc.instancePtr);

            if (newNpc.GetComponent<Routine>().routines.Any())
            {
                var initialSpawnPointName = newNpc.GetComponent<Routine>().routines.First().waypoint;
                initialSpawnPoint = GameData.I.World.waypointsDict[initialSpawnPointName];
            }
            
            newNpc.transform.position = initialSpawnPoint.position.ToUnityVector();
            newNpc.transform!.parent = GetRootGo().transform;
        }

        public bool ExtWldIsFPAvailable(IntPtr npcPtr, string fpNamePart)
        {
            var npcGo = GetNpcGo(npcPtr);
            var props = npcGo.GetComponent<Properties>();
            var freePoints = WayNetManager.I.FindFreePointsWithName(npcGo.transform.position, fpNamePart, fplookupDistance);

            foreach (var fp in freePoints)
            {
                if (props.CurrentFreePoint == fp)
                    return true;
                if (!fp.IsLocked)
                    return true;
            }

            return false;
        }
        
        public void ExtTaMin(VmGothicExternals.ExtTaMinData data)
        {
            var npc = GetNpcGo(data.Npc);
            
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

            npc.GetComponent<Routine>().routines.Add(routine);

            // Add element if key not yet exists.
            GameData.I.npcRoutines.TryAdd(data.Npc, new());
            GameData.I.npcRoutines[data.Npc].Add(routine);
        }

        public void ExtAiStandUp(IntPtr npcPtr)
        {
            // FIXME - from docu:
            // * Ist der Nsc in einem Animatinsstate, wird die passende Rücktransition abgespielt.
            // * Benutzt der NSC gerade ein MOBSI, poppt er ins stehen.
        }
        
        public void ExtAiSetWalkMode(IntPtr npcPtr, VmGothicEnums.WalkMode walkMode)
        {
            GetProperties(npcPtr).walkMode = walkMode;
        }

        public void ExtAiGotoWP(IntPtr npcPtr, string spawnPoint)
        {
            // FIXME implement
            // FIXME - e.g. for Thorus there's initially no string value for TA_Boss() self.wp - Intended or a bug on our side?
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
            var npc = GetNpcGo(npcPtr);

            // FIXME - If fatness is applied before, we reset it here. We need to do proper Vector multiplication here.
            npc.transform.localScale = scale;
        }

        public void ExtSetModelFatness(IntPtr npcPtr, float fatness)
        {
            var npc = GetNpcGo(npcPtr);
            var oldScale = npc.transform.localScale;
            var bonusFat = fatness * fatnessScale;
            
            npc.transform.localScale = new(oldScale.x + bonusFat, oldScale.y, oldScale.z + bonusFat);
        }

        public IntPtr ExtHlpGetNpc(int instanceId)
        {
            if (!lookupCache.npcCache.TryGetValue((uint)instanceId, out GameObject npcGo))
            {
                Debug.LogError($"Couldn't find NPC {instanceId} inside cache.");
                return IntPtr.Zero;
            }

            return npcGo.GetComponent<Properties>().npcPtr;
        }

        public void ExtNpcPerceptionEnable(IntPtr npcPtr, VmGothicEnums.PerceptionType perception, int function)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            props.Perceptions[perception] = function;
        }

        public void ExtNpcSetPerceptionTime(IntPtr npcPtr, float time)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            props.perceptionTime = time;
        }

        public void ExtNpcSetTalentValue(IntPtr npcPtr, VmGothicEnums.Talent talent, int level)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            props.Talents[talent] = level;
        }

        public void ExtCreateInvItems(IntPtr npcPtr, int itemId, int amount)
        {
            var npc = GetNpcGo(npcPtr);
            var props = npc.GetComponent<Properties>();
            
            if (!props.Items.TryGetValue(itemId, out _))
            {
                props.Items.Add(itemId, amount);
            }
            else
            {
                props.Items[itemId] += amount;
            }
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

                if (npcGo.name != "Thorus")
                    continue;

                var props = npcGo.GetComponent<Properties>();
                var routineComp = npcGo.GetComponent<Routine>();
                var firstRoutine = routineComp.routines.FirstOrDefault();
                
                PxVm.CallFunction(GameData.I.VmGothicPtr, (uint)firstRoutine.action, props.npcPtr);
                

                // var animationName = mdsName.ToLower() == "humans.mds" ? "T_1HSFREE" : "S_DANCE1";
                // AnimationCreator.I.PlayAnimation(mdsName, animationName, mdh, npcGo);
            }
        }
    }
}