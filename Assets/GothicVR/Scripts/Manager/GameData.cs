using System;
using System.Collections.Generic;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Properties;
using GVR.Util;
using GVR.Vob.WayNet;
using PxCs.Data.WayNet;
using PxCs.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Phoenix.Interface
{
    public class GameData : SingletonBehaviour<GameData>
    {
        public IntPtr VfsPtr;
        public IntPtr VmGothicPtr;
        public IntPtr VmSfxPtr;
        public IntPtr VmMusicPtr;

        private WorldData worldInternal;
        public WorldData World
        {
            get => worldInternal;
            set
            {
                worldInternal = value;
                if (value != null)
                    SetWayPointData(value.waypoints);
            }
        }

        public readonly Dictionary<string, WayPoint> WayPoints = new();
        public readonly Dictionary<string, FreePoint> FreePoints = new();
        public readonly List<VobProperties> VobsInteractable = new(); 
        
        public TMP_FontAsset EmptyFont;

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public Dictionary<IntPtr, List<RoutineData>> npcRoutines = new();

        public Scene? WorldScene;


        protected override void Awake()
        {
            base.Awake();
            
            GvrSceneManager.StartWorldLoading.AddListener(delegate
            {
                World = null;
                WayPoints.Clear();
                FreePoints.Clear();
                VobsInteractable.Clear();
            });
        }

        private void SetWayPointData(PxWayPointData[] wayPoints)
        {
            foreach (var wp in wayPoints)
            {
                WayPoints.Add(wp.name, new ()
                {
                    Name = wp.name,
                    Position = wp.position.ToUnityVector()
                });
            }
        }
        
        // FIXME: This destructor is called multiple times when starting Unity game (Also during start of game)
        // FIXME: We need to check why and improve!
        // Destroy memory on phoenix DLL when game closes.
        ~GameData()
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
            
            if (VmMusicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(VmMusicPtr);
                VmMusicPtr = IntPtr.Zero;
            }
        }
    }
}