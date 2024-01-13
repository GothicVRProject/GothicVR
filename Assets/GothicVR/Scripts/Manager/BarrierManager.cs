using GVR.Caches;
using GVR.Creator;
using GVR.Util;
using UnityEngine;

namespace GVR.GothicVR.Scripts.Manager
{
    public class BarrierManager : SingletonBehaviour<BarrierManager>
    {
        public void CreateBarrier()
        {
            var barrierMesh = AssetCache.TryGetMsh("MAGICFRONTIER_OUT.MSH");
            MeshObjectCreator.CreateBarrier("Barrier", barrierMesh, Vector3.zero, Quaternion.identity);
        }
    }
}