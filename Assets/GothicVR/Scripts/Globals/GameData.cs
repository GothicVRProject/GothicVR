using System.Collections.Generic;
using GVR.Extensions;
using GVR.Phoenix.Data;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Properties;
using GVR.Vob.WayNet;
using GVR.World;
using UnityEngine.SceneManagement;
using ZenKit;
using WayPoint = GVR.Vob.WayNet.WayPoint;

namespace GVR.Globals
{
    public static class GameData
    {
        public static Vfs Vfs;
        public static DaedalusVm GothicVm;
        public static DaedalusVm SfxVm; // Sound FX
        public static DaedalusVm PfxVm; // Particle FX
        public static DaedalusVm MusicVm;

        private static WorldData worldInternal;
        public static WorldData World
        {
            get => worldInternal;
            set
            {
                worldInternal = value;
                if (value != null)
                    SetWayPointData(value.WayNet.Points);
            }
        }

        public static readonly Dictionary<string, WayPoint> WayPoints = new();
        public static readonly Dictionary<string, FreePoint> FreePoints = new();
        // Reorganized waypoints from world data.
        public static Dictionary<string, DijkstraWaypoint> DijkstraWaypoints = new();
        public static readonly List<VobProperties> VobsInteractable = new(); 
        
        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<int, List<RoutineData>> npcRoutines = new();

        public static Scene? WorldScene;

        public static void Reset()
        {
            World = null;
            WayPoints.Clear();
            FreePoints.Clear();
            VobsInteractable.Clear();
        }


        private static void SetWayPointData(List<IWayPoint> wayPoints)
        {
            WayPoints.Clear();
            foreach (var wp in wayPoints)
            {
                WayPoints.Add(wp.Name, new ()
                {
                    Name = wp.Name,
                    Position = wp.Position.ToUnityVector()
                });
            }
        }

        public static void Dispose()
        {
            // Needs to be reset as Unity won't clear static variables when closing game in EditorMode.
            Vfs = null;
            World = null;
            GothicVm = null;
            SfxVm = null;
            MusicVm = null;
        }
    }
}
