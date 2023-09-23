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
using GVR.Vob.WayNet;
using PxCs.Data.Vm;
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

        private Properties GetProperties(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = lookupCache.NpcCache[symbolIndex];

            // Workaround: When calling PxVm.InitializeNpc(), phoenix will start executing all of the INSTANCEs methods.
            // But some of them like Hlp_GetNpc() need the IntPtr before it's being returned by InitializeNpc().
            // But Phoenix gives us the Pointer via other External calls. We then set it asap.
            if (props.npcPtr == IntPtr.Zero)
                props.npcPtr = npcPtr;

            return props;
        }
        
        /// <summary>
        /// Return cached GameObject based on lookup through IntPtr
        /// </summary>
        private GameObject GetNpcGo(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).gameObject;
        }

        /// <summary>
        /// Original Gothic uses this function to spawn an NPC instance into the world.
        /// 
        /// The startpoint to walk isn't neccessarily the spawnpoint mentioned here.
        /// It can also be the currently active routine point to walk to.
        /// We therefore execute the daily routines to collect current location and use this as spawn location.
        /// </summary>
        public void ExtWldInsertNpc(int npcInstance, string spawnPoint)
        {
            var newNpc = Instantiate(Resources.Load<GameObject>("Prefabs/Npc"));
            var props = newNpc.GetComponent<Properties>();
            newNpc.SetParent(GetRootGo());
            
            // Humans are singletons.
            if (lookupCache.NpcCache.TryAdd((uint)npcInstance, newNpc.GetComponent<Properties>()))
            {
                var pxNpc = PxVm.InitializeNpc(GameData.I.VmGothicPtr, (uint)npcInstance);
                props.npc = pxNpc;
            }
            // Monsters are used multiple times.
            else
            {
                var origNpc = lookupCache.NpcCache[(uint)npcInstance];
                var origProps = origNpc.GetComponent<Properties>();
                // clone Properties as they're required from the first instance.

                // CLone values from first/original Instance.
                props.Copy(origProps);
            }

            newNpc.name = props.npc!.names[0];
         

            var mdhName = string.IsNullOrEmpty(props.overlayMdhName) ? props.baseMdhName : props.overlayMdhName;
            NpcMeshCreator.I.CreateNpc(name, props.mdmName, mdhName, props.BodyData.Head, props.BodyData, newNpc);

            foreach (var equippedItem in props.EquippedItems)
                NpcMeshCreator.I.EquipWeapon(newNpc, equippedItem, equippedItem.mainFlag, equippedItem.flags);
            
            SetSpawnPoint(newNpc, spawnPoint, props.npc);
        }

        private void SetSpawnPoint(GameObject npcGo, string spawnPoint, PxVmNpcData pxNpc)
        {
            var npcRoutine = pxNpc.routine;
            PxVm.CallFunction(GameData.I.VmGothicPtr, (uint)npcRoutine, pxNpc.instancePtr);
            
            WayNetPoint initialSpawnPoint;
            if (npcGo.GetComponent<Routine>().routines.Any())
            {
                var routineSpawnPointName = npcGo.GetComponent<Routine>().routines.First().waypoint;
                initialSpawnPoint = WayNetManager.I.GetWayNetPoint(routineSpawnPointName);
            }
            else
            {
                initialSpawnPoint = WayNetManager.I.GetWayNetPoint(spawnPoint);
            }

            if (initialSpawnPoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnPoint));
                return;
            }
            
            npcGo.transform.position = initialSpawnPoint.Position;
        }

        public bool ExtWldIsFPAvailable(IntPtr npcPtr, string fpNamePart)
        {
            var props = GetProperties(npcPtr);
            var npcGo = props.gameObject;
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
        
        public void ExtMdlSetVisual(IntPtr npcPtr, string visual)
        {
            var props = GetProperties(npcPtr);

            props.baseMdsName = visual;
        }

        public void ExtApplyOverlayMds(IntPtr npcPtr, string overlayName)
        {
            var props = GetProperties(npcPtr);

            props.overlayMdsName = overlayName;
        }

        public void ExtSetVisualBody(VmGothicExternals.ExtSetVisualBodyData data)
        {
            var props = GetProperties(data.NpcPtr);

            props.BodyData = data;
            
            if (FeatureFlags.I.CreateNpcArmor && data.Armor >= 0)
            {
                var armorData = assetCache.TryGetItemData((uint)data.Armor);
                props.mdmName = armorData.visualChange;
            }
            else
            {
                props.mdmName = data.Body;
            }
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
            if (!lookupCache.NpcCache.TryGetValue((uint)instanceId, out Properties properties))
            {
                Debug.LogError($"Couldn't find NPC {instanceId} inside cache.");
                return IntPtr.Zero;
            }

            return properties.npcPtr;
        }

        public void ExtNpcPerceptionEnable(IntPtr npcPtr, VmGothicEnums.PerceptionType perception, int function)
        {
            var props = GetProperties(npcPtr);
            props.Perceptions[perception] = function;
        }

        public void ExtNpcSetPerceptionTime(IntPtr npcPtr, float time)
        {
            var props = GetProperties(npcPtr);
            props.perceptionTime = time;
        }

        public void ExtNpcSetTalentValue(IntPtr npcPtr, VmGothicEnums.Talent talent, int level)
        {
            var props = GetProperties(npcPtr);
            props.Talents[talent] = level;
        }

        public void ExtCreateInvItems(IntPtr npcPtr, int itemId, int amount)
        {
            var props = GetProperties(npcPtr);
            
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
            var props = GetProperties(npcPtr);
            var npc = props.gameObject;
            var itemData = assetCache.TryGetItemData((uint)itemId);

            props.EquippedItems.Add(itemData);
        }
        
        public void DebugAddIdleAnimationToAllNpc()
        {
            DebugDanceAll();
            DebugThorus();
            DebugMeatBug();

        }

        private void DebugDanceAll()
        {
            var npcs = lookupCache.NpcCache.Values
                .Where(i => i.name != "Thorus")
                .Where(i => i.name != "Fleischwanze")
                .Where(i => i.name != "Meatbug");
            
            foreach (var props in npcs)
            {
                var mdsName = props.baseMdsName;
                var mdh = assetCache.TryGetMdh(props.baseMdhName);
                var animationName = mdsName.ToLower() == "humans.mds" ? "T_1HSFREE" : "S_DANCE1";
                AnimationCreator.I.PlayAnimation(mdsName, animationName, mdh, props.gameObject);
            }
        }
        
        private void DebugThorus()
        {
            foreach (var props in lookupCache.NpcCache.Values.Where(i => i.name == "Thorus"))
            {
                var routineComp = props.GetComponent<Routine>();
                var firstRoutine = routineComp.routines.FirstOrDefault();

                props.GetComponent<Ai>().StartRoutine((uint)firstRoutine.action);
            }
        }

        private void DebugMeatBug()
        {
            var x = lookupCache.NpcCache.Values.Where(i => i.name == "Fleischwanze").Count();
            
            foreach (var props in lookupCache.NpcCache.Values.Where(i => i.name == "Fleischwanze"))
            {
                var mds = assetCache.TryGetMds(props.baseMdsName);
                var mdh = assetCache.TryGetMdh(props.baseMdhName);
                
                var animNames = mds.animations.Select(i => i.name).ToArray();
                
                AnimationCreator.I.PlayAnimation(props.baseMdsName, "s_FistRunL", mdh, props.gameObject);
            }
        }
    }
}