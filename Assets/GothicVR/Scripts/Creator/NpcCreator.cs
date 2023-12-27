using System;
using System.Linq;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Manager;
using GVR.Npc;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Properties;
using GVR.Vob.WayNet;
using PxCs.Data.Vm;
using PxCs.Interface;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Object = UnityEngine.Object;
using WayPoint = GVR.Vob.WayNet.WayPoint;

namespace GVR.Creator
{
    public static class NpcCreator
    {
        private static GameObject npcRootGo;

        // Hint - If this scale ratio isn't looking well, feel free to change it.
        private const float fatnessScale = 0.1f;

        private static GameObject GetRootGo()
        {
            // GO need to be created after world is loaded. Otherwise we will spawn NPCs inside Bootstrap.unity
            if (npcRootGo != null)
                return npcRootGo;
            
            npcRootGo = new GameObject("NPCs");
            
            return npcRootGo;
        }

        private static GameObject GetNpc(IntPtr npcPtr)
        {
            return GetProperties(npcPtr).gameObject;
        }
        
        private static NpcProperties GetProperties(IntPtr npcPtr)
        {
            var symbolIndex = PxVm.pxVmInstanceGetSymbolIndex(npcPtr);
            var props = LookupCache.NpcCache[symbolIndex];

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
        private static GameObject GetNpcGo(IntPtr npcPtr)
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
        public static void ExtWldInsertNpc(int npcInstance, string spawnPoint)
        {
            var newNpc = PrefabCache.TryGetObject(PrefabCache.PrefabType.Npc);
            var props = newNpc.GetComponent<NpcProperties>();

            var npcSymbol = GameData.GothicVm.GetSymbolByIndex((uint)npcInstance);
            
            if (npcSymbol == null)
            {
                Debug.LogError($"Npc with ID {npcInstance} not found.");
                return;
            }
            
            // Humans are singletons.
            if (LookupCache.NpcCache.TryAdd((uint)npcInstance, newNpc.GetComponent<NpcProperties>()))
            {
                props.npcInstance = GameData.GothicVm.InitInstance<NpcInstance>(npcSymbol);
                
                var pxNpc = PxVm.InitializeNpc(GameData.VmGothicPtr, (uint)npcInstance);
                props.npc = pxNpc;
            }
            // Monsters are used multiple times.
            else
            {
                var origNpc = LookupCache.NpcCache[(uint)npcInstance];
                var origProps = origNpc.GetComponent<NpcProperties>();
                // Clone Properties as they're required from the first instance.
                props.Copy(origProps);
            }

            if (FeatureFlags.I.npcToSpawn.Any() && !FeatureFlags.I.npcToSpawn.Contains(props.npc.id))
            {
                Object.Destroy(newNpc);
                return;
            }

            newNpc.name = props.npc!.names[0];
            
            var mdhName = string.IsNullOrEmpty(props.overlayMdhName) ? props.baseMdhName : props.overlayMdhName;
            MeshObjectCreator.CreateNpc(newNpc.name, props.mdmName, mdhName, props.BodyData, newNpc);
            newNpc.SetParent(GetRootGo());

            foreach (var equippedItem in props.EquippedItems)
                MeshObjectCreator.EquipNpcWeapon(newNpc, equippedItem, equippedItem.mainFlag, equippedItem.flags);
            
            SetSpawnPoint(newNpc, spawnPoint, props.npc);

            if (FeatureFlags.I.enableNpcRoutines)
                StartRoutine(newNpc);
        }

        private static void SetSpawnPoint(GameObject npcGo, string spawnPoint, PxVmNpcData pxNpc)
        {
            var npcRoutine = pxNpc.routine;
            PxVm.CallFunction(GameData.VmGothicPtr, (uint)npcRoutine, pxNpc.instancePtr);
            
            WayNetPoint initialSpawnPoint;
            if (npcGo.GetComponent<Routine>().routines.Any())
            {
                var routineSpawnPointName = npcGo.GetComponent<Routine>().routines.First().waypoint;
                initialSpawnPoint = WayNetHelper.GetWayNetPoint(routineSpawnPointName);
            }
            else
            {
                initialSpawnPoint = WayNetHelper.GetWayNetPoint(spawnPoint);
            }

            if (initialSpawnPoint == null)
            {
                Debug.LogWarning(string.Format("spawnpoint={0} couldn't be found.", spawnPoint));
                return;
            }
            
            npcGo.transform.position = initialSpawnPoint.Position;

            if (initialSpawnPoint.GetType() == typeof(WayPoint))
                npcGo.GetComponent<NpcProperties>().currentWayPoint = (WayPoint)initialSpawnPoint;
            else
                npcGo.GetComponent<NpcProperties>().currentFreePoint = (FreePoint)initialSpawnPoint;
            
        }
        
        public static void ExtTaMin(VmGothicExternals.ExtTaMinData data)
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
            GameData.npcRoutines.TryAdd(data.Npc, new());
            GameData.npcRoutines[data.Npc].Add(routine);
        }
        
        public static void ExtMdlSetVisual(IntPtr npcPtr, string visual)
        {
            var props = GetProperties(npcPtr);

            props.baseMdsName = visual;
        }

        public static void ExtApplyOverlayMds(IntPtr npcPtr, string overlayName)
        {
            var props = GetProperties(npcPtr);

            props.overlayMdsName = overlayName;
        }

        public static void ExtSetVisualBody(VmGothicExternals.ExtSetVisualBodyData data)
        {
            var props = GetProperties(data.NpcPtr);

            props.BodyData = data;
            
            if (data.Armor >= 0)
            {
                var armorData = AssetCache.TryGetItemData((uint)data.Armor);
                props.EquippedItems.Add(AssetCache.TryGetItemData((uint)data.Armor));
                props.mdmName = armorData.visualChange;
            }
            else
            {
                props.mdmName = data.Body;
            }
        }

        public static void ExtMdlSetModelScale(IntPtr npcPtr, Vector3 scale)
        {
            var npc = GetNpcGo(npcPtr);

            // FIXME - If fatness is applied before, we reset it here. We need to do proper Vector multiplication here.
            npc.transform.localScale = scale;
        }

        public static void ExtSetModelFatness(IntPtr npcPtr, float fatness)
        {
            var npc = GetNpcGo(npcPtr);
            var oldScale = npc.transform.localScale;
            var bonusFat = fatness * fatnessScale;
            
            npc.transform.localScale = new(oldScale.x + bonusFat, oldScale.y, oldScale.z + bonusFat);
        }

        public static IntPtr ExtHlpGetNpc(int instanceId)
        {
            if (!LookupCache.NpcCache.TryGetValue((uint)instanceId, out var properties))
            {
                Debug.LogError($"Couldn't find NPC {instanceId} inside cache.");
                return IntPtr.Zero;
            }

            return properties.npcPtr;
        }

        public static void ExtNpcPerceptionEnable(IntPtr npcPtr, VmGothicEnums.PerceptionType perception, int function)
        {
            var props = GetProperties(npcPtr);
            props.Perceptions[perception] = function;
        }

        public static void ExtNpcSetPerceptionTime(IntPtr npcPtr, float time)
        {
            var props = GetProperties(npcPtr);
            props.perceptionTime = time;
        }

        public static void ExtNpcSetTalentValue(IntPtr npcPtr, VmGothicEnums.Talent talent, int level)
        {
            var props = GetProperties(npcPtr);
            props.Talents[talent] = level;
        }

        public static void ExtCreateInvItems(IntPtr npcPtr, uint itemId, int amount)
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
        
        public static void ExtEquipItem(IntPtr npcPtr, int itemId)
        {
            var props = GetProperties(npcPtr);
            var itemData = AssetCache.TryGetItemData((uint)itemId);

            props.EquippedItems.Add(itemData);
        }
        
        private static void StartRoutine(GameObject npc)
        {
            var routineComp = npc.GetComponent<Routine>();
            var firstRoutine = routineComp.routines.First();

            npc.GetComponent<AiHandler>().StartRoutine((uint)firstRoutine.action, firstRoutine.waypoint);
        }
    }
}
