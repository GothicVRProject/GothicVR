using System;
using System.Collections.Generic;
using GVR.Extensions;
using GVR.Phoenix.Data;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Properties;
using GVR.Vob.WayNet;
using PxCs.Data.WayNet;
using PxCs.Interface;
using UnityEngine.SceneManagement;

namespace GVR.Phoenix.Interface
{
    public static class GameData
    {
        public static IntPtr VfsPtr;
        public static IntPtr VmGothicPtr;
        public static IntPtr VmSfxPtr; // Sound FX
        public static IntPtr VmPfxPtr; // Particle FX
        public static IntPtr VmMusicPtr;

        private static WorldData worldInternal;
        public static WorldData World
        {
            get => worldInternal;
            set
            {
                worldInternal = value;
                if (value != null)
                    SetWayPointData(value.waypoints);
            }
        }

        public static readonly Dictionary<string, WayPoint> WayPoints = new();
        public static readonly Dictionary<string, FreePoint> FreePoints = new();
        public static readonly List<VobProperties> VobsInteractable = new(); 
        
        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public static Dictionary<IntPtr, List<RoutineData>> npcRoutines = new();

        public static Scene? WorldScene;

        public static void Reset()
        {
            World = null;
            WayPoints.Clear();
            FreePoints.Clear();
            VobsInteractable.Clear();
        }

        public static void Dispose()
        {
            if (VfsPtr != IntPtr.Zero)
            {
                PxVfs.pxVfsDestroy(VfsPtr);
                VfsPtr = IntPtr.Zero;
            }

            if (VmGothicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(VmGothicPtr);
                VmGothicPtr = IntPtr.Zero;
            }

            if (VmSfxPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(VmSfxPtr);
                VmSfxPtr = IntPtr.Zero;
            }

            if (VmPfxPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(VmPfxPtr);
                VmPfxPtr = IntPtr.Zero;
            }

            if (VmMusicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(VmMusicPtr);
                VmMusicPtr = IntPtr.Zero;
            }
        }

        private static void SetWayPointData(PxWayPointData[] wayPoints)
        {
            WayPoints.Clear();
            foreach (var wp in wayPoints)
            {
                WayPoints.Add(wp.name, new ()
                {
                    Name = wp.name,
                    Position = wp.position.ToUnityVector()
                });
            }
        }
    }
}
