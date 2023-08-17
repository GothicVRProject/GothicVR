using System;
using System.Collections.Generic;
using TMPro;
using GVR.Phoenix.Data;
using GVR.Phoenix.Data.Vm.Gothic;
using GVR.Util;
using PxCs.Data.WayNet;
using PxCs.Interface;
using UnityEngine.SceneManagement;

namespace GVR.Phoenix.Interface
{
    public class GameData : SingletonBehaviour<GameData>
    {
        public IntPtr VfsPtr;
        public IntPtr VmGothicPtr;
        public IntPtr VmSfxPtr;
        public IntPtr VmMusicPtr;

        public WorldData World;

        // FIXME Find a better place for the NPC routines. E.g. on the NPCs itself? But we e.g. need to have a static NPCObject List to do so.
        public Dictionary<IntPtr, List<RoutineData>> npcRoutines = new();

        public Scene? WorldScene;

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