using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using GVR.Npc;
using GVR.Npc.Routines;
using GVR.Properties;
using GVR.Vm;
using GVR.Vob.WayNet;
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
        private static DaedalusVm vm => GameData.GothicVm;

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

        private static NpcProperties GetProperties(NpcInstance npc)
        {
            return LookupCache.NpcCache[npc.Index];
        }

        private static GameObject GetNpcGo(NpcInstance npcInstance)
        {
            return GetProperties(npcInstance).gameObject;
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
            var npcSymbol = vm.GetSymbolByIndex(npcInstance);
            
            if (npcSymbol == null)
            {
                Debug.LogError($"Npc with ID {npcInstance} not found.");
                return;
            }
            
            // Humans are singletons.
            if (LookupCache.NpcCache.TryAdd(npcInstance, newNpc.GetComponent<NpcProperties>()))
            {
                props.npcInstance = vm.AllocInstance<NpcInstance>(npcSymbol);
                vm.InitInstance(props.npcInstance);

                props.Dialogs = GameData.Dialogs.Instances
                    .Where(dialog => dialog.Npc == props.npcInstance.Index)
                    .OrderByDescending(dialog => dialog.Important)
                    .ToList();
            }
            // Monsters are used multiple times.
            else
            {
                var origNpc = LookupCache.NpcCache[npcInstance];
                var origProps = origNpc.GetComponent<NpcProperties>();
                // Clone Properties as they're required from the first instance.
                props.Copy(origProps);
            }

            if (FeatureFlags.I.npcToSpawn.Any() && !FeatureFlags.I.npcToSpawn.Contains(props.npcInstance.Id))
            {
                Object.Destroy(newNpc);
                LookupCache.NpcCache.Remove(props.npcInstance.Index);
                return;
            }

            newNpc.name = props.npcInstance.GetName(NpcNameSlot.Slot0);
            
            var mdhName = string.IsNullOrEmpty(props.overlayMdhName) ? props.baseMdhName : props.overlayMdhName;
            MeshObjectCreator.CreateNpc(newNpc.name, props.mdmName, mdhName, props.BodyData, newNpc);
            newNpc.SetParent(GetRootGo());

            foreach (var equippedItem in props.EquippedItems)
                MeshObjectCreator.EquipNpcWeapon(newNpc, equippedItem, (VmGothicEnums.ItemFlags)equippedItem.MainFlag, (VmGothicEnums.ItemFlags)equippedItem.Flags);
            
            var npcRoutine = props.npcInstance.DailyRoutine;

            // e.g. Monsters have no routine and therefore no further routine handling needed.
            if (npcRoutine != 0)
            {
                // We always need to set "self" before executing any Daedalus function.
                vm.GlobalSelf = props.npcInstance;
                vm.Call(npcRoutine);

                if (FeatureFlags.I.enableNpcRoutines)
                    StartRoutine(newNpc);
            }

            SetSpawnPoint(newNpc, spawnPoint);
        }

        private static void StartRoutine(GameObject npc)
        {
            var routineComp = npc.GetComponent<Routine>();
            var firstRoutine = routineComp.routines.First();

            npc.GetComponent<AiHandler>().StartRoutine(firstRoutine.action, firstRoutine.waypoint);
        }
        
        private static void SetSpawnPoint(GameObject npcGo, string spawnPoint)
        {
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
                Debug.LogWarning(string.Format("spawnPoint={0} couldn't be found.", spawnPoint));
                return;
            }
            
            npcGo.transform.position = initialSpawnPoint.Position;
            npcGo.GetComponent<NpcProperties>().CurrentWayNetPoint = initialSpawnPoint;
        }
        
        public static void ExtTaMin(NpcInstance npcInstance, int startH, int startM, int stopH, int stopM, int action, string waypoint)
        {
            var npc = GetNpcGo(npcInstance);
            
            // If we put h=24, DateTime will throw an error instead of rolling.
            var stop_hFormatted = (stopH == 24) ? 0 : stopH;

            RoutineData routine = new()
            {
                start_h = startH,
                start_m = startM,
                start = new(1, 1, 1, startH, startM, 0),
                stop_h = stopH,
                stop_m = stopM,
                stop = new(1, 1, 1, stop_hFormatted, stopM, 0),
                action = action,
                waypoint = waypoint
            };

            npc.GetComponent<Routine>().routines.Add(routine);

            // Add element if key not yet exists.
            GameData.npcRoutines.TryAdd(npcInstance.Index, new());
            GameData.npcRoutines[npcInstance.Index].Add(routine);
        }
        
        public static void ExtMdlSetVisual(NpcInstance npc, string visual)
        {
            var props = GetProperties(npc);
            props.baseMdsName = visual;
        }

        public static void ExtApplyOverlayMds(NpcInstance npc, string overlayName)
        {
            var props = GetProperties(npc);
            props.overlayMdsName = overlayName;
        }

        public static void ExtNpcSetTalentSkill(NpcInstance npc, VmGothicEnums.Talent talent, int level)
        {
            // FIXME - TBD.
            // FIXME - In OpenGothic it adds MDS overlays based on skill level.
        }

        public static void ExtSetVisualBody(VmGothicExternals.ExtSetVisualBodyData data)
        {
            var props = GetProperties(data.Npc);

            props.BodyData = data;
            
            if (data.Armor >= 0)
            {
                var armorData = AssetCache.TryGetItemData(data.Armor);
                props.EquippedItems.Add(AssetCache.TryGetItemData(data.Armor));
                props.mdmName = armorData.VisualChange;
            }
            else
            {
                props.mdmName = data.Body;
            }
        }

        public static void ExtMdlSetModelScale(NpcInstance npc, Vector3 scale)
        {
            var npcGo = GetNpcGo(npc);

            // FIXME - If fatness is applied before, we reset it here. We need to do proper Vector multiplication here.
            npcGo.transform.localScale = scale;
        }

        public static void ExtSetModelFatness(NpcInstance npc, float fatness)
        {
            var npcGo = GetNpcGo(npc);
            var oldScale = npcGo.transform.localScale;
            var bonusFat = fatness * fatnessScale;
            
            npcGo.transform.localScale = new(oldScale.x + bonusFat, oldScale.y, oldScale.z + bonusFat);
        }

        public static NpcInstance ExtHlpGetNpc(int instanceId)
        {
            if (!LookupCache.NpcCache.TryGetValue(instanceId, out var properties))
            {
                Debug.LogError($"Couldn't find NPC {instanceId} inside cache.");
                return null;
            }


            return properties.npcInstance;
        }

        public static void ExtNpcPerceptionEnable(NpcInstance npc, VmGothicEnums.PerceptionType perception, int function)
        {
            var props = GetProperties(npc);
            props.Perceptions[perception] = function;
        }

        public static void ExtNpcSetPerceptionTime(NpcInstance npc, float time)
        {
            var props = GetProperties(npc);
            props.perceptionTime = time;
        }

        public static void ExtNpcSetTalentValue(NpcInstance npc, VmGothicEnums.Talent talent, int level)
        {
            var props = GetProperties(npc);
            props.Talents[talent] = level;
        }

        public static void ExtCreateInvItems(NpcInstance npc, uint itemId, int amount)
        {
            var props = GetProperties(npc);

            props.Items.TryAdd(itemId, amount);
            props.Items[itemId] += amount;
        }
        
        public static void ExtEquipItem(NpcInstance npc, int itemId)
        {
            var props = GetProperties(npc);
            var itemData = AssetCache.TryGetItemData(itemId);

            props.EquippedItems.Add(itemData);
        }
    }
}
